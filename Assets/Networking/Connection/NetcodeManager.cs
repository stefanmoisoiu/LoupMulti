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
            
            Allocation alloc = await RelayManager.CreateRelay(MaxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            Debug.Log("Starting Host...");
            
            bool success = NetworkManager.Singleton.StartHost();
            if (!success) throw new Exception("Failed to start host");
            
            Debug.Log("Host started");
            
            CurrentServerJoinCode = joinCode;
            ServerData = relayServerData;
                
            IEnumerator RunCoroutine(IEnumerator c, TaskCompletionSource<bool> tcs)
            {
                yield return StartCoroutine(c);
                tcs.SetResult(true);
            }
                
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(RunCoroutine(LocalSceneChanger.Instance.LocalLoadScene(NetSceneChangerName, SceneType.Manager), tcs));
            await tcs.Task;
            
            await Task.Yield(); 
            
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

                
                TaskCompletionSource<bool> tcs = new();
                IEnumerator TransitionCoroutine(bool fadeIn)
                {
                    yield return TransitionManager.Instance.TransitionCoroutine(fadeIn);
                    tcs.SetResult(true);
                }
                TransitionManager.Instance.StartCoroutine(TransitionCoroutine(true));
                await tcs.Task;
            
                bool success = NetworkManager.Singleton.StartClient();
                if (!success) Debug.LogError("Failed to start client");
            
                CurrentServerJoinCode = joinCode;
                ServerData = relayServerData;
            
                InGame = true;
                LoadingGame = false;
            
                OnJoinGame?.Invoke();
                OnEnterGame?.Invoke();
                
                TransitionManager.Instance.TransitionCoroutine(false);
                
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