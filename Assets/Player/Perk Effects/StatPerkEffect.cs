using Player.Stats;
using Unity.Netcode;
using UnityEngine;

namespace Player.Perk_Effects
{
    public class StatPerkEffect : PerkEffect
    {
        [SerializeField] private PlayerStatComponent statComponent;
        private int _perkCount = 1;
        private PlayerStatComponent _statComponent;
    
        internal override void StartApply(PlayerReferences playerReferences, int perkCount = 1)
        {
            _perkCount = perkCount;

            statComponent.Power(perkCount);
            Debug.Log(name);
            statComponent.Add();
        }

        internal override void StopApply(PlayerReferences playerReferences)
        {
            statComponent.Remove();
        }
    }
}
