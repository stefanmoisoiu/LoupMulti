using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameData : NetworkBehaviour
{
    
    public NetworkVariable<PlayerGameData> playerGameData = new();
    public PlayerGameData playerGameDataPreview;
        
    public static Action<List<PlayerData>> OnClientPlayerDataChanged;
    public static Action<PlayerData> OnMyPlayerDataChanged;
    
    // cached Owner data:
    public ScriptableUpgrade[] CachedOwnerOwnedUpgrades { get; private set; }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            playerGameData.Value = new PlayerGameData(PlayerGameData.BasePlayerDatas());
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                playerGameData.Value = playerGameData.Value.AddOrUpdateData(new PlayerData(client));
        
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkObject.DestroyWithScene = false;
        }
        else
        {
            playerGameData.OnValueChanged += UpdateCachedOwnerData;
        }
    }

    private void UpdateCachedOwnerData(PlayerGameData previousData, PlayerGameData newData)
    {
        ulong id = NetworkManager.LocalClientId;
        PlayerData newOwnerData = newData.GetDataOrDefault(id);
        CachedOwnerOwnedUpgrades = newOwnerData.InGameData.GetUpgrades();
    }
    private void Update()
    {
        playerGameDataPreview = playerGameData.Value;
    }

    private void OnClientConnected(ulong clientId)
    {
        PlayerData data = new PlayerData(NetworkManager.Singleton.ConnectedClients[clientId]);
        if (GameManager.Instance.gameState.Value != GameManager.GameState.Lobby)
            data = new(data) { OuterData = data.OuterData.SetState(PlayerOuterData.PlayerState.SpectatingGame) };
        playerGameData.Value = playerGameData.Value.AddOrUpdateData(data);
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameState.Value == GameManager.GameState.Lobby)
        {
            playerGameData.Value = playerGameData.Value.RemoveData(clientId);
        }
        else SetPlayerState(PlayerOuterData.PlayerState.NotAssigned, clientId);
    }
    
    public void SetPlayerState(PlayerOuterData.PlayerState state, ulong clientId)
    {
        PlayerData data = playerGameData.Value.GetDataOrDefault(clientId);
        data.OuterData.SetState(state);
        playerGameData.Value = playerGameData.Value.UpdateData(data);
        
        NetcodeLogger.Instance.LogRpc(clientId + " is now " + state, NetcodeLogger.ColorType.Blue, new []{NetcodeLogger.AddedEffects.Bold});
    }

    public void StartedGameServer()
    {
        // set all players to playing state
        PlayerData[] playerDatas = playerGameData.Value.GetDatas();
        foreach (PlayerData data in playerDatas)
        {
            if (data.OuterData.CurrentPlayerState == PlayerOuterData.PlayerState.SpectatingGame) continue;
            playerGameData.Value = playerGameData.Value.UpdateData(new(data)
                { OuterData = data.OuterData.SetState(PlayerOuterData.PlayerState.Playing) });
        }
    }
}