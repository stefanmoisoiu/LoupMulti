using System;
using System.Collections.Generic;
using Game.Common;
using Game.Common.List;
using Game.Upgrade.Shop;
using Player.Networking;
using UnityEngine;

namespace Player.UI.Shop
{
    public class ShopCategory : PNetworkBehaviour
    {
        [SerializeField] private GameObject shopItemPrefab;
        
        private List<GameObject> shopItemObjects;

        [SerializeField] private string categoryTag;
        
        private Transform shopItemParent;

        protected override void StartAnyOwner()
        {
            shopItemParent = PCanvas.CanvasObjects[categoryTag].transform;
            ShopManager.OnShopOpenedChanged += ShopOpenedChangedChanged;
        }

        private void ShopOpenedChangedChanged(bool opened, ShopManager shopManager)
        {
            if (!opened) return;

            if (shopItemObjects == null) CreateShopItems(shopManager);
            
        }

        private void CreateShopItems(ShopManager shopManager)
        {
            ShopItemData[] shopItems = ShopItemList.Instance.shopCategories[categoryTag];
            shopItemObjects = new List<GameObject>(shopItems.Length);
            for (int i = 0; i < shopItems.Length; i++)
            {
                GameObject itemObject = Instantiate(shopItemPrefab, shopItemParent);
                if (!itemObject.TryGetComponent(out ShopItemUI ui)) throw new NullReferenceException("ShopItemUI component is missing on the shop item prefab.");
                ui.SetShopItem(shopItems[i], shopManager);
                shopItemObjects.Add(itemObject);
            }
        }
    }
}