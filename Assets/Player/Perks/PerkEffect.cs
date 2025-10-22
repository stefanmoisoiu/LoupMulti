using Game.Common;
using Player.Networking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Perks
{
    public abstract class PerkEffect : PNetworkBehaviour
    {
        private bool applied = false;
        public bool Applied => applied;

        public Item Item { get; private set; }

        public void Initialize(Item itemData)
        {
            Item = itemData;
        }

        public void SetApplied(bool applied)
        {
            if (applied == this.applied) return;
            Debug.Log($"SetApplied: {applied} for {Item.Info.Name}");
            
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