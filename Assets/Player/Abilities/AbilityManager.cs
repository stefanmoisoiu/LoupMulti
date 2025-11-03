using System;
using System.Collections.Generic;
using System.Linq;
using Input;
using Player.Abilities.UI;
using Player.Networking;
using Sirenix.OdinInspector;
using UnityEngine;
using Game.Common;
using Game.Data;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;

namespace Player.Abilities
{
    public class AbilityManager : PNetworkBehaviour
    {
        private PlayerReferences _playerReferences;
        [TitleGroup("UI")]
        [SerializeField] private AbilityManagerUI abilityManagerUI;

        [TitleGroup("Ability Mapping")]
        [SerializeField]
        private SerializedDictionary<Item, Ability> abilityPrefabMap;
        

        public const int AbilitySlotCount = 3;
        
        private Ability[] _abilitySlots;
        public Ability[] AbilitySlotsArray => _abilitySlots;
        
        private Ability _drillSlot;
        public Ability DrillSlot => _drillSlot;

        private Ability GetAbilityPrefab(Item item)
        {
            if (abilityPrefabMap.TryGetValue(item, out Ability prefab)) return prefab;
            throw new System.Exception($"Aucun prefab 'Ability' n'est mappé pour l'item '{item.Info.Name}' dans le AbilityManager.");
        }

        protected override void StartOnlineOwner()
        {
            _playerReferences = GetComponentInParent<PlayerReferences>();
            _abilitySlots = new Ability[AbilitySlotCount];
        
            InputManager.OnDrillUse += TryUseDrill;
            InputManager.OnAbilityUse += TryUseAbilitySlot;
            DataManager.OnEntryUpdatedOwner += OnPlayerDataUpdated;
            
            if (DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData initialData))
            {
                OnPlayerDataUpdated(default, initialData);
            }
        }

        protected override void StartOnlineServer()
        {
            _playerReferences = GetComponentInParent<PlayerReferences>();
            _abilitySlots = new Ability[AbilitySlotCount];
        
            DataManager.OnEntryUpdatedClient += OnPlayerDataUpdated;
            
            if (DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData initialData))
            {
                OnPlayerDataUpdated(default, initialData);
            }
        }

        protected override void DisableOnlineOwner()
        {
            InputManager.OnDrillUse -= TryUseDrill;
            InputManager.OnAbilityUse -= TryUseAbilitySlot;
            DataManager.OnEntryUpdatedOwner -= OnPlayerDataUpdated;
        }

        protected override void DisableOnlineServer()
        {
            DataManager.OnEntryUpdatedClient -= OnPlayerDataUpdated;
        }

        private void OnPlayerDataUpdated(PlayerData previousData, PlayerData newData)
        {
            if (newData.clientId != OwnerClientId) return;
            
            UpdateDrill(newData.inGameData.ownedDrillData);
            UpdateEquippedAbilities(newData.inGameData.equippedAbilities);
        }

        private void UpdateDrill(OwnedItemData drillData)
        {
            AbilitySlotUI drillUISlot = abilityManagerUI.GetSlotUI(-1); 
            
            if (_drillSlot == null)
            {
                Item item = ItemRegistry.Instance.GetItem(drillData.ItemRegistryIndex);
                _drillSlot = GetAbilityPrefab(item);
                _drillSlot.UpdateInfo(drillData);
                _drillSlot.EnableAbility(drillUISlot);
            }
            else
            {
                _drillSlot.UpdateInfo(drillData);
            }
        }

        private void UpdateEquippedAbilities(OwnedItemData[] serverAbilities)
        {
            for (int i = 0; i < AbilitySlotCount; i++)
            {
                OwnedItemData serverItemData = serverAbilities[i];
                Ability localAbility = _abilitySlots[i];
                AbilitySlotUI slotUI = abilityManagerUI.GetSlotUI(i);

                if (serverItemData.IsEmpty())
                {
                    localAbility?.DisableAbility();
                    _abilitySlots[i] = null;
                }
                else 
                {
                    if (localAbility != null && localAbility.ItemRegistryIndex == serverItemData.ItemRegistryIndex)
                    {
                        localAbility.UpdateInfo(serverItemData);
                    }
                    else
                    {
                        localAbility?.DisableAbility();
                        Item item = ItemRegistry.Instance.GetItem(serverItemData.ItemRegistryIndex);
                        Ability newAbility = GetAbilityPrefab(item);
                        
                        newAbility.UpdateInfo(serverItemData);
                        newAbility.EnableAbility(slotUI);
                        
                        _abilitySlots[i] = newAbility;
                    }
                }
            }
        }
        
        private void TryUseDrill() => _drillSlot?.TryUseAbility(out bool _);
        
        private void TryUseAbilitySlot(int slot)
        {
            if (slot < 0 || slot >= AbilitySlotCount) return;
            _abilitySlots[slot]?.TryUseAbility(out bool _);
        }
        
        [ServerRpc]
        public void RequestEquipAbilityServerRpc(ushort itemIndexToEquip, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= AbilitySlotCount) return;
            
            PlayerData data = DataManager.Instance[OwnerClientId];
            
            if (!data.inGameData.HasItem(itemIndexToEquip)) return;

            OwnedItemData ownedItemData = data.inGameData.GetOwnedItem(itemIndexToEquip);

            int currentlyEquippedAtIndex = Array.FindIndex(data.inGameData.equippedAbilities, x => !x.IsEmpty() && x.ItemRegistryIndex == itemIndexToEquip);

            if (currentlyEquippedAtIndex != -1)
            {
                data.inGameData.equippedAbilities[currentlyEquippedAtIndex] = data.inGameData.equippedAbilities[slotIndex];
            }
            
            data.inGameData.equippedAbilities[slotIndex] = ownedItemData;
            
            DataManager.Instance[OwnerClientId] = data;
        }

        [ServerRpc]
        public void RequestSwapAbilityServerRpc(int slotA, int slotB)
        {
            if (slotA < 0 || slotA >= AbilitySlotCount || slotB < 0 || slotB >= AbilitySlotCount) return;
            
            PlayerData data = DataManager.Instance[OwnerClientId];

            (data.inGameData.equippedAbilities[slotA], data.inGameData.equippedAbilities[slotB]) = 
                (data.inGameData.equippedAbilities[slotB], data.inGameData.equippedAbilities[slotA]);

            DataManager.Instance[OwnerClientId] = data;
        }

        [ServerRpc]
        public void RequestUnequipAbilityServerRpc(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= AbilitySlotCount) return;
            
            PlayerData data = DataManager.Instance[OwnerClientId];
            data.inGameData.equippedAbilities[slotIndex] = OwnedItemData.Empty;
            DataManager.Instance[OwnerClientId] = data;
        }
    }
}