using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetcodeManager : MonoBehaviour
{
    public static NetcodeManager Instance { get; private set; }
    public static RelayServerData ServerData { get; private set; }
    public const int MaxPlayers = 4;
    public const int MinPlayersToStart = 2;
    
    public static bool InGame { get; private set; }
    public static bool LoadingGame { get; private set; }
    
    public static Action OnEnterGame, OnCreateGame, OnJoinGame, OnLeaveGame;
    private void Awake()
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

    public async Task<bool> CreateGame()
    {
        bool previousInGame = InGame;
        try
        {
            LoadingGame = true;
            InGame = true;
            
            Allocation alloc = await RelayManager.CreateRelay(MaxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            
            CurrentServerJoinCode = joinCode;
            ServerData = relayServerData;
            
            LoadingGame = false;
            
            OnCreateGame?.Invoke();
            OnEnterGame?.Invoke();
            return true;
        }
        catch
        {
            Debug.LogError("Failed to create game");
            InGame = previousInGame;
            LoadingGame = false;
            
            return false;
        }
    }
    public async Task JoinGame(string joinCode)
    {
        bool previousInGame = InGame;
        if (string.IsNullOrEmpty(joinCode) || joinCode.Length != 6)
        {
            throw new Exception("Invalid join code");
        }
        try
        {
            InGame = true;
            
            JoinAllocation joinAlloc = await RelayManager.JoinRelayByCode(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAlloc, "dtls");
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        
            NetworkManager.Singleton.StartClient();
            
            CurrentServerJoinCode = joinCode;
            ServerData = relayServerData;
            
            LoadingGame = false;
            
            OnJoinGame?.Invoke();
            OnEnterGame?.Invoke();
            
            NetworkManager.Singleton.OnClientStopped += ClientStoppedLeaveGame;
        }
        catch
        {
            InGame = previousInGame;
            LoadingGame = false;
            
            throw new Exception("Failed to join game");
        }
    }
    
    public string CurrentServerJoinCode { get; private set; }

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