using System;
using System.Collections;
using Base_Scripts;
using Game.Data;
using Game.Data.Extensions;
using Game.Maps;
using Game.Upgrade.Perks;
using Game.Upgrade.Shop;
using Unity.Netcode;
using UnityEngine;

namespace Game.Game_Loop
{
    public class GameManager : NetworkBehaviour
    {


        public static GameManager Instance;
        public static event Action<GameManager> OnCreated;
        [SerializeField] private ItemSelectionManager itemSelectionManager;
        public ItemSelectionManager ItemSelectionManager => itemSelectionManager;
        [SerializeField] private ShopManager shopManager;
        public ShopManager ShopManager => shopManager;
        [SerializeField] private MapManager mapManager;
        public MapManager MapManager => mapManager;
        [SerializeField] private GameLoop gameLoop;
        public GameLoop GameLoop => gameLoop;

        private readonly NetworkVariable<GameState> _gameState = new();
        public static GameState CurrentGameState => Instance == null ? GameState.NotConnected : Instance._gameState.Value;
        public static event Action<GameState,GameState> OnGameStateChanged;
        private void GameStateChanged(GameState oldState, GameState newState) => OnGameStateChanged?.Invoke(oldState, newState);

        public static event Action OnGameStartedServer;
        public static event Action OnGameEndedServer;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _gameState.OnValueChanged += GameStateChanged;
            
            Instance = this;
            OnCreated?.Invoke(this);
        }

        private void OnDisable()
        {
            _gameState.OnValueChanged -= GameStateChanged;
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
            
            _gameState.Value = GameState.Loading;
        
            DataManager.Instance.PlayerState.SetNotAssignedPlayersToPlayingState();
        
            yield return mapManager.LoadRandomGameMap();

            yield return mapManager.SetPlayerSpawnPositions();
            
            _gameState.Value = GameState.InGame;
            OnGameStartedServer?.Invoke();
        
            yield return gameLoop.MainLoop(this);
            
            OnGameEndedServer?.Invoke();
            
            _gameState.Value = GameState.Loading;
            
            yield return mapManager.LoadLobbyMap();
            
            _gameState.Value = GameState.Lobby;
            
            DataManager.Instance.Reset();
        }
        public enum GameState
        {
            Lobby,
            Loading,
            InGame,
            NotConnected
        }
    }
}
