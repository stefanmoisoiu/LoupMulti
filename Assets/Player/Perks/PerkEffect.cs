using Game.Common;
using Player.Networking;
using Player.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Perks
{
    public abstract class PerkEffect : PNetworkBehaviour
    {
        public bool Applied { get; private set; } = false;

        public Item Item { get; private set; }
        public PlayerReferences PlayerReferences { get; private set; }

        public void Initialize(Item itemData, PlayerReferences playerReferences)
        {
            Item = itemData;
            PlayerReferences = playerReferences;
        }

        public void SetApplied(bool applied)
        {
            if (applied == Applied) return;
            Debug.Log($"SetApplied: {applied} for {Item.Info.Name}");
            
            if (applied)
                StartApply();
            else
                StopApply();
            Applied = applied;
        }

        internal abstract void StartApply();
        internal abstract void StopApply();
    }
}