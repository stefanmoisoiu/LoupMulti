    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Common;
    using Unity.Netcode;
    using UnityEngine;

    [Serializable]
    public struct InGameData : INetworkSerializable, IEquatable<InGameData>
    {
        public ushort health;
        public List<OwnedItemData> ownedItems;
        public OwnedResourcesData resources;
        public ushort rerollsAvailable;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref resources);
            serializer.SerializeValue(ref health);
            serializer.SerializeValue(ref rerollsAvailable);

            int count = 0;
            if (!serializer.IsReader && ownedItems != null) count = ownedItems.Count;
            serializer.SerializeValue(ref count);
            if (serializer.IsReader) ownedItems = new List<OwnedItemData>(count);
            for(int i = 0; i < count; i++)
            {
                var entry = serializer.IsReader ? default : ownedItems[i];
                entry.NetworkSerialize(serializer);
                if(serializer.IsReader) ownedItems.Add(entry);
            }
        }

        public InGameData(ushort health = ushort.MaxValue, ushort rerolls = ushort.MaxValue, List<OwnedItemData> ownedItems = null, OwnedResourcesData resources = new())
        {
            if (health == ushort.MaxValue) health = GameSettings.Instance.PlayerMaxHealth;
            if (health > GameSettings.Instance.PlayerMaxHealth) health = GameSettings.Instance.PlayerMaxHealth;
            
            if (rerolls == ushort.MaxValue) rerolls = GameSettings.Instance.StartingRerolls;
            
            this.health = health;
            this.rerollsAvailable = rerolls;
            this.ownedItems = ownedItems ?? new List<OwnedItemData>();
            this.resources = resources;
        }

        public InGameData(InGameData copy)
        {
            health = copy.health;
            rerollsAvailable = copy.rerollsAvailable;
            ownedItems = copy.ownedItems;
            resources = copy.resources;
        }

        public InGameData SetResources(OwnedResourcesData newResources) { resources = newResources; return this; }
        public InGameData AddHealth(ushort amount)
        {
            health = (ushort)Mathf.Min(GameSettings.Instance.PlayerMaxHealth, health + amount);
            return this;
        }
        public InGameData RemoveHealth(ushort amount)
        {
            health = (ushort)Mathf.Max(0, health - amount);
            return this;
        }
        public InGameData ResetHealth() { health = GameSettings.Instance.PlayerMaxHealth; return this; }
        public bool IsAlive() => health > 0;
        
        public InGameData UseReroll()
        {
            if (rerollsAvailable > 0) rerollsAvailable--;
            return this;
        }
        public InGameData AddReroll(ushort amount = 1)
        {
            rerollsAvailable += amount;
            return this;
        }

        public InGameData AddItem(ushort itemIndex, ushort level = 0)
        {
            ownedItems ??= new List<OwnedItemData>();
            ownedItems.Add(new OwnedItemData(itemIndex, level));
            return this;
        }

        public InGameData UpgradeItem(ushort itemIndex, ushort upgradeAmount = 1)
        {
            for (int i = 0; i < ownedItems.Count; i++)
            {
                if (ownedItems[i].ItemRegistryIndex != itemIndex) continue;
                if (ownedItems[i].Level >= GameSettings.Instance.MaxItemLevel) return this;
                OwnedItemData updatedItem = ownedItems[i];
                updatedItem.Level += upgradeAmount;
                ownedItems[i] = updatedItem;
                return this;
            }
            Debug.LogError($"Tried to upgrade item {itemIndex} but it was not owned.");
            return this;
        }

        public bool HasItem(ushort itemIndex)
        {
             ownedItems ??= new List<OwnedItemData>();
             return ownedItems.Any(item => item.ItemRegistryIndex == itemIndex);
        }

        public bool Equals(InGameData other)
        {
            if (health != other.health) return false;
            if (!resources.Equals(other.resources)) return false;
            if (rerollsAvailable != other.rerollsAvailable) return false;
            if ((ownedItems?.Count ?? 0) != (other.ownedItems?.Count ?? 0)) return false;
            if (ownedItems.Except(other.ownedItems).Any()) return false;
            
            return true;
        }
        public override bool Equals(object obj) => obj is InGameData other && Equals(other);
        public override int GetHashCode()
        {
             int listHash = 0;
             if (ownedItems != null)
                 foreach (var item in ownedItems) listHash ^= item.GetHashCode();
             return HashCode.Combine(health, resources, listHash);
        }
    }