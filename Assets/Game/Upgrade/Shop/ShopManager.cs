using System;
using Base_Scripts;
using Game.Common;
using Game.Common.List;
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
        [Rpc(SendTo.Everyone)] private void OnShopOpenedChangedRpc(bool opened) => OnShopOpenedChanged?.Invoke(opened, this);
        
        public static event Action<ushort> OnShopItemBought;
        [Rpc(SendTo.SpecifiedInParams)] private void OnShopItemBoughtClientRpc(ushort shopItemIndex, RpcParams @params) => OnShopItemBought?.Invoke(shopItemIndex);
        
        public void SetOpened(bool opened)
        {
            if (shopOpened == opened) return;
            shopOpened = opened;

            OnShopOpenedChangedRpc(opened);

            NetcodeLogger.Instance.LogRpc(opened ? "Shop opened" : "Shop closed", NetcodeLogger.LogType.GameLoop);
        }
        
        public void BuyShopItem(ShopItemData shopItemData)
        {
            Debug.Log($"Buying shop item {shopItemData}");
            if (!shopItemData.HasEnoughResources(DataManager.Instance[NetworkManager.LocalClientId]))
            {
                Debug.LogWarning("Not enough resources to buy this item.");
                return;
            }
            
            BuyShopItemServerRpc(ShopItemList.Instance.GetShopItem(shopItemData), NetworkManager.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void BuyShopItemServerRpc(ushort shopItemDataInd, ulong clientId)
        {
            Debug.Log($"BuyShopItemServerRpc: {shopItemDataInd} for {clientId}");
            ShopItemData shopItemData = ShopItemList.Instance.GetShopItem(shopItemDataInd);
            if (!shopItemData.HasEnoughResources(DataManager.Instance[NetworkManager.LocalClientId]))
            {
                NetcodeLogger.Instance.LogRpc($"Not enough resources to buy {shopItemData.ItemName} for {clientId}", NetcodeLogger.LogType.GameLoop);
                return;
            }
            
            PlayerData data = DataManager.Instance[clientId];
            InGameData ingData = data.inGameData.AddShopItem(shopItemDataInd).AddPerk(PerkList.Instance.GetPerk(shopItemData.PerkData));
            DataManager.Instance[clientId] = new(data) {inGameData = ingData};
            
            OnShopItemBoughtClientRpc(shopItemDataInd, data.SendRpcTo());
        }
    }
}