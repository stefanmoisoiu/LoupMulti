using Game.Common;
using Game.Common.List;
using Game.Upgrade.Shop;
using Player.UI.Shop.ShopSelectionInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Player.UI.Shop
{
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] private ShopInfoBubbleTarget shopInfoBubbleTarget;
        private ShopItemData shopItemData;
        public ShopItemData ShopItemData => shopItemData;
        [SerializeField] private Button button;
        [SerializeField] private Image image;

        private void Start()
        {
            ShopManager.OnShopItemBought += ShopItemBought;
        }

        private void OnDestroy()
        {
            ShopManager.OnShopItemBought -= ShopItemBought;
        }

        public void SetShopItem(ShopItemData newShopItemData, ShopManager shopManager)
        {
            image.sprite = newShopItemData.Icon;
            shopItemData = newShopItemData;
            shopInfoBubbleTarget.SetData(newShopItemData.ItemName, newShopItemData.ItemDescription);
            button.onClick.AddListener(() => shopManager.BuyShopItem(newShopItemData));
        }
        
        private void ShopItemBought(ushort shopItemIndex)
        {
            if (shopItemData.MultipleAllowed) return;
            if (ShopItemList.Instance.GetShopItem(shopItemData) != shopItemIndex) return;
            button.interactable = false;
        }
    }
}