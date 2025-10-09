using Game.Common;
using Player.Networking;
using UnityEngine;

namespace Player.Perks
{
    public abstract class PerkEffect : PNetworkBehaviour{
        
        private bool applied = false;
        public bool Applied => applied;

        [SerializeField] private PerkData associatedPerkData;
        public PerkData Data => associatedPerkData;
        
        public void SetApplied(bool applied)
        {
            if (applied == this.applied) return;
            Debug.Log($"SetApplied: {applied} for {name}");
            
            if (applied)
                StartApply();
            else
                StopApply();
            this.applied = applied;
        }

        internal abstract void StartApply();
        internal abstract void StopApply();
    }
}