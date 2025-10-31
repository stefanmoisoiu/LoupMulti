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
    public OwnedItemData[] equippedAbilities;
    public OwnedItemData ownedDrillData;
    public OwnedResourcesData resources;
    public ushort rerollsAvailable;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref resources);
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref rerollsAvailable);
        serializer.SerializeValue(ref ownedDrillData);

        int itemsCount = 0;
        if (!serializer.IsReader && ownedItems != null) itemsCount = ownedItems.Count;
        serializer.SerializeValue(ref itemsCount);
        if (serializer.IsReader) ownedItems = new List<OwnedItemData>(itemsCount);
        for(int i = 0; i < itemsCount; i++)
        {
            var entry = serializer.IsReader ? default : ownedItems[i];
            entry.NetworkSerialize(serializer);
            if(serializer.IsReader) ownedItems.Add(entry);
        }
        
        if (serializer.IsReader) equippedAbilities = new OwnedItemData[GameSettings.Instance.AbilitySlots];
        for (int i = 0; i < GameSettings.Instance.AbilitySlots; i++)
        {
            serializer.SerializeValue(ref equippedAbilities[i]);
        }
    }

    public InGameData(ushort health, ushort rerolls, List<OwnedItemData> ownedItems, OwnedItemData ownedDrillData, OwnedItemData[] equippedAbilities, OwnedResourcesData resources)
    {
        if (health == ushort.MaxValue) health = GameSettings.Instance.PlayerMaxHealth;
        if (health > GameSettings.Instance.PlayerMaxHealth) health = GameSettings.Instance.PlayerMaxHealth;
        
        if (rerolls == ushort.MaxValue) rerolls = GameSettings.Instance.StartingRerolls;
        
        this.health = health;
        this.rerollsAvailable = rerolls;
        this.ownedItems = ownedItems;
        this.equippedAbilities = equippedAbilities;
        this.ownedDrillData = ownedDrillData;
        this.resources = resources;
    }

    public InGameData(InGameData copy)
    {
        health = copy.health;
        rerollsAvailable = copy.rerollsAvailable;
        resources = copy.resources;
        ownedDrillData = copy.ownedDrillData;
        
        ownedItems = new List<OwnedItemData>(copy.ownedItems);
        
        equippedAbilities = new OwnedItemData[GameSettings.Instance.AbilitySlots];
        if (copy.equippedAbilities != null)
        {
            Array.Copy(copy.equippedAbilities, equippedAbilities, GameSettings.Instance.AbilitySlots);
        }
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
        OwnedItemData data = new OwnedItemData(itemIndex, level);
        ownedItems.Add(data);
        
        if (ItemRegistry.Instance.GetItem(itemIndex).Type == Item.ItemType.Ability) this = EquipAbilityInFirstSlot(data);
        
        return this;
    }

    public InGameData EquipAbilityInFirstSlot(OwnedItemData abilityData)
    {
        for (ushort i = 0; i < equippedAbilities.Length; i++)
        {
            if (equippedAbilities[i].IsEmpty())
            {
                equippedAbilities[i] = abilityData;
                return this;
            }
        }
        Debug.LogError("Tried to equip ability in first slot but all slots are taken");
        return this;
    }

    public InGameData UpgradeItem(ushort itemIndex, ushort upgradeAmount = 1)
    {
        if (itemIndex == ownedDrillData.ItemRegistryIndex)
        {
            ownedDrillData.Level += upgradeAmount;
            return this;
        }
        
        for (int i = 0; i < ownedItems.Count; i++)
        {
            if (ownedItems[i].ItemRegistryIndex != itemIndex) continue;
            if (ownedItems[i].Level >= GameSettings.Instance.MaxItemLevel) return this;
            
            OwnedItemData updatedItem = ownedItems[i];
            updatedItem.Level += upgradeAmount;
            ownedItems[i] = updatedItem;
            
            UpdateEquippedItem(updatedItem);
            return this;
        }
        Debug.LogError($"Tried to upgrade item {itemIndex} but it was not owned.");
        return this;
    }
    
    private void UpdateEquippedItem(OwnedItemData updatedItem)
    {
        if (equippedAbilities == null) return;
        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            if (!equippedAbilities[i].IsEmpty() && equippedAbilities[i].ItemRegistryIndex == updatedItem.ItemRegistryIndex)
            {
                equippedAbilities[i] = updatedItem;
                return;
            }
        }
    }

    public bool HasItem(ushort itemIndex)
    {
         ownedItems ??= new List<OwnedItemData>();
         return ownedItems.Any(item => item.ItemRegistryIndex == itemIndex) || itemIndex == ownedDrillData.ItemRegistryIndex;
    }

    public OwnedItemData GetOwnedItem(ushort itemIndex)
    {
        if (itemIndex == ownedDrillData.ItemRegistryIndex)
        {
            return ownedDrillData;
        }
        return ownedItems.First(item => item.ItemRegistryIndex == itemIndex);
    }

    public List<OwnedItemData> GetAllOwnedItems()
    {
        List<OwnedItemData> res = new(ownedItems);
        res.Add(ownedDrillData);
        return res;
    }
    public List<OwnedItemData> GetAllAbilities() => GetAllOwnedItems().FindAll(itemData => ItemRegistry.Instance.GetItem(itemData.ItemRegistryIndex).Type == Item.ItemType.Ability);
    public List<OwnedItemData> GetAllPerks() => GetAllOwnedItems().FindAll(itemData => ItemRegistry.Instance.GetItem(itemData.ItemRegistryIndex).Type == Item.ItemType.Perk);
    
    public InGameData EquipAbility(ushort slot, ushort itemRegistryIndex)
    {
        if (slot >= GameSettings.Instance.AbilitySlots) throw new ArgumentException("Slot index is out of bounds");
        if(!HasItem(itemRegistryIndex)) throw new ArgumentException("Item is not owned");
        
        OwnedItemData item = GetOwnedItem(itemRegistryIndex);
        equippedAbilities[slot] = item;
        return this;
    }

    public InGameData UnequipAbility(ushort slot)
    {
        if (slot >= GameSettings.Instance.AbilitySlots) throw new ArgumentException("Slot index is out of bounds");
        equippedAbilities[slot] = OwnedItemData.Empty;
        return this;
    }

    public bool Equals(InGameData other)
    {
        if (health != other.health) return false;
        if (!resources.Equals(other.resources)) return false;
        if (rerollsAvailable != other.rerollsAvailable) return false;
        if (!ownedDrillData.Equals(other.ownedDrillData)) return false;

        if ((ownedItems?.Count ?? 0) != (other.ownedItems?.Count ?? 0)) return false;
        if (ownedItems != null && other.ownedItems != null && !ownedItems.SequenceEqual(other.ownedItems)) return false;
        
        if (equippedAbilities == null && other.equippedAbilities == null) return true;
        if (equippedAbilities == null || other.equippedAbilities == null) return false;
        
        return equippedAbilities.SequenceEqual(other.equippedAbilities);
    }

    public override int GetHashCode()
    {
         int listHash = 0;
         if (ownedItems != null)
             foreach (var item in ownedItems) listHash ^= item.GetHashCode();
         
         int equippedHash = 0;
         if(equippedAbilities != null)
             foreach (var item in equippedAbilities) equippedHash ^= item.GetHashCode();
             
         return HashCode.Combine(health, resources, listHash, equippedHash, ownedDrillData, rerollsAvailable);
    }
}