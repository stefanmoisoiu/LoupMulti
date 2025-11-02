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
        
        private static List<GameObject> _shopItemObjects;

        [SerializeField] private string categoryTag;
        
        [SerializeField] private ShopCategory.CategoryType type;
        
        private Transform _shopItemParent;

        protected override void StartOnlineOwner()
        {
            _shopItemParent = PCanvas.CanvasObjects[categoryTag].transform;
            ShopManager.OnShopOpenedChanged += ShopOpenedChangedChanged;
        }
        
        protected override void DisableAnyOwner()
        {
            ShopManager.OnShopOpenedChanged -= ShopOpenedChangedChanged;
        }

        private void ShopOpenedChangedChanged(bool opened, ShopManager shopManager)
        {
            if (!opened) return;

            if (_shopItemObjects == null) CreateShopItems(shopManager);
            
        }

        private void CreateShopItems(ShopManager shopManager)
        {
            Item[] items = ItemRegistry.Instance.GetShopCategory(type).Items;
            _shopItemObjects = new List<GameObject>(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                GameObject itemObject = Instantiate(shopItemPrefab, _shopItemParent);
                if (!itemObject.TryGetComponent(out ShopItemUI ui)) throw new NullReferenceException("ShopItemUI component is missing on the shop item prefab.");
                ui.SetShopItem(items[i], shopManager);
                _shopItemObjects.Add(itemObject);
            }
        }
    }
}