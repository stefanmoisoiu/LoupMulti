using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings")]
    public class GameSettings : ScriptableObjectSingleton<GameSettings>
    {
        [BoxGroup("Player Settings")]
        [SerializeField] private ushort playerMaxHealth = 100;
        public ushort PlayerMaxHealth => playerMaxHealth;
        [BoxGroup("Item Settings")]
        [SerializeField] private ushort baseCarouselChoices = 3;
        public ushort BaseCarouselChoices => baseCarouselChoices;
        [BoxGroup("Item Settings")]
        [SerializeField] private ushort startingRerolls = 2;
        public ushort StartingRerolls => startingRerolls;
        [BoxGroup("Item Settings")]
        [SerializeField] private ushort rareResourceRerollCost = 1;
        public ushort RareResourceRerollCost => rareResourceRerollCost;
        [BoxGroup("Item Settings")]
        [SerializeField] private int maxItemLevel = 20;
        public int MaxItemLevel => maxItemLevel;
        
        [BoxGroup("Ability Settings")]
        [SerializeField] private int abilitySlots = 3;
        public int AbilitySlots => abilitySlots;
        
        [BoxGroup("Round Settings")]
        [SerializeField] private float collectRoundLength = 60f;
        [BoxGroup("Round Settings")]
        [SerializeField] private float upgradeRoundLength = 15f;

        
        
        [BoxGroup("DEBUG")]
        [SerializeField] private bool debug;
        [BoxGroup("DEBUG")] [Space]
        [SerializeField] private bool debugRoundLengths;
        [BoxGroup("DEBUG")]
        [SerializeField] private float debugCollectRoundLength = 10f;
        [BoxGroup("DEBUG")]
        [SerializeField] private float debugUpgradeRoundLength = 10f;
        
        public float CollectRoundLength => debugRoundLengths ? debugCollectRoundLength : collectRoundLength;
        public float UpgradeRoundLength => debugRoundLengths ? debugUpgradeRoundLength : upgradeRoundLength;

        public bool NeverStopGame => debug;
    }
}