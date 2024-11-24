using System;
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
    public NetworkVariable<PlayerDataList> playerDataList = new (readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
    
    public PlayerDataList editorPreviewPlayerDataList;
    
    public const int RoundCount = 10;
    public const int TimeToUpgrade = 20;
    public const int startCountdown = 5;

    private Coroutine _gameCoroutine;
    private void Update()
    {
        editorPreviewPlayerDataList = playerDataList.Value;
    }

    private const string LobbyMap = "MultiLobby";
    private string currentMap;
    
    public Action<string> OnMapLoaded;
    [Rpc(SendTo.Everyone)] private void OnMapLoadedClientRpc(string map) => OnMapLoaded?.Invoke(map);
    
    public Action OnGameStart;
    [Rpc(SendTo.Everyone)] private void OnGameStartClientRpc() => OnGameStart?.Invoke();
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        instance = null;
        Destroy(gameObject);
    }

    private void Start()
    {
        if (!IsServer) return;
        
        LogRpc("Server started");
        
        playerDataList.Value = new PlayerDataList();
        playerDataList.Value = playerDataList.Value.SetupPlayerData();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkObject.DestroyWithScene = false;
    }

    private void OnClientConnected(ulong clientId)
    {
        LogRpc("Client connected: " + clientId);
        PlayerData data = new PlayerData(NetworkManager.Singleton.ConnectedClients[clientId]);
        if (gameState.Value != GameState.Lobby) Spectate(clientId);
        
        playerDataList.Value = playerDataList.Value.AddPlayerData(data);
    }
    private void OnClientDisconnected(ulong clientId)
    {
        LogRpc("Client disconnected: " + clientId);
        playerDataList.Value = playerDataList.Value.RemovePlayerData(clientId);
    }

    [Rpc(SendTo.Server)]
    public void StartGameServerRpc()
    {
        LogRpc("Starting game");
        gameState.Value = GameState.InGame;
        
        string map = GetRandomMap(maps);
        NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
        currentMap = map;
        LogRpc("Loading map " + map + "...");
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += StartGameMapLoaded;
    }

    private void StartGameMapLoaded(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        LogRpc("Map loaded");
        OnMapLoadedClientRpc(currentMap);
        
        SetPlayerSpawnPositions();

        foreach (PlayerData data in playerDataList.Value.playerDatas)
        {
            data.ResetGameData();
            
            if (data.playerState == PlayerData.PlayerState.Spectating)
            {
                
            }
            else
            {
                if (data.playerState == PlayerData.PlayerState.NotAssigned) data.SetState(PlayerData.PlayerState.Playing);
            }
        }
        
        OnGameStartClientRpc();
    }
    

    private void SetPlayerSpawnPositions()
    {
        ushort[] spawnIndexes = MapSpawnPositions.instance.GetTransformIndexes(playerDataList.Value.playerDatas.Length);
        for(int i = 0; i < playerDataList.Value.playerDatas.Length; i++)
        {
            ushort spawnIndex = spawnIndexes[i];
            LogRpc("Setting spawn position of index " +  spawnIndex + " for " + playerDataList.Value.playerDatas[i].clientId);
            
            Transform spawnPoint = MapSpawnPositions.instance.GetSpawnPoint(spawnIndex);
            SmoothSyncNetcode sync = NetworkManager.Singleton.ConnectedClients[playerDataList.Value.playerDatas[i].clientId].PlayerObject.GetComponent<SmoothSyncNetcode>();
            sync.teleportAnyObjectFromServer(spawnPoint.position, spawnPoint.rotation, Vector3.one);
        }
    }
    
    public void Spectate(ulong clientId) => SpectateServerRpc(SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void SpectateServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        
        PlayerData data = playerDataList.Value.GetPlayerData(clientId);
        data.SetState(PlayerData.PlayerState.Spectating);
        playerDataList.Value = playerDataList.Value.UpdatePlayerData(data);
        
        LogRpc(clientId + " is now spectating");
    }
    public void BecomePlayer(ulong clientId) => BecomePlayerServerRpc(SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void BecomePlayerServerRpc(RpcParams @params)
    {
        if (!CanBecomePlayer()) return;
        
        ulong clientId = @params.Receive.SenderClientId;
        
        PlayerData data = playerDataList.Value.GetPlayerData(clientId);
        data.SetState(PlayerData.PlayerState.Playing);
        playerDataList.Value.UpdatePlayerData(data);
        
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
    
    [Rpc(SendTo.Everyone)] public void LogRpc(string message) => Debug.Log($"<color=#29b929>{message}");

    private string GetRandomMap(string[] mapPool) => maps[Random.Range(0, mapPool.Length)];
    
    public enum GameState
    {
        Lobby,
        InGame,
        ChoosingUpgrade,
    }
}
