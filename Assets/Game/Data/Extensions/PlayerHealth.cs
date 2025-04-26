using System;
using Unity.Netcode;

namespace Game.Data.Extensions
{
    public class PlayerHealth : NetworkBehaviour
    {
    
        // Server
        public static event Action<ulong> OnPlayerDiedServer;
        public static event Action OnOnePlayerLeftServer;

        private void Start()
        {
            if (IsServer) PlayerDataManager.OnEntryUpdatedServer += DetectPlayerDeath;
        }

        private void DetectPlayerDeath(PlayerData previousPlayerData, PlayerData newPlayerData)
        {
            if (previousPlayerData.InGameData.IsAlive() && !newPlayerData.InGameData.IsAlive())
                OnPlayerDiedServer?.Invoke(newPlayerData.ClientId);
            if (AlivePlayingPlayers().Length == 1)
                OnOnePlayerLeftServer?.Invoke();
        }

        public static PlayerData[] AlivePlayingPlayers()
        {
            PlayerData[] players = PlayerDataManager.Instance.GetValues();
            return Array.FindAll(players, player => player.OuterData.playingState == PlayerOuterData.PlayingState.Playing
                                                    && player.InGameData.IsAlive());
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
            foreach (PlayerData player in PlayerDataManager.Instance.GetValues())
            {
                if (player.OuterData.playingState != PlayerOuterData.PlayingState.Playing) continue;
                PlayerDataManager.Instance[player.ClientId] = new(player) {InGameData = player.InGameData.ResetHealth()};
            }
        }
    }
}