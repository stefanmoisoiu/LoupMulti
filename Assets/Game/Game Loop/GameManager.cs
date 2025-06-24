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
        [SerializeField] private PerkSelectionManager perkSelectionManager;
        public PerkSelectionManager PerkSelectionManager => perkSelectionManager;
        [SerializeField] private ShopManager shopManager;
        public ShopManager ShopManager => shopManager;
        [SerializeField] private MapManager mapManager;
        public MapManager MapManager => mapManager;
        [SerializeField] private GameLoop gameLoop;
        public GameLoop GameLoop => gameLoop;

        public NetworkVariable<GameState> gameState = new(GameState.Lobby, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public static event Action<GameState, float> OnGameStateChanged;
        [Rpc(SendTo.Everyone)]
        private void OnGameStateChangedClientRpc(GameState newState, float serverTime)
            => OnGameStateChanged?.Invoke(newState, serverTime);

        public static event Action OnGameStartedServer;
        public static event Action OnGameEndedServer;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
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
        
            DataManager.Instance.PlayerState.SetNotAssignedPlayersToPlayingState();
        
            yield return mapManager.LoadRandomGameMap();

            gameState.Value = GameState.InGame;
            OnGameStateChangedClientRpc(GameState.InGame, NetworkManager.ServerTime.TimeAsFloat);
            OnGameStartedServer?.Invoke();
        
            MapManager.SetPlayerSpawnPositions();
            yield return gameLoop.MainLoop(this);
            
            gameState.Value = GameState.Lobby;
            OnGameStateChangedClientRpc(GameState.Lobby, NetworkManager.ServerTime.TimeAsFloat);
            OnGameEndedServer?.Invoke();
            
            yield return MapManager.LoadLobbyMap();
            DataManager.Instance.Setup();
        }
        public enum GameState
        {
            Lobby,
            InGame
        }
    }
}
