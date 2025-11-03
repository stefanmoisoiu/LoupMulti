using System;
using Game.Common;
using Player.Networking;
using Player.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Perks
{
    public abstract class PerkEffect : PNetworkBehaviour
    {
        public bool PerkEnabled { get; private set; } = false;
        public OwnedItemData OwnedItemData { get; private set; }
        public Item Item { get; private set; }
        public PlayerReferences PlayerReferences { get; private set; }
        
        private void Awake()
        {
            PlayerReferences ??= GetComponentInParent<PlayerReferences>();
        }

        public void UpdateInfo(OwnedItemData ownedItemData)
        {
            OwnedItemData = ownedItemData;
            Item = ItemRegistry.Instance.GetItem(ownedItemData.ItemRegistryIndex);
        }

        /// <summary>
        /// Enables the perk on OWNER & SERVER ! (Called only once if both)
        /// </summary>
        public abstract void EnablePerk();
        /// <summary>
        /// Disables the perk on OWNER & SERVER ! (Called only once if both)
        /// </summary>
        public abstract void DisablePerk();

        public void SetPerkEnabled(bool value)
        {
            if (PerkEnabled == value) return;
            PerkEnabled = value;
            if (PerkEnabled) EnablePerk();
            else DisablePerk();
        }
    }
}