using Game.Common;
using Player.Networking;
using UnityEngine;

namespace Player.Perk_Effects
{
    public abstract class PerkEffect : PNetworkBehaviour{
        
        private bool applied = false;
        public bool Applied => applied;

        [SerializeField] private PerkData associatedPerkData;
        public PerkData Data => associatedPerkData;
        
        public void SetApplied(bool applied, PlayerReferences playerReferences, int perkCount = 1)
        {
            if (applied == this.applied) return;
            Debug.Log($"SetApplied: {applied}");
            
            if (applied)
                StartApply(playerReferences, perkCount);
            else
                StopApply(playerReferences);
            this.applied = applied;
        }

        internal abstract void StartApply(PlayerReferences playerReferences, int perkCount = 1);
        internal abstract void StopApply(PlayerReferences playerReferences);
    }
}