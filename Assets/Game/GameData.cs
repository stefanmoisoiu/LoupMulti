using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameData : NetworkBehaviour
{
    // CONTROLLED BY SERVER
    private PlayerDataList _playerDataList;
    public PlayerDataList ServerSidePlayerDataList => _playerDataList;

    public List<PlayerData> clientPlayerData = new();
    public PlayerData myPlayerData;
        
    public static Action<List<PlayerData>> OnClientPlayerDataChanged;
    public static Action<PlayerData> OnMyPlayerDataChanged;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            LogRpc("Starting Game Manager");
            
            _playerDataList = new ();
            _playerDataList.SetupPlayerData();
        
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkObject.DestroyWithScene = false;
        }
        if (IsHost) RequestEntireClientPlayerData_ServerRpc(RpcParamsExt.Instance.SenderClientID(NetworkManager.Singleton.LocalClientId));
    }
    
    
    
    private void OnClientConnected(ulong clientId)
    {
        PlayerData data = new PlayerData(NetworkManager.Singleton.ConnectedClients[clientId]);
        _playerDataList.AddPlayerData(data);
        
        UpdateSpecificClientPlayerData_ClientRpc(data, RpcParamsExt.Instance.SendToAllClients());
        UpdateEntireClientPlayerData_ClientRpc(_playerDataList.playerDatas.ToArray(), RpcParamsExt.Instance.SendToClientIDs(new []{clientId}));
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.ConnectedClients.Count == 0) return;
        
        LogRpc("Client disconnected: " + clientId);
        _playerDataList.RemovePlayerData(clientId);
        
        RemoveClientPlayerData_ClientRpc(clientId, RpcParamsExt.Instance.SendToAllClients());
    }
        
    
    
    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateEntireClientPlayerData_ClientRpc(PlayerData[] playerDatas, RpcParams @params)
    {
        clientPlayerData = new List<PlayerData>(playerDatas);
        
        myPlayerData = clientPlayerData.Find(data => data.ClientId == NetworkManager.Singleton.LocalClientId);
        
        OnClientPlayerDataChanged?.Invoke(clientPlayerData);
        OnMyPlayerDataChanged?.Invoke(myPlayerData);
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateSpecificClientPlayerData_ClientRpc(PlayerData data, RpcParams @params)
    {
        if (data.ClientId == NetworkManager.Singleton.LocalClientId) myPlayerData = data;
        
        for (int i = 0; i < clientPlayerData.Count; i++)
        {
            if (clientPlayerData[i].ClientId != data.ClientId) continue;
            
            clientPlayerData[i] = data;
            
            OnClientPlayerDataChanged?.Invoke(clientPlayerData);
            OnMyPlayerDataChanged?.Invoke(myPlayerData);
            return;
        }
        
        clientPlayerData.Add(data);
        
        OnClientPlayerDataChanged?.Invoke(clientPlayerData);
    }
    [Rpc(SendTo.SpecifiedInParams)]
    public void RemoveClientPlayerData_ClientRpc(ulong clientId, RpcParams @params)
    {
        for (int i = 0; i < clientPlayerData.Count; i++)
        {
            if (clientPlayerData[i].ClientId != clientId) continue;
            
            clientPlayerData.RemoveAt(i);
            OnClientPlayerDataChanged?.Invoke(clientPlayerData);
            return;
        }
    }
    
    
    [Rpc(SendTo.Server)]
    public void RequestMyClientPlayerData_ServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        PlayerData data = _playerDataList.GetPlayerData(clientId);
        
        LogRpc(clientId + " Requested player data");
        
        UpdateSpecificClientPlayerData_ClientRpc(data, RpcParamsExt.Instance.SendToClientIDs(new []{clientId}));
    }
    
    [Rpc(SendTo.Server)]
    public void RequestEntireClientPlayerData_ServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        
        LogRpc(clientId + " Requested all player data");
        
        UpdateEntireClientPlayerData_ClientRpc(_playerDataList.playerDatas.ToArray(), RpcParamsExt.Instance.SendToClientIDs(new []{clientId}));
    }
    
    public void SetPlayerState(PlayerData.PlayerState state, ulong clientId)
    {
        PlayerData data = _playerDataList.GetPlayerData(clientId);
        data.SetState(state);
        _playerDataList.UpdatePlayerData(data);
        
        UpdateSpecificClientPlayerData_ClientRpc(data, RpcParamsExt.Instance.SendToAllClients());
        
        LogRpc(clientId + " is now " + state);
    }

    
    [Rpc(SendTo.Everyone)] public void LogRpc(string message) => Debug.Log($"<color=#80eeee><b>{message}");

}