using Game.Common;
using Game.Game_Loop;
using Game.Upgrade.Shop;
using Player.Networking;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Game.Data;
using Game.Upgrade.Carousel;
using Player.Stats;

namespace Player.Perks
{
    public class PerkEffectManager : PNetworkBehaviour
    {
        private PlayerReferences _playerReferences;
        [TitleGroup("Perk Prefab Mapping")]
        [SerializeField]
        private SerializedDictionary<Item, PerkEffect> perkMap;
        protected override void StartOnlineOwner()
        {
            _playerReferences = GetComponentInParent<PlayerReferences>();
            
            DataManager.OnEntryUpdatedOwner += OnPlayerDataUpdated;
            
            if (DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData initialData))
            {
                OnPlayerDataUpdated(default, initialData);
            }
        }

        protected override void StartOnlineServer()
        {
            _playerReferences = GetComponentInParent<PlayerReferences>();
            
            DataManager.OnEntryUpdatedClient += OnPlayerDataUpdated;
            
            if (DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData initialData))
            {
                OnPlayerDataUpdated(default, initialData);
            }
        }

        protected override void DisableAnyOwner()
        {
            DataManager.OnEntryUpdatedOwner -= OnPlayerDataUpdated;
        }

        protected override void DisableOnlineServer()
        {
            DataManager.OnEntryUpdatedClient -= OnPlayerDataUpdated;
        }

        private void OnPlayerDataUpdated(PlayerData previousData, PlayerData newData)
        {
            if (newData.clientId != OwnerClientId) return;
            
            UpdateAppliedPerks(newData.inGameData.GetAllPerks());
        }

        private void UpdateAppliedPerks(List<OwnedItemData> serverPerks)
        {
            foreach (Item item in perkMap.Keys)
            {
                ushort itemRegistryIndex = ItemRegistry.Instance.GetItem(item);
                PerkEffect perkEffect = perkMap[item];

                OwnedItemData ownedPerkData = OwnedItemData.Empty;
                foreach (OwnedItemData ownedItemData in serverPerks)
                {
                    if (ownedItemData.ItemRegistryIndex == itemRegistryIndex)
                    {
                        ownedPerkData = ownedItemData;
                        break;
                    }
                }
                
                if (ownedPerkData.IsEmpty())
                {
                    perkEffect.SetPerkEnabled(false);
                }
                else
                {
                    perkEffect.UpdateInfo(ownedPerkData);
                    perkEffect.SetPerkEnabled(true);
                }
            }
        }
    }
}