using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Perk", menuName = "Game/Upgrade/Perk/Perk")]
    public class PerkData : ScriptableObject
    {
        [TitleGroup("Base")]
        [FoldoutGroup("Base/Info")][SerializeField] private Sprite perkIcon;
        [FoldoutGroup("Base/Info")][SerializeField] private string perkName;
        [FoldoutGroup("Base/Info")][SerializeField][TextArea] private string perkDescription;
        [FoldoutGroup("Base/Info")][SerializeField] private PerkRarity perkRarity;
        [FoldoutGroup("Base/Info")][SerializeField] private GameObject perkEffectPrefab;
        
        
        public enum PerkRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
        
        public Sprite PerkIcon => perkIcon;
        public string PerkName => perkName;
        public string PerkDescription => perkDescription;
        
        public PerkRarity Rarity => perkRarity;
        
        public GameObject PerkEffectPrefab => perkEffectPrefab;
        
        public override string ToString() =>
            $"PerkName: {perkName}\n" +
            $"PerkDescription: {perkDescription}\n" +
            $"Icon: {perkIcon}\n" +
            $"Rarity: {perkRarity}\n";
    }
}