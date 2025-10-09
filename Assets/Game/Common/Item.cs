using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
    public class Item : ScriptableObject
    {
        // public ItemType Type;
        
        [InlineEditor]
        public ObjectInfo Info;
        [InlineEditor]
        public ShopItemData ShopItemData;
        [InlineEditor]
        public PerkData PerkData;
        [InlineEditor]
        public AbilityData AbilityData;
        
        // public enum ItemType
        // {
        //     ShopItem,
        //     Perk,
        // }
    }
}