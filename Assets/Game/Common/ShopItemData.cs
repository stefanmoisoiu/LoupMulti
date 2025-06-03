using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Shop Item", menuName = "Game/Upgrade/Shop/Shop Item")]
    public class ShopItemData : ScriptableObject
    {
    
        [TitleGroup("Base")] [SerializeField] private string itemName;
        public string ItemName => itemName;
        [TitleGroup("Base")] [TextArea][SerializeField] private string itemDescription;
        public string ItemDescription => itemDescription;
        [TitleGroup("Base")] [SerializeField] private Sprite icon;
        public Sprite Icon => icon;
        [TitleGroup("Base")] [SerializeField] private bool multipleAllowed = true;
        public bool MultipleAllowed => multipleAllowed;
    
    
        [TitleGroup("Price")] [SerializeField] private ResourceType costType;
        public ResourceType CostType => costType;
        [TitleGroup("Price")] [SerializeField] private int costAmount;
        public int CostAmount => costAmount;
        
        public bool HasEnoughResources(PlayerData data)
        {
            int value = costType == ResourceType.Common ? data.inGameData.resources.commonAmount 
                : data.inGameData.resources.rareAmount;
            return value >= costAmount;
        }
        
        [TitleGroup("Effect")] [SerializeField] [InlineEditor]
        private PerkData perkData;
        public PerkData PerkData => perkData;
    }
}
  