using System;
using System.Collections;
using System.Threading.Tasks;
using Rendering.Transitions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Connection
{
    public class NetcodeManager : MonoBehaviour
    {
        public static NetcodeManager Instance { get; private set; }
        public static RelayServerData ServerData { get; private set; }
        public const int MaxPlayers = 4;
        public const int MinPlayersToStart = 2;
    
        public const string NetManagers = "NetManagers";
        public const string MultiplayerLobbySceneName = "MultiLobby";
        public const string NetSceneChangerName = "NetSceneChanger";
        
        public static bool InGame { get; private set; }
        public static bool LoadingGame { get; private set; }
        public string CurrentServerJoinCode { get; private set; }
    
        public static event Action OnEnterGame, OnCreateGame, OnJoinGame, OnLeaveGame;
        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

    public async Task CreateGame()
        {
            LoadingGame = true;
            
            // 1. Logique Relay
            Allocation alloc = await RelayManager.CreateRelay(MaxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            Debug.Log("Starting Host...");

            // 2. Attendre que Netcode soit prêt
            var serverStartedTcs = new TaskCompletionSource<bool>();
            Action onServerStarted = null;
            onServerStarted = () =>
            {
                NetworkManager.Singleton.OnServerStarted -= onServerStarted;
                serverStartedTcs.SetResult(true);
            };
            
            NetworkManager.Singleton.OnServerStarted += onServerStarted;

            if (!NetworkManager.Singleton.StartHost())
            {
                NetworkManager.Singleton.OnServerStarted -= onServerStarted;
                throw new Exception("Failed to start host");
            }
            
            await serverStartedTcs.Task; 
            
            Debug.Log("Host started successfully.");
            
            CurrentServerJoinCode = joinCode;
            ServerData = relayServerData;
            
            
            IEnumerator RunCoroutine(IEnumerator c, TaskCompletionSource<bool> tcs)
            {
                yield return StartCoroutine(c);
                tcs.SetResult(true);
            }
            TaskCompletionSource<bool> tcs = new();
            // StartCoroutine(RunCoroutine(LocalSceneChanger.Instance.LocalLoadScene(NetSceneChangerName, SceneType.Manager), tcs));
            // await tcs.Task;

            NetworkManager.Singleton.SceneManager.LoadScene(NetSceneChangerName, LoadSceneMode.Additive);

            int waitFrames = 0;
            while (NetcodeSceneChanger.Instance == null)
            {
                await Task.Yield();
                waitFrames++;
                if (waitFrames > 9999)
                {
                    throw new Exception("Timeout: NetcodeSceneChanger.Instance n'a pas été initialisé.");
                }
            }
            Debug.Log($"NetcodeSceneChanger.Instance initialisé après {waitFrames} frames.");
            
            tcs = new TaskCompletionSource<bool>();
            StartCoroutine(RunCoroutine(NetcodeSceneChanger.Instance.NetcodeLoadScene(NetManagers, SceneType.Manager), tcs));
            await tcs.Task;
            
            tcs = new TaskCompletionSource<bool>();
            StartCoroutine(RunCoroutine(NetcodeSceneChanger.Instance.NetcodeLoadScene(MultiplayerLobbySceneName, SceneType.Active), tcs));
            await tcs.Task;
                
            LoadingGame = false;
            InGame = true;
            
            OnCreateGame?.Invoke();
            OnEnterGame?.Invoke();
                
            Debug.Log("Created game with join code: " + joinCode);
        }
        
        public async Task JoinGame(string joinCode)
        {
            if (string.IsNullOrEmpty(joinCode) || joinCode.Length != 6)
            {
                throw new Exception("Invalid join code");
            }
            try
            {
                LoadingGame = true;
            
                JoinAllocation joinAlloc = await RelayManager.JoinRelayByCode(joinCode);
                RelayServerData relayServerData = new (joinAlloc, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
                Debug.Log($"Attempting StartClient. IsClient={NetworkManager.Singleton.IsClient}, IsHost={NetworkManager.Singleton.IsHost}, IsServer={NetworkManager.Singleton.IsServer}");
                bool success = NetworkManager.Singleton.StartClient();
                if (!success) Debug.LogError("Failed to start client");
            
                CurrentServerJoinCode = joinCode;
                ServerData = relayServerData;
            
                InGame = true;
                LoadingGame = false;
            
                OnJoinGame?.Invoke();
                OnEnterGame?.Invoke();
                
                NetworkManager.Singleton.OnClientStopped += ClientStoppedLeaveGame;
            }
            catch
            {
                LoadingGame = false;
            
                throw new Exception("Failed to join game");
            }
        }
    

        private void ClientStoppedLeaveGame(bool _) => LeaveGame();
        public void LeaveGame()
        {
            NetworkManager.Singleton.OnClientStopped -= ClientStoppedLeaveGame;
        
            NetworkManager.Singleton.Shutdown();
            CurrentServerJoinCode = null;
        
            InGame = false;
            LoadingGame = false;
        
            OnLeaveGame?.Invoke();
        }
    }
}