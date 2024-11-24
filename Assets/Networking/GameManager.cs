using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Smooth;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private string[] maps;
    
    public static GameManager instance;
    
    public NetworkVariable<GameState> gameState = new();
    public PlayerData myPlayerData;
    private Dictionary<ulong, PlayerData> playerData;
    
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
        SetupPlayerData();
        NetworkObject.DestroyWithScene = false;
    }
    private void SetupPlayerData()
    {
        playerData = new();
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            PlayerData data = new PlayerData(client);
            playerData.Add(client.ClientId, data);
            
            UpdateMyPlayerDataClientRpc(ToClientIDs(new [] {client.ClientId}));
        }
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void UpdateMyPlayerDataClientRpc(RpcParams @params)
    {
        myPlayerData = playerData[NetworkManager.Singleton.LocalClientId];
        LogRpc("My player data set for " + myPlayerData.ClientID);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerData.Remove(clientId);
    }

    private void OnClientConnected(ulong clientId)
    {
        PlayerData data = new PlayerData(NetworkManager.Singleton.ConnectedClients[clientId]);
        if (gameState.Value != GameState.Lobby) Spectate(clientId);
        
        playerData.Add(clientId, data);
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

        foreach (ulong clientID in playerData.Keys)
        {
            PlayerData data = playerData[clientID];
            data.ResetGameData();
            
            if (data.playerState == PlayerState.Spectating)
            {
                
            }
            else
            {
                if (data.playerState == PlayerState.NotAssigned) data.SetState(PlayerState.Playing);
            }
            UpdateMyPlayerDataClientRpc(ToClientIDs(new[] {clientID}));
        }
        
        OnGameStartClientRpc();
    }
    

    private void SetPlayerSpawnPositions()
    {
        ushort[] spawnIndexes = MapSpawnPositions.instance.GetTransformIndexes(playerData.Count);
        ulong[] clientIds = playerData.Keys.ToArray();
        for(int i = 0; i < playerData.Count; i++)
        {
            ushort spawnIndex = spawnIndexes[i];
            LogRpc("Setting spawn position of index " +  spawnIndex + " for " + clientIds[i]);
            
            Transform spawnPoint = MapSpawnPositions.instance.GetSpawnPoint(spawnIndex);
            SmoothSyncNetcode sync = playerData[clientIds[i]].client.PlayerObject.GetComponent<SmoothSyncNetcode>();
            sync.teleportAnyObjectFromServer(spawnPoint.position, spawnPoint.rotation, Vector3.one);
        }
    }
    
    public void Spectate(ulong clientId) => SpectateServerRpc(SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void SpectateServerRpc(RpcParams @params)
    {
        ulong clientId = @params.Receive.SenderClientId;
        playerData[clientId].SetState(PlayerState.Spectating);
        
        UpdateMyPlayerDataClientRpc(ToClientIDs(new[] {clientId}));
        
        LogRpc(clientId + " is now spectating");
    }
    public void BecomePlayer(ulong clientId) => BecomePlayerServerRpc(SenderClientID(clientId));
    [Rpc(SendTo.Server)]
    private void BecomePlayerServerRpc(RpcParams @params)
    {
        if (!CanBecomePlayer()) return;
        
        ulong clientId = @params.Receive.SenderClientId;
        playerData[clientId].SetState(PlayerState.Playing);
        
        UpdateMyPlayerDataClientRpc(ToClientIDs(new[] {clientId}));
        
        LogRpc(clientId + " is now playing");
    }

    private bool CanBecomePlayer() => gameState.Value == GameState.Lobby;

    private RpcParams ToClientIDs(ulong[] clientIDS) => new RpcParams
    {
        Send = new RpcSendParams
        {
            Target = RpcTarget.Group(clientIDS, RpcTargetUse.Temp)
        }
    };
    public RpcParams SenderClientID(ulong clientID) => new RpcParams
    {
        Receive = new()
        {
            SenderClientId = clientID
        }
    };
    
    [Rpc(SendTo.Everyone)] private void LogRpc(string message) => Debug.Log(message);

    private string GetRandomMap(string[] mapPool) => maps[Random.Range(0, mapPool.Length)];
    
    public enum GameState
    {
        Lobby,
        InGame,
        ChoosingUpgrade,
    }

    public enum PlayerState
    {
        NotAssigned,
        Playing,
        Spectating,
    }
    
    [Serializable]
    public class PlayerData
    {
        public ulong ClientID => client.ClientId;
        public NetworkClient client { get; private set; }
        public PlayerState playerState { get; private set; }
        public PlayerGameData gameData { get; private set; }
        

        public PlayerData(NetworkClient client)
        {
            this.client = client;
            playerState = PlayerState.NotAssigned;
            gameData = new PlayerGameData();
        }

        public void ResetGameData()
        {
            gameData = new();
        }
        
        public void SetState(PlayerState state) => playerState = state;
    }

    [Serializable]
    public struct PlayerGameData
    {
        public int Score { get; private set; }
        
        public void AddScore(int amount) => Score += amount;
        public void RemoveScore(int amount) => Score -= amount;
        public void ResetScore() => Score = 0;
    }
}
