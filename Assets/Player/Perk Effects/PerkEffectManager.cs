using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Base_Scripts;
using Game.Common;
using Game.Common.List;
using Game.Data;
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Player.Networking;
using UnityEngine;

namespace Player.Perk_Effects
{
    public class PerkEffectManager : PNetworkBehaviour
    {
        [SerializeField] private PlayerReferences references;

        [SerializeField] private SerializedDictionary<PerkData, PerkEffect> perkEffects;

        protected override void StartOnlineOwner()
        {
            GameLoopEvents.OnRoundStateChangedAll += RoundStateChanged;
        }

        protected override void DisableAnyOwner()
        {
            GameLoopEvents.OnRoundStateChangedAll -= RoundStateChanged;
        }
        
        private void RoundStateChanged(GameRoundState newState, float time)
        {
            EnteredCollectRound(newState == GameRoundState.Collect);
        }

        private void EnteredCollectRound(bool value)
        {
            PlayerData myData = DataManager.Instance[NetworkManager.LocalClientId];
            PerkData[] ownedPerks = myData.inGameData.GetPerks();
            List<PerkData> perksChecked = new List<PerkData>();
            foreach (PerkData perk in ownedPerks)
            {
                if(perksChecked.Contains(perk)) continue;
                perksChecked.Add(perk);
                
                int perkCount = ownedPerks.Count(x => x == perk);
                Debug.Log($"Has {perkCount} perks of type {perk.name}");
                
                if (perkEffects.TryGetValue(perk, out PerkEffect perkEffect))
                {
                    perkEffect.SetApplied(value, references, perkCount);
                }
                else
                {
                    Debug.LogWarning($"Perk effect not found for perk: {perk.name}");
                }
            }
        }
    }
}