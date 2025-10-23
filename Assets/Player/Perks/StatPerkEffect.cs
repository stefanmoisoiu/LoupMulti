using Player.Stats;
using UnityEngine;

namespace Player.Perks
{
    public class StatPerkEffect : PerkEffect
    {
        [SerializeField] private PlayerStatModifier playerStatModifier;
        
        internal override void StartApply()
        {
            playerStatModifier.Add(PlayerReferences.StatManager);
        }

        internal override void StopApply()
        {
            playerStatModifier.Remove(PlayerReferences.StatManager);
        }
    }
}
