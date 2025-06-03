using System;
using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common.List
{
    [CreateAssetMenu(fileName = "Perk List", menuName = "Game/Upgrade/Perk/Perk List")]
    public class PerkList : ScriptableObjectSingleton<PerkList>
    {
        [AssetList(AutoPopulate = true)] 
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