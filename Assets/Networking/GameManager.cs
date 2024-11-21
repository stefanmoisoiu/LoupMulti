using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private string[] maps;
    
    public static GameManager instance;
    
    public NetworkVariable<GameState> gameState = new();
    private Dictionary<ulong, PlayerData> playerData;
    
    private const string LobbyMap = "MultiLobby";
    private string currentMap;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        NetworkObject.DestroyWithScene = false;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        instance = null;
        Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        NetworkObject.DestroyWithScene = false;
    }

    private void Start()
    {
        if (IsServer) SetupPlayerData();
        if(Input.GetKeyDown(KeyCode.U)) StartGameServerRpc();
        NetworkObject.DestroyWithScene = false;
    }
    private void SetupPlayerData()
    {
        playerData = new();
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            PlayerData data = new PlayerData(client);
            playerData.Add(client.ClientId, data);
        }
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerData.Remove(clientId);
    }

    private void OnClientConnected(ulong clientId)
    {
        PlayerData data = new PlayerData(NetworkManager.Singleton.ConnectedClients[clientId]);
        if (gameState.Value != GameState.Lobby) data.playerState = PlayerState.Spectating;
        
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
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnStartGameMapLoaded;
    }
    private void OnStartGameMapLoaded(ulong id, string sceneName, LoadSceneMode loadSceneMode)
    {
        LogRpc("Map loaded");
        
        SetPlayerSpawnPositions();
    }

    private void SetPlayerSpawnPositions()
    {
        ushort[] spawnIndexes = MapSpawnPositions.instance.GetTransformIndexes(playerData.Count);
        ulong[] clientIds = playerData.Keys.ToArray();
        for(int i = 0; i < playerData.Count; i++)
        {
            ushort spawnIndex = spawnIndexes[i];
            LogRpc("Setting spawn position of index " +  spawnIndex + " for " + clientIds[i]);
            ulong playerObjectID = playerData[clientIds[i]].client.PlayerObject.NetworkObjectId;
            SetPlayerSpawnPositionClientRpc(spawnIndex, playerObjectID);
        }
    }
    [Rpc(SendTo.Everyone)]
    private void SetPlayerSpawnPositionClientRpc(ushort index, ulong networkObjectID)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectID];
        Rigidbody rb = networkObject.GetComponent<Rigidbody>();
        Transform spawnPoint = MapSpawnPositions.instance.GetSpawnPoint(index);
        rb.position = spawnPoint.position;
        rb.linearVelocity = Vector3.zero;
        
        Debug.Log("Set spawn position of " + networkObject.OwnerClientId + " to " + spawnPoint.position);
    }

    private RpcParams ToClientIDs(ulong[] clientIDS) => new RpcParams
    {
        Send = new RpcSendParams
        {
            Target = RpcTarget.Group(clientIDS, RpcTargetUse.Temp)
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
        InGame,
        Spectating,
    }
    
    public struct PlayerData
    {
        public NetworkClient client;
        public PlayerState playerState;
        
        public PlayerData(NetworkClient client)
        {
            this.client = client;
            playerState = PlayerState.InGame;
        }
    }
}
