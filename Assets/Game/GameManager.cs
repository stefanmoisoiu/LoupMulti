using System;
using System.Collections;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{


    public static GameManager Instance;
    public static Action<GameManager> OnCreated;
    public GameData gameData { get; private set; }
    public UpgradesManager upgradesManager { get; private set; }
    public MapManager mapManager { get; private set; }
    

    public NetworkVariable<GameState> gameState = new();

    public enum GameStateCallbackType
    {
        StateStarted,
        StateEnded,
    }
    public Action<GameState, GameStateCallbackType> OnGameStateChanged;
    [Rpc(SendTo.Everyone)] private void OnGameStateChangedClientRpc(GameState state, GameStateCallbackType type) => OnGameStateChanged?.Invoke(state, type);

    [BoxGroup("Game Info")][Range(0,10)][SerializeField] private int roundCount = 2;
    [BoxGroup("Game Info")][SerializeField] private int timeToUpgrade = 20;
    [BoxGroup("Game Info")][SerializeField] private int startCountdown = 5;
    [BoxGroup("Game Info")][SerializeField] private int gameLength = 60;
    
    public int RoundCount => roundCount;
    public int TimeToUpgrade => timeToUpgrade;
    public int StartCountdown => startCountdown;
    public int GameLength => gameLength;

    private Coroutine _gameLoopCoroutine;
    
    public Action OnGameStartedServer, OnStartPlayRound, OnEndPlayRound, OnGameEndedServer;

    private void Awake()
    {
        Instance = this;
        gameData = GetComponent<GameData>();
        upgradesManager = GetComponent<UpgradesManager>();
        mapManager = GetComponent<MapManager>();
        OnCreated?.Invoke(this);
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetcodeLogger.Instance.LogRpc("Starting Game Manager", NetcodeLogger.ColorType.Green);
        
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkObject.DestroyWithScene = false;
        }
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
        mapManager.LoadRandomGameMap();
        mapManager.OnMapLoadedServer += StartGameMapLoadedServer;
        
        OnGameStartedServer?.Invoke();
    }

    private void StartGameMapLoadedServer(string mapName)
    {
        mapManager.OnMapLoadedServer -= StartGameMapLoadedServer;
        
        gameState.Value = GameState.InGame;
        
        if (_gameLoopCoroutine != null) StopCoroutine(_gameLoopCoroutine);
        _gameLoopCoroutine = StartCoroutine(GameLoop());
    }



    private IEnumerator GameLoop()
    {
        OnGameStateChangedClientRpc(GameState.Lobby, GameStateCallbackType.StateEnded);
        
        // Choose Upgrade -> Play Round -> Repeat
        int round = 1;
        
        while (round <= RoundCount)
        {
            OnStartPlayRound?.Invoke();
            
            NetcodeLogger.Instance.LogRpc("Round " + round, NetcodeLogger.ColorType.Green);
            
            mapManager.SetPlayerSpawnPositions();
            yield return ChooseUpgrade();
            
            yield return PlayRound();
            round++;
            
            OnEndPlayRound?.Invoke();
        }

        yield return EndGame();
        
        OnGameEndedServer?.Invoke();
        
        OnGameStateChangedClientRpc(GameState.Lobby, GameStateCallbackType.StateStarted);
    }
    
    private IEnumerator ChooseUpgrade()
    {
        NetcodeLogger.Instance.LogRpc($"Choosing upgrade for {TimeToUpgrade} seconds...", NetcodeLogger.ColorType.Green);
        gameState.Value = GameState.ChoosingUpgrade;
        
        OnGameStateChangedClientRpc(GameState.ChoosingUpgrade, GameStateCallbackType.StateStarted);

        upgradesManager.ChooseUpgradesForPlayingPlayersServer();
        yield return new WaitForSeconds(TimeToUpgrade);
        upgradesManager.UpgradeTimeFinishedServer();
        
        OnGameStateChangedClientRpc(GameState.ChoosingUpgrade, GameStateCallbackType.StateEnded);
    }
    private IEnumerator PlayRound()
    {
        NetcodeLogger.Instance.LogRpc($"Countdown of {StartCountdown} seconds", NetcodeLogger.ColorType.Green);
        gameState.Value = GameState.InGame;
        
        OnGameStateChangedClientRpc(GameState.InGame, GameStateCallbackType.StateStarted);
        
        yield return new WaitForSeconds(StartCountdown);
        
        NetcodeLogger.Instance.LogRpc($"Playing round for {GameLength} seconds", NetcodeLogger.ColorType.Green);
        yield return new WaitForSeconds(GameLength);
        
        OnGameStateChangedClientRpc(GameState.InGame, GameStateCallbackType.StateEnded);
    }
    private IEnumerator EndGame()
    {
        NetcodeLogger.Instance.LogRpc("Game ended", NetcodeLogger.ColorType.Green);
        gameState.Value = GameState.Lobby;
        yield break;
    }
    
    public void Spectate(ulong clientId) => SpectateServerRpc(RpcParamsExt.Instance.SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void SpectateServerRpc(RpcParams @params)
    {
        gameData.SetPlayerState(PlayerOuterData.PlayerState.SpectatingGame, @params.Receive.SenderClientId);
    }
    public void BecomePlayer(ulong clientId) => BecomePlayerServerRpc(RpcParamsExt.Instance.SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void BecomePlayerServerRpc(RpcParams @params)
    {
        if (!CanBecomePlayer()) return;
        
        gameData.SetPlayerState(PlayerOuterData.PlayerState.Playing, @params.Receive.SenderClientId);
    }
    private bool CanBecomePlayer() => Instance.gameState.Value == GameState.Lobby;
    

    
    public enum LogType
    {
        ServerInfo,
        InGameInfo,
        MapInfo,
        GameDataInfo,
        UpgradeInfo,
        Error,
    }



    
    public enum GameState
    {
        Lobby,
        InGame,
        ChoosingUpgrade,
    }
}
