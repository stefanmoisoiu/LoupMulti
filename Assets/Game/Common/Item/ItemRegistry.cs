using System;
using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Game Registry", menuName = "Game/Item Registry")]
    public class ItemRegistry : ScriptableObjectSingleton<ItemRegistry>
    {
        [Title("Master List")]
        [Tooltip("Liste auto-populée de TOUS les items du projet. C'est la source de vérité.")]
        [AssetList(AutoPopulate = true)]
        [SerializeField] private Item[] items;
        public Item[] Items => items;
        
        public Item GetItem(ushort index) => items[index];
        public ushort GetItem(Item item) => (ushort)System.Array.IndexOf(items, item);

        [SerializeField] private Item drillItem;
        public Item DrillItem => drillItem;
        public ushort DrillItemIndex => GetItem(drillItem);

        [Title("Generated Lists")]
        [InfoBox("Ces listes sont générées automatiquement en cliquant sur le bouton 'Rebuild Lists' ci-dessous.")]
        
        [SerializeField] private ShopCategory[] shopCategories;
        public ShopCategory[] ShopCategories => shopCategories;
        public ShopCategory GetShopCategory(ShopCategory.CategoryType type) => Array.Find(shopCategories, c => c.Type == type);

        [SerializeField] private Item[] itemsInCarousel;
        public Item[] ItemsInCarousel => itemsInCarousel;
        

        
        [Button("Rebuild Item, Shop & Selection Lists", ButtonSizes.Large)]
        public void RebuildItemRegistry()
        {
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Game.Common.Item");
            
            List<Item> foundItems = new List<Item>();
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Item item = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(path);
                if (item != null)
                {
                    foundItems.Add(item);
                }
            }
            items = foundItems.OrderBy(item => item.name).ToArray(); 
            
            Debug.Log($"Master List 'items' rafraîchie : {items.Length} items trouvés."); 
            #endif
            
            if (items == null || items.Length == 0)
            {
                Debug.LogWarning("La liste 'items' est vide. Assure-toi qu'elle est auto-populée.");
                return;
            }
            
            Dictionary<ShopCategory.CategoryType, List<Item>> shopItemsByCategory = new Dictionary<ShopCategory.CategoryType, List<Item>>();
            
            foreach (ShopCategory category in shopCategories)
            {
                shopItemsByCategory[category.Type] = new List<Item>();
            }

            foreach (var item in items.Where(item => item.IsInShop))
            {
                if (shopItemsByCategory.ContainsKey(item.ShopCategory))
                {
                    shopItemsByCategory[item.ShopCategory].Add(item);
                }
            }

            for (int i = 0; i < shopCategories.Length; i++)
            {
                shopCategories[i].Items = shopItemsByCategory[shopCategories[i].Type].ToArray();
            }

            itemsInCarousel = items.Where(item => item.InCarousel).ToArray();
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
            
            Debug.Log("Listes du ItemRegistry reconstruites avec succès !");
        }
    }

    [Serializable]
    public struct ShopCategory
    {
        public CategoryType Type;
        
        // [AssetSelector] retiré car cette liste est maintenant générée
        public Item[] Items; 
        
        public enum CategoryType
        {
            Main
            // Tu peux ajouter d'autres catégories ici (ex: Armes, Armures, Consommables)
        }
    }
}