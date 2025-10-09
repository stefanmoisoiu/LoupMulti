using Player.Stats;
using UnityEngine;

namespace Player.Perks
{
    public class StatPerkEffect : PerkEffect
    {
        [SerializeField] private PlayerStatComponent statComponent;
        internal override void StartApply()
        {
            statComponent.Add();
        }

        internal override void StopApply()
        {
            statComponent.Remove();
        }
    }
}
