using System;
using System.Collections.Generic;
using Game.Common;
using Game.Upgrade.Shop;
using Player.Networking;
using UnityEngine;

namespace Player.General_UI.Shop
{
    public class ShopCategoryUI : PNetworkBehaviour
    {
        [SerializeField] private GameObject shopItemPrefab;
        
        private List<GameObject> shopItemObjects;

        [SerializeField] private string categoryTag;
        
        [SerializeField] private ShopCategory.CategoryType type;
        
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
            Item[] items = ItemRegistry.Instance.GetShopCategory(type).Items;
            shopItemObjects = new List<GameObject>(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                GameObject itemObject = Instantiate(shopItemPrefab, shopItemParent);
                if (!itemObject.TryGetComponent(out ShopItemUI ui)) throw new NullReferenceException("ShopItemUI component is missing on the shop item prefab.");
                ui.SetShopItem(items[i], shopManager);
                shopItemObjects.Add(itemObject);
            }
        }
    }
}