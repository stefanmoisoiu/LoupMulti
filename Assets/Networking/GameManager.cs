using System;
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
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (IsServer) SetupPlayerData();
        if(Input.GetKeyDown(KeyCode.U)) StartGameServerRpc();
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
        Debug.Log("Starting game");
        gameState.Value = GameState.InGame;
        
        string map = GetRandomMap(maps);
        NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
        currentMap = map;
        Debug.Log("Loading map " + map + "...");
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnStartGameMapLoaded;
    }
    private void OnStartGameMapLoaded(ulong id, string sceneName, LoadSceneMode loadSceneMode)
    {
        Debug.Log("Map loaded");
        
        SetPlayerSpawnPositions();
    }

    private void SetPlayerSpawnPositions()
    {
        ushort[] spawnIndexes = MapSpawnPositions.instance.GetTransformIndexes(playerData.Count);
        ulong[] clientIds = playerData.Keys.ToArray();
        for(int i = 0; i < playerData.Count; i++)
        {
            RpcParams @params = new RpcParams
            {
                Send = new RpcSendParams
                {
                    Target = RpcTarget.Single(clientIds[i], RpcTargetUse.Temp)
                }
            };
            SetPlayerSpawnPositionClientRpc(spawnIndexes[i], @params);
        }
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void SetPlayerSpawnPositionClientRpc(ushort index, RpcParams @params = default)
    {
        Debug.Log("Spawn Index : " + index);
    }

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
