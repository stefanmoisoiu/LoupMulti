using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerDeath : NetworkBehaviour
{
    
    // Server
    public static event Action<ulong> OnPlayerDiedServer;

    private void Start()
    {
        if (IsServer) PlayerDataManager.OnEntryUpdatedServer += DetectPlayerDeath;
    }

    private void DetectPlayerDeath(PlayerData previousPlayerData, PlayerData newPlayerData)
    {
        if (previousPlayerData.InGameData.IsAlive() && !newPlayerData.InGameData.IsAlive())
            OnPlayerDiedServer?.Invoke(newPlayerData.ClientId);
    }

    public PlayerData[] GetAlivePlayingPlayers()
    {
        PlayerData[] players = PlayerDataManager.Instance.GetValues();
        return Array.FindAll(players, player => player.OuterData.playingState == PlayerOuterData.PlayingState.Playing
                                                && player.InGameData.IsAlive());
    }
}