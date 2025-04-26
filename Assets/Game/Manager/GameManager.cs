using System;
using System.Collections;
using Base_Scripts;
using Game.Data;
using Game.Game_Loop;
using Unity.Netcode;
using UnityEngine;

namespace Game.Manager
{
    public class GameManager : NetworkBehaviour
    {


        public static GameManager Instance;
        public static event Action<GameManager> OnCreated;
    
        [SerializeField] private GameData gameData;
        public GameData GameData => gameData;
        [SerializeField] private UpgradesManager upgradesManager;
        public UpgradesManager UpgradesManager => upgradesManager;
        [SerializeField] private MapManager mapManager;
        public MapManager MapManager => mapManager;
        [SerializeField] private GameLoop gameLoop;
        public GameLoop GameLoop => gameLoop;

        public NetworkVariable<GameState> gameState = new();

        public static event Action<GameState, float> OnGameStateChanged;
        [Rpc(SendTo.Everyone)]
        private void OnGameStateChangedClientRpc(GameState newState, float serverTime)
            => OnGameStateChanged?.Invoke(newState, serverTime);

        public static event Action OnGameStartedServer;

        private void Awake()
        {
            Instance = this;
            OnCreated?.Invoke(this);
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                NetcodeLogger.Instance.LogRpc("Starting Game Manager", NetcodeLogger.LogType.Netcode);
        
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                NetworkObject.DestroyWithScene = false;
            }
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            Instance = null;
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
            StartCoroutine(StartGameCoroutine());
        }

        private IEnumerator StartGameCoroutine()
        {
            NetcodeLogger.Instance.LogRpc("Starting game", NetcodeLogger.LogType.Netcode);
        
            gameData.SetNotAssignedPlayersToPlayingState();
        
            // Load map et attendre
            bool mapLoaded = false;
            void OnMapLoaded(string mapName) => mapLoaded = true;
            MapManager.OnMapLoadedServer += OnMapLoaded;
            mapManager.LoadRandomGameMap();
            yield return new WaitUntil(() => mapLoaded);
            MapManager.OnMapLoadedServer -= OnMapLoaded;
        
            OnGameStateChangedClientRpc(GameState.InGame, NetworkManager.ServerTime.TimeAsFloat);
            OnGameStartedServer?.Invoke();
        
            gameLoop.StartGameLoop(this);
        }
        public enum GameState
        {
            Lobby,
            InGame
        }
    }
}
