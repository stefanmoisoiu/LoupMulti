using System;
using System.Collections;
using System.Collections.Generic;
using Smooth;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private string[] maps;

    public static GameManager instance;

    public NetworkVariable<GameState> gameState = new();
    
    // CONTROLLED BY SERVER
    private PlayerDataList _playerDataList;

    public List<PlayerData> clientPlayerData = new();
    public PlayerData myPlayerData;
    
    public static Action<List<PlayerData>> OnClientPlayerDataChanged;
    public static Action<PlayerData> OnMyPlayerDataChanged;
    
    public const int RoundCount = 10;
    public const int TimeToUpgrade = 20;
    public const int StartCountdown = 5;

    private Coroutine _gameLoopCoroutine;

    private const string LobbyMap = "MultiLobby";
    private string currentMap;
    
    public Action<string> OnMapLoaded;
    [Rpc(SendTo.Everyone)] private void OnMapLoadedClientRpc(string map) => OnMapLoaded?.Invoke(map);
    
    public Action OnGameStart;
    [Rpc(SendTo.Everyone)] private void OnGameStartClientRpc() => OnGameStart?.Invoke();

    private void Awake()
    {
        instance = this;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        instance = null;
        Destroy(gameObject);
    }

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
        if (IsHost) RequestEntireClientPlayerData_ServerRpc(SenderClientID(NetworkManager.Singleton.LocalClientId));
    }
    
    [Rpc(SendTo.Server)]
    public void RequestMyClientPlayerData_ServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        PlayerData data = _playerDataList.GetPlayerData(clientId);
        
        LogRpc(clientId + " Requested player data");
        
        UpdateSpecificClientPlayerData_ClientRpc(data, SendToClientIDs(new []{clientId}));
    }
    
    [Rpc(SendTo.Server)]
    public void RequestEntireClientPlayerData_ServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        
        LogRpc(clientId + " Requested all player data");
        
        UpdateEntireClientPlayerData_ClientRpc(_playerDataList.playerDatas.ToArray(), SendToClientIDs(new []{clientId}));
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
            return;
        }
        OnClientPlayerDataChanged?.Invoke(clientPlayerData);
    }
    
    private void OnClientConnected(ulong clientId)
    {
        LogRpc("Client connected: " + clientId);
        
        PlayerData data = new PlayerData(NetworkManager.Singleton.ConnectedClients[clientId]);
        _playerDataList.AddPlayerData(data);
        
        if (gameState.Value != GameState.Lobby) Spectate(clientId);
        
        UpdateSpecificClientPlayerData_ClientRpc(data, SendToAllClients());
        UpdateEntireClientPlayerData_ClientRpc(_playerDataList.playerDatas.ToArray(), SendToClientIDs(new []{clientId}));
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.ConnectedClients.Count == 0) return;
        
        LogRpc("Client disconnected: " + clientId);
        _playerDataList.RemovePlayerData(clientId);
        
        RemoveClientPlayerData_ClientRpc(clientId, SendToAllClients());
    }

    [Rpc(SendTo.Server)]
    public void StartGameServerRpc()
    {
        gameState.Value = GameState.InGame;
        
        string map = GetRandomMap(maps);
        NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
        currentMap = map;
        LogRpc("Loading map " + map + "...");
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += StartGameMapLoaded;
    }

    // SERVER SIDE
    private void StartGameMapLoaded(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        LogRpc("Map loaded");
        OnMapLoadedClientRpc(currentMap);
        
        SetPlayerSpawnPositions();

        foreach (PlayerData data in _playerDataList.playerDatas)
        {
            data.ResetGameData();
            
            if (data.CurrentPlayerState == PlayerData.PlayerState.Spectating)
            {
                
            }
            else
            {
                if (data.CurrentPlayerState == PlayerData.PlayerState.NotAssigned) data.SetState(PlayerData.PlayerState.Playing);
            }
        }
        OnGameStartClientRpc();
        
        if (_gameLoopCoroutine != null) StopCoroutine(_gameLoopCoroutine);
        _gameLoopCoroutine = StartCoroutine(GameLoop());
    }
    
    

    private void SetPlayerSpawnPositions()
    {
        ushort[] spawnIndexes = MapSpawnPositions.instance.GetTransformIndexes(_playerDataList.playerDatas.Count);
        for(int i = 0; i < _playerDataList.playerDatas.Count; i++)
        {
            PlayerData data = _playerDataList.playerDatas[i];
            ushort spawnIndex = spawnIndexes[i];
            
            Transform spawnPoint = MapSpawnPositions.instance.GetSpawnPoint(spawnIndex);
            SmoothSyncNetcode sync = NetworkManager.Singleton.ConnectedClients[data.ClientId].PlayerObject.GetComponent<SmoothSyncNetcode>();
            sync.teleportAnyObjectFromServer(spawnPoint.position, spawnPoint.rotation, Vector3.one);
        }
    }

    private IEnumerator GameLoop()
    {
        // Choose Upgrade -> Play Round -> Repeat
        int round = 1;
        
        while (round <= RoundCount)
        {
            yield return ChooseUpgrade();
            yield return PlayRound();
            round++;
        }
    }
    
    private IEnumerator ChooseUpgrade()
    {
        gameState.Value = GameState.ChoosingUpgrade;
        yield return new WaitForSeconds(TimeToUpgrade);
    }
    private IEnumerator PlayRound()
    {
        gameState.Value = GameState.InGame;
        yield return new WaitForSeconds(StartCountdown);
    }
    private IEnumerator EndGame()
    {
        gameState.Value = GameState.Lobby;
        yield break;
    }
    
    public void Spectate(ulong clientId) => SpectateServerRpc(SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void SpectateServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        
        PlayerData data = _playerDataList.GetPlayerData(clientId);
        data.SetState(PlayerData.PlayerState.Spectating);
        _playerDataList.UpdatePlayerData(data);
        
        UpdateSpecificClientPlayerData_ClientRpc(data, SendToAllClients());
        
        LogRpc(clientId + " is now spectating");
    }
    public void BecomePlayer(ulong clientId) => BecomePlayerServerRpc(SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void BecomePlayerServerRpc(RpcParams @params)
    {
        if (!CanBecomePlayer()) return;
        
        ulong clientId = @params.Receive.SenderClientId;
        
        PlayerData data = _playerDataList.GetPlayerData(clientId);
        data.SetState(PlayerData.PlayerState.Playing);
        _playerDataList.UpdatePlayerData(data);
        
        UpdateSpecificClientPlayerData_ClientRpc(data, SendToAllClients());
        
        LogRpc(clientId + " is now playing");
    }

    private bool CanBecomePlayer() => gameState.Value == GameState.Lobby;

    private RpcParams SendToClientIDs(ulong[] clientIDs) => new RpcParams
    {
        Send = new RpcSendParams
        {
            Target = RpcTarget.Group(clientIDs, RpcTargetUse.Temp)
        }
    };
    public RpcParams SenderClientID(ulong clientID) => new RpcParams
    {
        Receive = new()
        {
            SenderClientId = clientID
        }
    };
    private RpcParams SendToAllClients() => new RpcParams
    {
        Send = new RpcSendParams
        {
            Target = RpcTarget.Everyone
        }
    };
    
    [Rpc(SendTo.Everyone)] public void LogRpc(string message) => Debug.Log($"<color=#29b929><b>{message}");

    private string GetRandomMap(string[] mapPool) => maps[Random.Range(0, mapPool.Length)];
    
    public enum GameState
    {
        Lobby,
        InGame,
        ChoosingUpgrade,
    }
}
