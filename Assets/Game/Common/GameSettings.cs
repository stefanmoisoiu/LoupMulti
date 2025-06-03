using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings")]
    public class GameSettings : ScriptableObjectSingleton<GameSettings>
    {
        [TitleGroup("Perk Selection")]
        [SerializeField] private int maxPerks = 64;
        public int MaxPerks => maxPerks;
        [TitleGroup("Perk Selection")]
        [SerializeField] private int perkChoices = 3;
        public int PerkChoices => perkChoices;
        [TitleGroup("Shop")]
        [SerializeField] private int maxShopItems = 64;
        public int MaxShopItems => maxShopItems;
       
        public const ushort PlayerMaxHealth = 100;

        [SerializeField] private bool debug;

        public bool DebugMode => debug;
    }
}