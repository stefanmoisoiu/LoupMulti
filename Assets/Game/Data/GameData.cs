using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameData : NetworkBehaviour
{
    
    private NetworkVariable<PlayerGameData> playerGameData = new();
    public PlayerGameData PlayerGameData => playerGameData.Value;
    public void SetPlayerGameData(PlayerGameData playerGameData)
    {
        this.playerGameData.Value = playerGameData;
        this.playerGameData.SetDirty(true);
    }
    public PlayerGameData playerGameDataPreview;
    
    private void Start()
    {
        playerGameData.OnValueChanged += (_, newData) =>
        {
            PlayerData myPlayerData = newData.GetDataOrDefault(NetworkManager.Singleton.LocalClientId);
            CachedOwnerClientData.UpdateCachedDataOwner(myPlayerData);
        };
        
        if (!IsServer) return;
        NetcodeLogger.Instance.LogRpc("Server spawned", NetcodeLogger.LogType.Data, new []{NetcodeLogger.AddedEffects.Bold});
        playerGameData.Value = new PlayerGameData(PlayerGameData.BasePlayerDatas());
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            playerGameData.Value = playerGameData.Value.AddOrUpdateData(new PlayerData(client));
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void Update()
    {
        playerGameDataPreview = playerGameData.Value;
    }

    private void OnClientConnected(ulong clientId)
    {
        // spectate if in game
        SetPlayerState(
            GameManager.Instance.gameState.Value != GameManager.GameState.Lobby
                ? PlayerOuterData.PlayerState.SpectatingGame
                : PlayerOuterData.PlayerState.NotAssigned, clientId);
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameState.Value == GameManager.GameState.Lobby)
            playerGameData.Value = playerGameData.Value.RemoveData(clientId);
        else
            SetPlayerState(PlayerOuterData.PlayerState.Disconnected, clientId);
    }
    
    public void SetStateSpectate(ulong clientId) => SetStateSpectateServerRpc(RpcParamsExt.Instance.SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void SetStateSpectateServerRpc(RpcParams @params)
    {
        SetPlayerState(PlayerOuterData.PlayerState.SpectatingGame, @params.Receive.SenderClientId);
    }
    public void SetStatePlaying(ulong clientId) => SetStatePlayingServerRpc(RpcParamsExt.Instance.SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void SetStatePlayingServerRpc(RpcParams @params)
    {
        if (GameManager.Instance != null && GameManager.Instance.gameState.Value != GameManager.GameState.Lobby) return;
        
        SetPlayerState(PlayerOuterData.PlayerState.Playing, @params.Receive.SenderClientId);
    }
    
    //Server
    public void SetNotAssignedPlayersToPlaying()
    {
        // set all players to playing state
        PlayerData[] playerDatas = playerGameData.Value.GetDatas();
        foreach (PlayerData data in playerDatas)
        {
            if (data.OuterData.CurrentPlayerState != PlayerOuterData.PlayerState.NotAssigned) continue;
            SetPlayerState(PlayerOuterData.PlayerState.Playing, data.ClientId);
        }
    }
    
    public void SetPlayerState(PlayerOuterData.PlayerState state, ulong clientId)
    {
        PlayerData data = playerGameData.Value.GetDataOrDefault(clientId);
        data = new(data) { OuterData = data.OuterData.SetState(PlayerOuterData.PlayerState.Playing) };
        SetPlayerGameData(playerGameData.Value.UpdateData(data));
        
        NetcodeLogger.Instance.LogRpc(clientId + " is now " + state, NetcodeLogger.LogType.Data, new []{NetcodeLogger.AddedEffects.Bold});
    }
}