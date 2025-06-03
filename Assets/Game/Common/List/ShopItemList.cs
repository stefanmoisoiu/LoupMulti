using System;
using AYellowpaper.SerializedCollections;
using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common.List
{
    [CreateAssetMenu(fileName = "Shop Item", menuName = "Game/Upgrade/Shop/Shop Item List")]
    public class ShopItemList : ScriptableObjectSingleton<ShopItemList>
    {
        [AssetList(AutoPopulate = true)] 
        public ShopItemData[] shopItems;
        public SerializedDictionary<string, ShopItemData[]> shopCategories;
        
        public ShopItemData GetShopItem(ushort shopItemIndex)
        {
            if (shopItemIndex >= shopItems.Length) return null;
            return shopItems[shopItemIndex];
        }
        public ushort GetShopItem(ShopItemData shopItemData) => (ushort)Array.IndexOf(shopItems, shopItemData);
    }
}