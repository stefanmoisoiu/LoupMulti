using Game.Common;
using Game.Game_Loop;
using Game.Upgrade.Perks;
using Game.Upgrade.Shop;
using Player.Networking;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections; // Ajouté

namespace Player.Perks
{
    public class PerkEffectManager : PNetworkBehaviour
    {
        [TitleGroup("Perk Prefab Mapping")]
        [Tooltip("Relation 'bakée' entre un Item et son prefab PerkEffect.")]
        [SerializeField]
        private SerializedDictionary<Item, PerkEffect> perkMap; // Plus de liste !
        
        private List<PerkEffect> _activePerks = new List<PerkEffect>();


        private PerkEffect GetPerkPrefabForItem(Item item)
        {
            if (perkMap.TryGetValue(item, out PerkEffect prefab))
            {
                return prefab;
            }
            
            Debug.LogError($"Aucun prefab 'PerkEffect' n'est mappé pour l'item '{item.name}' dans le PerkEffectManager.");
            return null;
        }

        protected override void StartOnlineOwner()
        {
            ShopManager.OnShopItemBoughtOwner += OnShopItemBoughtOwner;
            ItemSelectionManager.OnItemChosenOwner += OnItemChosenOwner;
            GameManager.OnGameStateChangedAll += OnGameStateChangedAll;
        }

        protected override void DisableAnyOwner()
        {
            ShopManager.OnShopItemBoughtOwner -= OnShopItemBoughtOwner;
            ItemSelectionManager.OnItemChosenOwner -= OnItemChosenOwner;
            GameManager.OnGameStateChangedAll -= OnGameStateChangedAll;
        }
        
        private void OnShopItemBoughtOwner(ushort itemInd)
        {
            Item item = ItemRegistry.Instance.GetItem(itemInd);
            if (item.Type != Item.ItemType.Perk) return;
            ApplyPerk(item);
        }

        private void OnItemChosenOwner(ushort itemInd)
        {
            Item item = ItemRegistry.Instance.GetItem(itemInd);
            if (item.Type != Item.ItemType.Perk) return;
            ApplyPerk(item);
        }
        
        private void ApplyPerk(Item item)
        {
            PerkEffect prefab = GetPerkPrefabForItem(item);
            if (prefab == null) return;
            
            foreach(var perk in _activePerks)
            {
                if(perk.Item == item)
                {
                    Debug.Log($"Perk {item.Info.Name} déjà appliqué.");
                    return;
                }
            }

            GameObject spawnedObject = Instantiate(prefab.gameObject, transform);
            PerkEffect newPerkInstance = spawnedObject.GetComponent<PerkEffect>();

            newPerkInstance.Initialize(item);
            newPerkInstance.SetApplied(true);
            _activePerks.Add(newPerkInstance);
        }
        
        private void OnGameStateChangedAll(GameManager.GameState previousState, GameManager.GameState newState)
        {
            SetPerksEnabled(newState == GameManager.GameState.InGame);
        }
        
        private void SetPerksEnabled(bool value)
        {
            foreach (PerkEffect perkInstance in _activePerks)
            {
                perkInstance.SetApplied(value);
            }
        }
    }
}