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

        public void UpdateInfo(OwnedItemData ownedItemData, PlayerReferences playerReferences)
        {
            PlayerReferences = playerReferences;
            OwnedItemData = ownedItemData;
            Item = ItemRegistry.Instance.GetItem(ownedItemData.ItemRegistryIndex);
        }

        public abstract void EnablePerk();
        public abstract void DisablePerk();

        public void SetPerkEnabled(bool value)
        {
            PerkEnabled = value;
            if (PerkEnabled) EnablePerk();
            else DisablePerk();
        }
    }
}