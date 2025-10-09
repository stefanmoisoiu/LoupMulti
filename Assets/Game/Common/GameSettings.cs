using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings")]
    public class GameSettings : ScriptableObjectSingleton<GameSettings>
    {
        [BoxGroup("Player Settings")]
        [SerializeField] private int maxItems = 64;
        public int MaxItems => maxItems;
        
        [BoxGroup("Player Settings")]
        [SerializeField] private int itemSelectionChoices = 3;
        public int ItemSelectionChoices => itemSelectionChoices;
        
        [BoxGroup("Player Settings")]
        [SerializeField] private int abilitySlots = 3;
        public int AbilitySlots => abilitySlots;
       
        public const ushort PlayerMaxHealth = 100;
        
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

        public bool DebugMode => debug;
    }
}