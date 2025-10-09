using System;
using Base_Scripts;
using Game.Common.List;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Game Registry", menuName = "Game/Item Registry")]
    public class ItemRegistry : ScriptableObjectSingleton<ItemRegistry>
    {
        [AssetList(AutoPopulate = true)]
        [SerializeField] private Item[] items;
        public Item GetItem(ushort index) => items[index];
        public ushort GetItem(Item item) => (ushort)System.Array.IndexOf(items, item);

        [SerializeField] private ShopCategory[] shopCategories;
        public ShopCategory[] ShopCategories => shopCategories;
        public ShopCategory GetShopCategory(ShopCategory.CategoryType type) => Array.Find(shopCategories, c => c.Type == type);

        [AssetSelector]
        [SerializeField] private Item[] itemsInSelection;
        public Item[] ItemsInSelection => itemsInSelection;
    }
    [Serializable]
    public struct ShopCategory
    {
        public CategoryType Type;
        [AssetSelector]
        public Item[] Items;
        
        public enum CategoryType
        {
            Main
        }
    }
}