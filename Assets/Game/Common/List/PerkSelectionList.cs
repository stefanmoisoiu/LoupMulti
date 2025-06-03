using System;
using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common.List
{
    [CreateAssetMenu(fileName = "PerkSelectionList", menuName = "Game/Upgrade/Perk/Perk Selection List")]
    public class PerkSelectionList : ScriptableObjectSingleton<PerkSelectionList>
    {
        [AssetList(AutoPopulate = false)] 
        public PerkData[] perks;
        public PerkData GetPerk(ushort perkIndex)
        {
            if (perkIndex >= perks.Length) return null;
            return perks[perkIndex];
        }
        public ushort GetPerk(PerkData perkData)
        {
            return (ushort)Array.IndexOf(perks, perkData);
        }
    }
}