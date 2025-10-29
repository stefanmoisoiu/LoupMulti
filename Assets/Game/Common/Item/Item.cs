using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
    public class Item : ScriptableObject
    {
        [BoxGroup("Object Information")] [SerializeField]
        private ObjectInfo info;
        public ObjectInfo Info => info;

        [BoxGroup("Shop Information")] [SerializeField]
        private bool isInShop = false;
        public bool IsInShop => isInShop;
        [BoxGroup("Shop Information")] [ShowIf("isInShop")] [SerializeField]
        private ShopCategory.CategoryType shopCategory = Common.ShopCategory.CategoryType.Main;
        public ShopCategory.CategoryType ShopCategory => shopCategory;
        [BoxGroup("Shop Information")] [ShowIf("isInShop")] [SerializeField]
        private ShopItemData shopItemData;
        public ShopItemData ShopItemData => shopItemData;
        public enum ItemType
        {
            Perk,
            Ability,
        }

        [BoxGroup("Carousel Information")]
        [SerializeField] private bool inCarousel = true;
        public bool InCarousel => inCarousel;
        [BoxGroup("Item Data")] [SerializeField]
        private ItemType itemType;
        public ItemType Type => itemType;
        

        [BoxGroup("Item Data")] [ShowIf("itemType", ItemType.Perk)] [SerializeField]
        private PerkData perkData;
        public PerkData PerkData => perkData;

        [BoxGroup("Item Data")] [ShowIf("itemType", ItemType.Ability)] [SerializeField]
        private AbilityData abilityData;
        public AbilityData AbilityData => abilityData;

        [Button("Rebuild Item Registry")]
        private void RebuildItemRegistry()
        {
            #if UNITY_EDITOR
            
            ItemRegistry registry = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemRegistry>("Assets/Resources/ItemRegistry.asset");
            registry.RebuildItemRegistry();
            
            #endif
        }
    }
}