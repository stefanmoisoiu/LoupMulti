using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerDashboard : NetworkBehaviour
{
    [SerializeField] private string multiplayerLobbySceneName = "MultiLobby";
    [SerializeField] private string soloLobbySceneName = "SoloLobby";
    
    [SerializeField] private SceneChange sceneChange;
    
    [SerializeField] private TMP_InputField joinCodeInput;
    private int _maxCodeSize = 6;
    
    [Space]
    
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Color notEnoughPlayersColor, enoughPlayersColor;
    
    public enum DashboardState
    {
        CreateJoin,
        LobbyInfo
    }
    private DashboardState _dashboardState;
    
    [SerializeField] private CanvasGroup createGamePanel;
    [SerializeField] private CanvasGroup lobbyInfoPanel;

    private void Start()
    {
        joinCodeInput.characterLimit = _maxCodeSize;
    }
    private void OnEnable()
    {
        if (NetcodeManager.InGame)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerCountChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += PlayerCountChanged;
            
            UpdateLobbyDashboardInfo();
        }

        // NetcodeManager.OnCreateGame += () => ChangeDashboardState(DashboardState.LobbyInfo);
        // NetcodeManager.OnJoinGame += () => ChangeDashboardState(DashboardState.LobbyInfo);
        // NetcodeManager.OnLeaveGame += () => ChangeDashboardState(DashboardState.CreateJoin);
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= PlayerCountChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerCountChanged;
        }

        
        // NetcodeManager.OnCreateGame -= () => ChangeDashboardState(DashboardState.LobbyInfo);
        // NetcodeManager.OnJoinGame -= () => ChangeDashboardState(DashboardState.LobbyInfo);
        // NetcodeManager.OnLeaveGame -= () => ChangeDashboardState(DashboardState.CreateJoin);
    }

    public async void CreateGame()
    {
        Debug.Log("Create Game button pressed");
        SetDashboardEnabled(false);
        try
        {
            await NetcodeManager.Instance.CreateGame();
            CopyToClipboardJoinCode();
            sceneChange.ChangeScene(multiplayerLobbySceneName);

            // NetworkManager.Singleton.OnConnectionEvent += CreateOrJoinConnectionEvent;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            
        }
        SetDashboardEnabled(true);
    }
    
    public async void JoinGame()
    {
        Debug.Log("Join Game button pressed");
        SetDashboardEnabled(false);
        try
        {
            await NetcodeManager.Instance.JoinGame(joinCodeInput.text);
            sceneChange.ChangeScene(multiplayerLobbySceneName);
            // NetworkManager.Singleton.OnConnectionEvent += CreateOrJoinConnectionEvent;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        SetDashboardEnabled(true);
    }
    // private void CreateOrJoinConnectionEvent(NetworkManager arg1, ConnectionEventData arg2)
    // {
    //     Debug.Log("Connection Event: " + arg2.EventType);
    //     if (arg2.EventType == ConnectionEvent.ClientConnected)
    //     {
    //         NetworkManager.Singleton.OnConnectionEvent -= CreateOrJoinConnectionEvent;
    //         FinishedCreatingOrJoiningGame();
    //     }
    // }
    // private void FinishedCreatingOrJoiningGame()
    // {
    //     sceneChange.ChangeScene(multiplayerLobbySceneName);
    // }
    
    
    
    public void LeaveGame()
    {
        Debug.Log("Leave Game button pressed");
        
        NetcodeManager.Instance.LeaveGame();
        sceneChange.ChangeScene(soloLobbySceneName);
    }
    private void PlayerCountChanged(ulong playerID) => UpdateLobbyDashboardInfo();
    private void UpdateLobbyDashboardInfo()
    {
        int playerCount = NetworkManager.Singleton.GetCurrentPlayerCount();
        int maxPlayers = NetcodeManager.MaxPlayers;
        int minPlayers = NetcodeManager.MinPlayersToStart;
        bool enoughPlayers = playerCount >= minPlayers;
        playerCountText.text = $"{playerCount}/{maxPlayers}";
        playerCountText.color = enoughPlayers ? enoughPlayersColor : notEnoughPlayersColor;
        
        string joinCode = NetcodeManager.Instance.CurrentServerJoinCode;
        joinCodeText.text = joinCode;
    }

    
    private void ChangeDashboardState(DashboardState newState)
    {
        _dashboardState = newState;
        
        createGamePanel.alpha = _dashboardState == DashboardState.CreateJoin ? 1 : 0;
        createGamePanel.interactable = _dashboardState == DashboardState.CreateJoin;
        createGamePanel.blocksRaycasts = _dashboardState == DashboardState.CreateJoin;
        
        lobbyInfoPanel.alpha = _dashboardState == DashboardState.LobbyInfo ? 1 : 0; 
        lobbyInfoPanel.interactable = _dashboardState == DashboardState.LobbyInfo;
        lobbyInfoPanel.blocksRaycasts = _dashboardState == DashboardState.LobbyInfo;
        
        if (_dashboardState == DashboardState.LobbyInfo) UpdateLobbyDashboardInfo();
    }
    
    private void SetDashboardEnabled(bool enabled)
    {
        createGamePanel.interactable = enabled;
        lobbyInfoPanel.interactable = enabled;
    }
    
    public void CopyToClipboardJoinCode()
    {
        GUIUtility.systemCopyBuffer = NetcodeManager.Instance.CurrentServerJoinCode;
    }
}
