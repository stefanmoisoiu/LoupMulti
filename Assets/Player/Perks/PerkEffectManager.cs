using AYellowpaper.SerializedCollections;
using Game.Common;
using Game.Game_Loop;
using Game.Upgrade.Perks;
using Game.Upgrade.Shop;
using Player.Networking;
using UnityEngine;

namespace Player.Perks
{
    public class PerkEffectManager : PNetworkBehaviour
    {
        [SerializeField] private SerializedDictionary<PerkData, PerkEffect> perkEffects;

        protected override void StartOnlineOwner()
        {
            ShopManager.OnShopItemBoughtOwner += OnShopItemBoughtOwner;
            ItemSelectionManager.OnItemChosenOwner += ApplyItem;
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnShopItemBoughtOwner(ushort itemInd)
        {
            Item item = ItemRegistry.Instance.GetItem(itemInd);
            if (item.PerkData == null) return;
            ApplyItem(itemInd);
        }

        protected override void DisableAnyOwner()
        {
            ShopManager.OnShopItemBoughtOwner -= OnShopItemBoughtOwner;
            ItemSelectionManager.OnItemChosenOwner -= ApplyItem;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }
        private void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState newState)
        {
            SetPerksEnabled(newState == GameManager.GameState.InGame);
        }
        private void SetPerksEnabled(bool value)
        {
            foreach (PerkEffect perkEffect in perkEffects.Values)
            {
                if (perkEffect.Applied)
                    perkEffect.SetApplied(value);
            }
        }
        
        
        
        private void ApplyItem(ushort itemInd)
        {
            Item item = ItemRegistry.Instance.GetItem(itemInd);
            PerkData perk = item.PerkData;
            if (perkEffects.TryGetValue(perk, out PerkEffect perkEffect))
            {
                perkEffect.SetApplied(true);
            }
            else
            {
                Debug.LogWarning($"Perk effect not found for perk: {item.Info.Name}");
            }
        }
    }
}