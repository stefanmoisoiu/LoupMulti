using System;
using Game.Common;
using Game.Data;
using Game.Game_Loop;
using Player.Networking;
using UnityEngine;

namespace Player.Target
{
    public class HitboxVisibility : PNetworkBehaviour
    {
        [SerializeField] private Targetable targetable;

        protected override void StartOnlineOwner()
        {
            UpdateHitboxVisibility();
            DataManager.OnEntryUpdatedClient += EntryUpdatedClient;
            GameManager.OnGameStateChangedAll += GameStateOnValueChangedAll;
        }
        
        protected override void DisableAnyOwner()
        {
            GameManager.OnGameStateChangedAll -= GameStateOnValueChangedAll;
            DataManager.OnEntryUpdatedClient -= EntryUpdatedClient;
        }


        private void GameStateOnValueChangedAll(GameManager.GameState previousValue, GameManager.GameState newValue) => UpdateHitboxVisibility();
        private void EntryUpdatedClient(PlayerData previousValue, PlayerData newValue) => UpdateHitboxVisibility();
        
        private void UpdateHitboxVisibility()
        {
            targetable.enabled = ShouldEnableHitbox();
        }
        
        private bool ShouldEnableHitbox() => DataManager.Instance != null && DataManager.Instance[OwnerClientId].outerData.playingState == OuterData.PlayingState.Playing &&
                                             GameManager.CurrentGameState == GameManager.GameState.InGame;
    }
}