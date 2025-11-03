using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Data;
using Game.Upgrade.Shop;
using Player.General_UI.Tooltips;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Player.General_UI.Shop
{
    public class ShopItemUI : MonoBehaviour, IInfoTooltipDataProvider
    {
        private Item _item;
        [SerializeField] private Button button;
        [SerializeField] private Image image;

        private void Start()
        {
            ShopManager.OnShopItemBoughtOwner += ShopItemBoughtOwner;
        }

        private void OnDestroy()
        {
            ShopManager.OnShopItemBoughtOwner -= ShopItemBoughtOwner;
        }

        public void SetShopItem(Item newItem, ShopManager shopManager)
        {
            _item = newItem;
            image.sprite = _item.Info.Icon;

            List<OwnedItemData> ownedItems = DataManager.Instance[NetworkManager.Singleton.LocalClientId].inGameData.ownedItems;
            
            if (ownedItems.Any(x => x.ItemRegistryIndex == ItemRegistry.Instance.GetItem(_item))) DisableShopItemUI();
            else button.onClick.AddListener(() => shopManager.BuyShopItem(_item));
        }

        private void OnDisable()
        {
            button?.onClick.RemoveAllListeners();
        }

        private void ShopItemBoughtOwner(ushort boughtItemInd)
        {
            ushort myItemInd = ItemRegistry.Instance.GetItem(_item);
            if (myItemInd != boughtItemInd) return;
            DisableShopItemUI();
        }

        private void DisableShopItemUI()
        {
            button.interactable = false;
        }

        public string GetHeaderText()
        {
            return _item.Info.Name;
        }

        public string GetDescriptionText()
        {
            return _item.Info.Description;
        }

        public Sprite GetMainIcon()
        {
            return _item.Info.Icon;
        }

        public bool ShouldShowPrice(out int price, out ResourceType resourceType)
        {
            price = _item.ShopItemData.CostAmount;
            resourceType = _item.ShopItemData.CostType;
            return true;
        }

        public Color GetHeaderColor()
        {
            return Color.white;
        }
    }
}