using System.Linq;
using Game.Common;
using Game.Data;
using Game.Upgrade.Shop;
using Player.General_UI.Shop.ShopSelectionInfo;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Player.General_UI.Shop
{
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] private ShopInfoBubbleTarget shopInfoBubbleTarget;
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
            shopInfoBubbleTarget.SetData(_item.Info.Name, _item.Info.Description, _item.ShopItemData.CostAmount, _item.ShopItemData.CostType);

            ushort[] ownedItems = DataManager.Instance[NetworkManager.Singleton.LocalClientId].inGameData.items;
            
            if (ownedItems.Contains(ItemRegistry.Instance.GetItem(_item))) DisableShopItemUI();
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
            shopInfoBubbleTarget.OnPointerExit();
        }
    }
}