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
            if (IsServer) DataManager.OnEntryUpdatedServer += DetectPlayerDeath;
        }

        private void DetectPlayerDeath(PlayerData previousPlayerData, PlayerData newPlayerData)
        {
            if (previousPlayerData.inGameData.IsAlive() && !newPlayerData.inGameData.IsAlive())
                OnPlayerDiedServer?.Invoke(newPlayerData.ClientId);
            if (AlivePlayingPlayers().Length == 1)
                OnOnePlayerLeftServer?.Invoke();
        }

        public static PlayerData[] AlivePlayingPlayers()
        {
            PlayerData[] players = DataManager.Instance.GetValues();
            return Array.FindAll(players, player => player.outerData.playingState == OuterData.PlayingState.Playing
                                                    && player.inGameData.IsAlive());
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
    }
}