using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Smooth;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
    
    public Action OnGameStart;
    [Rpc(SendTo.Everyone)] private void OnGameStartClientRpc() => OnGameStart?.Invoke();

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
            LogRpc("Starting Game Manager", LogType.ServerInfo);
        
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkObject.DestroyWithScene = false;
        }
    }
    private void OnClientConnected(ulong clientId)
    {
        if (gameState.Value != GameState.Lobby) LatePlayerJoined(clientId);
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;
        if (gameData == null) return;
        
        if (!gameData.ServerSidePlayerDataList.ContainsPlayerData(clientId)) return;
        
        if (gameState.Value == GameState.Lobby)
        {
            gameData.ServerSidePlayerDataList.RemovePlayerData(clientId);
            gameData.UpdateEntireClientPlayerData_ClientRpc(gameData.ServerSidePlayerDataList.playerDatas.ToArray(), RpcParamsExt.Instance.SendToAllClients(NetworkManager.Singleton));
        }
        else gameData.SetPlayerState(PlayerOuterData.PlayerState.NotAssigned, clientId);
    }

    [Rpc(SendTo.Server)]
    public void StartGameServerRpc()
    {
        LogRpc("Starting game", LogType.ServerInfo);
        mapManager.LoadRandomGameMap();
        mapManager.OnMapLoadedServer += StartGameMapLoaded;
    }

    // SERVER SIDE
    private void StartGameMapLoaded(string mapName)
    {
        mapManager.OnMapLoadedServer -= StartGameMapLoaded;

        for (int i = 0; i < gameData.ServerSidePlayerDataList.playerDatas.Count; i++)
        {
            PlayerData data = new(gameData.ServerSidePlayerDataList.playerDatas[i]);
            data.OuterData = data.OuterData.SetState(PlayerOuterData.PlayerState.Playing);
            gameData.ServerSidePlayerDataList.playerDatas[i] = data;
        }
        gameData.UpdateEntireClientPlayerData_ClientRpc(gameData.ServerSidePlayerDataList.playerDatas.ToArray(),
            RpcParamsExt.Instance.SendToAllClients(NetworkManager.Singleton));
        
        gameState.Value = GameState.InGame;
        OnGameStartClientRpc();
        
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
            LogRpc("Round " + round, LogType.InGameInfo);
            mapManager.SetPlayerSpawnPositions();
            yield return ChooseUpgrade();
            yield return PlayRound();
            round++;
        }

        yield return EndGame();
        
        OnGameStateChangedClientRpc(GameState.Lobby, GameStateCallbackType.StateStarted);
    }
    
    private IEnumerator ChooseUpgrade()
    {
        LogRpc($"Choosing upgrade for {TimeToUpgrade} seconds...", LogType.InGameInfo);
        gameState.Value = GameState.ChoosingUpgrade;
        
        OnGameStateChangedClientRpc(GameState.ChoosingUpgrade, GameStateCallbackType.StateStarted);
        
        foreach (PlayerData data in gameData.ServerSidePlayerDataList.playerDatas)
        {
            if (data.OuterData.CurrentPlayerState == PlayerOuterData.PlayerState.SpectatingGame) continue;
            upgradesManager.ChooseRandomPlayerUpgradesServer(data);
        }
        yield return new WaitForSeconds(TimeToUpgrade);
        upgradesManager.UpgradeTimeFinished();
        upgradesManager.ResetChoices();
        OnGameStateChangedClientRpc(GameState.ChoosingUpgrade, GameStateCallbackType.StateEnded);
    }
    private IEnumerator PlayRound()
    {
        LogRpc($"Countdown of {StartCountdown} seconds", LogType.InGameInfo);
        gameState.Value = GameState.InGame;
        
        OnGameStateChangedClientRpc(GameState.InGame, GameStateCallbackType.StateStarted);
        
        yield return new WaitForSeconds(StartCountdown);
        
        LogRpc($"Playing round for {GameLength} seconds", LogType.InGameInfo);
        yield return new WaitForSeconds(GameLength);
        
        OnGameStateChangedClientRpc(GameState.InGame, GameStateCallbackType.StateEnded);
    }
    private IEnumerator EndGame()
    {
        LogRpc("Game ended", LogType.InGameInfo);
        gameState.Value = GameState.Lobby;
        yield break;
    }
    
    private void LatePlayerJoined(ulong clientId)
    {
        if (gameState.Value == GameState.Lobby) return;
        
        PlayerData data = gameData.ServerSidePlayerDataList.GetPlayerData(clientId);

        switch (gameState.Value)
        {
            case GameState.ChoosingUpgrade:
                upgradesManager.ChooseRandomPlayerUpgradesServer(data);
                break;
            case GameState.InGame:
                data.OuterData = data.OuterData.SetState(PlayerOuterData.PlayerState.SpectatingUntilNextRound);
                gameData.ServerSidePlayerDataList.UpdatePlayerData(data);
                break;
        }
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

    [Rpc(SendTo.Everyone)]
    public void LogRpc(string message, LogType type)
    {
        string color = "<color=#29b929>";
        switch (type)
        {
            case LogType.ServerInfo:
                color = "<color=#80eeee>";
                break;
            case LogType.Error:
                color = "<color=#ff0000>";
                break;
            case LogType.MapInfo:
                color = "<color=#ff00ff>";
                break;
            case LogType.GameDataInfo:
                color = "<color=#ff9900>";
                break;
            case LogType.UpgradeInfo:
                color = "<color=#ffcc00>";
                break;
        }
        
        Debug.Log($"{color}<b>{message}");
    }


    
    public enum GameState
    {
        Lobby,
        InGame,
        ChoosingUpgrade,
    }
}
