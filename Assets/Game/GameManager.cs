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
    public static GameData gameData;
    

    public NetworkVariable<GameState> gameState = new();

    
    public const int RoundCount = 2;
    public const int TimeToUpgrade = 20;
    public const int StartCountdown = 5;
    public const int GameLength = 60;

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
        gameData = GetComponent<GameData>();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        instance = null;
        gameData = null;
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
        
    }
    private void OnClientDisconnected(ulong clientId)
    {
        
    }
    
    

    [Rpc(SendTo.Server)]
    public void StartGameServerRpc()
    {
        gameState.Value = GameState.InGame;
        
        string map = GetRandomMap(maps);
        NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
        currentMap = map;
        LogRpc("Loading map " + map + "...", LogType.ServerInfo);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += StartGameMapLoaded;
    }

    // SERVER SIDE
    private void StartGameMapLoaded(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        LogRpc("Map loaded", LogType.ServerInfo);
        OnMapLoadedClientRpc(currentMap);
        
        SetPlayerSpawnPositions();

        foreach (PlayerData data in gameData.ServerSidePlayerDataList.playerDatas)
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
        ushort[] spawnIndexes = MapSpawnPositions.instance.GetTransformIndexes(gameData.ServerSidePlayerDataList.playerDatas.Count);
        for(int i = 0; i < gameData.ServerSidePlayerDataList.playerDatas.Count; i++)
        {
            PlayerData data = gameData.ServerSidePlayerDataList.playerDatas[i];
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
            LogRpc("Round " + round, LogType.InGameInfo);
            yield return ChooseUpgrade();
            yield return PlayRound();
            round++;
        }

        yield return EndGame();
    }
    
    private IEnumerator ChooseUpgrade()
    {
        LogRpc($"Choosing upgrade for {TimeToUpgrade} seconds...", LogType.InGameInfo);
        gameState.Value = GameState.ChoosingUpgrade;
        yield return new WaitForSeconds(TimeToUpgrade);
    }
    private IEnumerator PlayRound()
    {
        LogRpc($"Countdown of {StartCountdown} seconds", LogType.InGameInfo);
        gameState.Value = GameState.InGame;
        yield return new WaitForSeconds(StartCountdown);
        
        LogRpc($"Playing round for {GameLength} seconds", LogType.InGameInfo);
        yield return new WaitForSeconds(GameLength);
    }
    private IEnumerator EndGame()
    {
        LogRpc("Game ended", LogType.InGameInfo);
        gameState.Value = GameState.Lobby;
        yield break;
    }
    
    
    public void Spectate(ulong clientId) => SpectateServerRpc(RpcParamsExt.Instance.SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void SpectateServerRpc(RpcParams @params)
    {
        gameData.SetPlayerState(PlayerData.PlayerState.Spectating, @params.Receive.SenderClientId);
    }
    public void BecomePlayer(ulong clientId) => BecomePlayerServerRpc(RpcParamsExt.Instance.SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void BecomePlayerServerRpc(RpcParams @params)
    {
        if (!CanBecomePlayer()) return;
        
        gameData.SetPlayerState(PlayerData.PlayerState.Playing, @params.Receive.SenderClientId);
    }
    private bool CanBecomePlayer() => GameManager.instance.gameState.Value == GameManager.GameState.Lobby;
    

    
    public enum LogType
    {
        ServerInfo,
        InGameInfo,
        Error,
    }

    [Rpc(SendTo.Everyone)]
    public void LogRpc(string message, LogType type)
    {
        string color = "<color=#29b929>";
        if (type == LogType.ServerInfo) color = "<color=#80eeee>";
        else if (type == LogType.Error) color = "<color=#ff0000>";
        
        Debug.Log($"{color}<b>{message}");
    }

    private string GetRandomMap(string[] mapPool) => maps[Random.Range(0, mapPool.Length)];
    
    public enum GameState
    {
        Lobby,
        InGame,
        ChoosingUpgrade,
    }
}
