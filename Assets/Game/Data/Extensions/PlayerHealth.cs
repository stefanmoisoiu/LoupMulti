using System;
using Game.Common;
using Unity.Netcode;
using UnityEngine;
using PlayerData = Game.Common.PlayerData;

namespace Game.Data.Extensions
{
    public class PlayerHealth : NetworkBehaviour
    {
    
        public static event Action<ulong> OnOnePlayerLeftServer;
        public static event Action<ulong> OnOnePlayerLeftOwner;
        public static event Action<ulong> OnOnePlayerLeftClient;
        
        public static event Action<ulong> OnPlayerDiedServer;
        public static event Action OnPlayerDiedOwner;
        public static event Action<ulong> OnPlayerDiedClient;
        
        public static event Action<ushort,ushort, ulong> OnHealthChangedServer;
        public static event Action<ushort,ushort> OnHealthChangedOwner;
        public static event Action<ushort,ushort, ulong> OnHealthChangedClient;

        private void Start()
        {
            DataManager.OnEntryUpdatedClient += DetectPlayerDeath;
            DataManager.OnEntryUpdatedClient += PlayerHealthChanged;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            DataManager.OnEntryUpdatedClient -= DetectPlayerDeath;
            DataManager.OnEntryUpdatedClient -= PlayerHealthChanged;
        }

        private void DetectPlayerDeath(PlayerData previousPlayerData, PlayerData newPlayerData)
        {
            if (newPlayerData.outerData.playingState != OuterData.PlayingState.SpectatingGame && previousPlayerData.inGameData.IsAlive() && !newPlayerData.inGameData.IsAlive())
            {
                if (IsServer)
                {
                    DataManager.Instance[newPlayerData.ClientId] = new(newPlayerData)
                    {
                        outerData = new OuterData(newPlayerData.outerData) {playingState = OuterData.PlayingState.SpectatingGame}
                    };
                    OnPlayerDiedServer?.Invoke(newPlayerData.ClientId);
                }
                if (newPlayerData.ClientId == NetworkManager.LocalClientId)
                    OnPlayerDiedOwner?.Invoke();
                OnPlayerDiedClient?.Invoke(newPlayerData.ClientId);
            }

            PlayerData[] alivePlayers = AlivePlayingPlayers();
            if (alivePlayers == null || alivePlayers.Length == 0)
            {
                return;
            }
            if (alivePlayers.Length == 1)
            {
                if (IsServer) OnOnePlayerLeftServer?.Invoke(alivePlayers[0].ClientId);
                if (alivePlayers[0].ClientId == NetworkManager.LocalClientId)
                    OnOnePlayerLeftOwner?.Invoke(alivePlayers[0].ClientId);
                OnOnePlayerLeftClient?.Invoke(alivePlayers[0].ClientId);
            }
        }
        
        private void PlayerHealthChanged(PlayerData previousPlayerData, PlayerData newPlayerData)
        {
            ushort previousHealth = previousPlayerData.inGameData.health;
            ushort newHealth = newPlayerData.inGameData.health;

            if (previousHealth == newHealth) return;
            
            if (IsServer) OnHealthChangedServer?.Invoke(previousHealth, newHealth, newPlayerData.ClientId);
            if (newPlayerData.ClientId == NetworkManager.LocalClientId)
                OnHealthChangedOwner?.Invoke(previousPlayerData.inGameData.health, newPlayerData.inGameData.health);
            OnHealthChangedClient?.Invoke(previousPlayerData.inGameData.health, newPlayerData.inGameData.health, newPlayerData.ClientId);
        }

        public static PlayerData[] AlivePlayingPlayers()
        {
            return DataManager.Instance.Search(new[] { OuterData.PlayingState.Playing }, DataExt.HealthSearchConditions.Alive);
        }
    
        public static int AlivePlayerCount() => AlivePlayingPlayers().Length;

        public static PlayerData RandomAlivePlayer()
        {
            PlayerData[] players = AlivePlayingPlayers();
            if (players.Length == 0) return new();
            int randomIndex = UnityEngine.Random.Range(0, players.Length);
            return players[randomIndex];
        }
    
        public static void ResetPlayersHealth()
        {
            foreach (PlayerData player in DataManager.Instance.GetValues())
            {
                if (player.outerData.playingState != OuterData.PlayingState.Playing) continue;
                DataManager.Instance[player.ClientId] = new(player) {inGameData = player.inGameData.ResetHealth()};
            }
        }
        [ServerRpc(RequireOwnership = false)]
        public void AddHealthServerRpc(ushort amount, ulong clientId)
        {
            if (!IsServer) return;
            PlayerData playerData = DataManager.Instance[clientId];
            DataManager.Instance[clientId] = new(playerData) {inGameData = playerData.inGameData.AddHealth(amount)};
        }
        [ServerRpc(RequireOwnership = false)]
        public void RemoveHealthServerRpc(ushort amount, ulong clientId)
        {
            if (!IsServer) return;
            PlayerData playerData = DataManager.Instance[clientId];
            DataManager.Instance[clientId] = new(playerData) {inGameData = playerData.inGameData.RemoveHealth(amount)};
        }
    }
}