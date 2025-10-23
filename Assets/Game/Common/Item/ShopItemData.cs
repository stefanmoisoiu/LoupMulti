using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [Serializable]
    public class ShopItemData
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
    }
}
  