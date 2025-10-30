using System;
using Base_Scripts;
using Game.Common;
using Game.Data;
using Unity.Netcode;
using UnityEngine;

namespace Game.Upgrade.Shop
{
    public class ShopManager : NetworkBehaviour
    {
        private bool shopOpened = false;
        public bool ShopOpened => shopOpened;

        public static event Action<bool, ShopManager> OnShopOpenedChanged;

        [Rpc(SendTo.Everyone)]
        private void OnShopOpenedChangedRpc(bool opened) => OnShopOpenedChanged?.Invoke(opened, this);

        public static event Action<ushort> OnShopItemBoughtOwner;

        [Rpc(SendTo.SpecifiedInParams)]
        private void OnShopItemBoughtOwnerRpc(ushort itemInd, RpcParams @params) =>
            OnShopItemBoughtOwner?.Invoke(itemInd);

        public void SetOpened(bool opened)
        {
            if (shopOpened == opened) return;
            shopOpened = opened;
            
            if (opened)
                CursorManager.Instance.RequestCursorUnlock(this);
            else
                CursorManager.Instance.ReleaseCursorUnlock(this);

            OnShopOpenedChangedRpc(opened);

            NetcodeLogger.Instance.LogRpc(opened ? "Shop opened" : "Shop closed", NetcodeLogger.LogType.GameLoop);
        }

        public void BuyShopItem(Item item)
        {
            Debug.Log($"Buying shop item {item.Info.Name}");
            if (!item.ShopItemData.HasEnoughResources(DataManager.Instance[NetworkManager.LocalClientId]))
            {
                Debug.LogWarning("Not enough resources to buy this item.");
                return;
            }

            BuyShopItemServerRpc(ItemRegistry.Instance.GetItem(item), NetworkManager.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void BuyShopItemServerRpc(ushort itemInd, ulong clientId)
        {
            Item item = ItemRegistry.Instance.GetItem(itemInd);
            
            if (!item.ShopItemData.HasEnoughResources(DataManager.Instance[clientId]))
            {
                NetcodeLogger.Instance.LogRpc($"Not enough resources to buy {item.Info.Name} for client {clientId}",
                    NetcodeLogger.LogType.GameLoop);
                return;
            }

            PlayerData data = DataManager.Instance[clientId];
            InGameData ingData = data.inGameData;
            ingData = ingData.SetResources(ingData.resources.RemoveResource(item.ShopItemData.CostType, item.ShopItemData.CostAmount));
            ingData = ingData.AddItem(itemInd);
            DataManager.Instance[clientId] = new(data) { inGameData = ingData };

            OnShopItemBoughtOwnerRpc(itemInd, data.SendRpcTo());
        }
        public void BuyHealth(ushort cost, ushort healAmount = 10)
        {
            if (!DataManager.Instance[NetworkManager.LocalClientId].inGameData.resources.HasEnough(ResourceType.Common, cost))
            {
                Debug.LogWarning("Not enough resources to buy health.");
                return;
            }
            BuyHealthServerRpc(cost, NetworkManager.LocalClientId, healAmount);
        }
        [ServerRpc(RequireOwnership = false)]
        private void BuyHealthServerRpc(ushort cost, ulong clientId, ushort healAmount = 10)
        {
            PlayerData data = DataManager.Instance[clientId];
            InGameData ingData = data.inGameData;
            if (!ingData.resources.HasEnough(ResourceType.Common, cost))
            {
                NetcodeLogger.Instance.LogRpc($"Not enough resources to buy health for {clientId}",
                    NetcodeLogger.LogType.GameLoop);
                return;
            }

            ingData = ingData.AddHealth(healAmount);
            ingData = new(ingData)
            {
                resources = ingData.resources.RemoveResource(ResourceType.Common, cost)
            };
            DataManager.Instance[clientId] = new(data) { inGameData = ingData };
        }

        private void OnDisable()
        {
            CursorManager.Instance.ReleaseCursorUnlock(this);
        }
    }
}