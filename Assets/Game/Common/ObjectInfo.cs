using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Perk", menuName = "Game/Upgrade/Object Info")]
    public class ObjectInfo : ScriptableObject
    {
#pragma warning disable CS0108, CS0114
        [TitleGroup("Information")] [SerializeField] private string name;
#pragma warning restore CS0108, CS0114
        [TitleGroup("Information")] [SerializeField] [TextArea] private string description;
        [TitleGroup("Information")] [SerializeField] private Sprite icon;
        [TitleGroup("Information")] [SerializeField] private ObjectRarity rarity;
        
        public string Name => name;
        public string Description => description;
        public Sprite Icon => icon;
        public ObjectRarity Rarity => rarity;
        
        public enum ObjectRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
        
        public override string ToString()
                {
                    return $"{Name} ({Rarity}): {Description}";
                }
    }
}