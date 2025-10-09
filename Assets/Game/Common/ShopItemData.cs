using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Shop Item", menuName = "Game/Upgrade/Shop/Shop Item")]
    public class ShopItemData : ScriptableObject
    {
        [TitleGroup("Price")] [SerializeField] private ResourceType costType;
        public ResourceType CostType => costType;
        [TitleGroup("Price")] [SerializeField] private ushort costAmount;
        public ushort CostAmount => costAmount;
        
        public bool HasEnoughResources(PlayerData data)
        {
            int value = costType == ResourceType.Common ? data.inGameData.resources.commonAmount 
                : data.inGameData.resources.rareAmount;
            return value >= costAmount;
        }
        
        public enum ShopItemType
        {
            Perk,
            Ability
        }
        public ShopItemType shopItemType;
        
        [ShowIf("@shopItemType == ShopItemType.Perk")]
        [TitleGroup("Perk")] [InlineEditor]
        public PerkData perkData;
        
        [ShowIf("@shopItemType == ShopItemType.Ability")]
        [TitleGroup("Ability")] [InlineEditor]
        public AbilityData abilityData;
    }
}
  