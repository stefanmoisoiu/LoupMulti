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
        if (_playerDataList.ContainsPlayerData(clientId)) return;
        
        PlayerData data = new PlayerData(NetworkManager.Singleton.ConnectedClients[clientId]);
        _playerDataList.AddPlayerData(data);
        
        UpdateSpecificClientPlayerData_ClientRpc(data, RpcParamsExt.Instance.SendToAllClients(NetworkManager.Singleton));
        UpdateEntireClientPlayerData_ClientRpc(_playerDataList.playerDatas.ToArray(), RpcParamsExt.Instance.SendToClientIDs(new []{clientId}, NetworkManager.Singleton));
    }
    private void OnClientDisconnected(ulong clientId)
    {
        
    }
        
    
    
    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateEntireClientPlayerData_ClientRpc(PlayerData[] datas, RpcParams @params)
    {
        clientPlayerData = new List<PlayerData>(datas);
        
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
    
    
    [Rpc(SendTo.Server)]
    public void RequestMyClientPlayerData_ServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        PlayerData data = _playerDataList.GetPlayerData(clientId);
        
        GameManager.Instance.LogRpc(clientId + " Requested player data", GameManager.LogType.GameDataInfo);
        
        UpdateSpecificClientPlayerData_ClientRpc(data, RpcParamsExt.Instance.SendToClientIDs(new []{clientId}, NetworkManager.Singleton));
    }
    
    [Rpc(SendTo.Server)]
    public void RequestEntireClientPlayerData_ServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        
        GameManager.Instance.LogRpc(clientId + " Requested all player data", GameManager.LogType.GameDataInfo);
        
        UpdateEntireClientPlayerData_ClientRpc(_playerDataList.playerDatas.ToArray(), RpcParamsExt.Instance.SendToClientIDs(new []{clientId}, NetworkManager.Singleton));
    }
    
    public void SetPlayerState(PlayerOuterData.PlayerState state, ulong clientId)
    {
        PlayerData data = _playerDataList.GetPlayerData(clientId);
        data.OuterData.SetState(state);
        _playerDataList.UpdatePlayerData(data);
        
        UpdateSpecificClientPlayerData_ClientRpc(data, RpcParamsExt.Instance.SendToAllClients(NetworkManager.Singleton));
        
        GameManager.Instance.LogRpc(clientId + " is now " + state, GameManager.LogType.GameDataInfo);
    }
}