using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{


    public static GameManager Instance;
    public static Action<GameManager> OnCreated;
    public GameData gameData { get; private set; }
    public UpgradesManager upgradesManager { get; private set; }
    public MapManager mapManager { get; private set; }
    public GameLoop gameLoop { get; private set; }

    public NetworkVariable<GameState> gameState = new();

    public Action<GameState, float> OnGameStateChanged;
    [Rpc(SendTo.Everyone)]
    private void OnGameStateChangedClientRpc(GameState newState, float serverTime)
        => OnGameStateChanged?.Invoke(newState, serverTime);

    public Action OnGameStartedServer;

    private void Awake()
    {
        Instance = this;
        gameData = GetComponent<GameData>();
        upgradesManager = GetComponent<UpgradesManager>();
        mapManager = GetComponent<MapManager>();
        gameLoop = GetComponent<GameLoop>();
        OnCreated?.Invoke(this);
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.LogError($"IsServer: {IsServer}, IsClient: {IsClient}, IsHost: {IsHost}, IsOwner(of gameManager): {IsOwner}");

        if (IsServer)
        {
            NetcodeLogger.Instance.LogRpc("Starting Game Manager", NetcodeLogger.ColorType.Green);
        
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkObject.DestroyWithScene = false;
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Instance = null;
        gameData = null;
        upgradesManager = null;
        mapManager = null;
        Destroy(gameObject);
    }
    
    private void OnClientConnected(ulong clientId)
    {
        
    }
    private void OnClientDisconnected(ulong clientId)
    {
        
    }
    

    [Rpc(SendTo.Server)]
    public void StartGameServerRpc()
    {
        NetcodeLogger.Instance.LogRpc("Starting game", NetcodeLogger.ColorType.Green);
        
        gameData.SetNotAssignedPlayersToPlaying();
        
        mapManager.LoadRandomGameMap();
        mapManager.OnMapLoadedServer += StartGameMapLoadedServer;
        
        // When map is loaded call StartGameMapLoadedServer
        
    }
    private void StartGameMapLoadedServer(string mapName)
    {
        mapManager.OnMapLoadedServer -= StartGameMapLoadedServer;
        
        OnGameStateChangedClientRpc(GameState.InGame, NetworkManager.ServerTime.TimeAsFloat);
        OnGameStartedServer?.Invoke();
        
        gameLoop.StartGameLoop();
    }

    public enum GameState
    {
        Lobby,
        InGame
    }
}
