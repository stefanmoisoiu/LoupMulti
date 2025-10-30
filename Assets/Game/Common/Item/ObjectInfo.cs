using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Common
{
    [Serializable]
    public class ObjectInfo
    {
#pragma warning disable CS0108, CS0114
        [TitleGroup("Information")] [HorizontalGroup("Information/0")] [VerticalGroup("Information/0/0")] [SerializeField] private string name;
        [TitleGroup("Information")] [SerializeField] [VerticalGroup("Information/0/0")]  private ObjectRarity rarity = ObjectRarity.Common;
        [PreviewField(ObjectFieldAlignment.Right, Height = 64)] [TitleGroup("Information")] [HorizontalGroup("Information/0")] [SerializeField] private Sprite icon;
#pragma warning restore CS0108, CS0114
        [TitleGroup("Information")] [SerializeField] [TextArea] private string description;
        
        public string Name => name;
        public string Description => description;
        public Sprite Icon => icon;
        public ObjectRarity Rarity => rarity;
        
        public override string ToString()
        {
            return $"{Name} ({Rarity}): {Description}";
        }
    }
    
    public enum ObjectRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}