using System;
using System.Linq;
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

    [SerializeField] private TMP_Text changeStateText;
    

    [SerializeField] private CanvasGroup startButtonGroup;
    
    
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
    }

    public async void CreateGame()
    {
        Debug.Log("Create Game button pressed");
        SetDashboardEnabled(false);
        try
        {
            await NetcodeManager.Instance.CreateGame();
            CopyToClipboardJoinCode();
            sceneChange.NetworkChangeScene(multiplayerLobbySceneName);
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
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        SetDashboardEnabled(true);
    }
    public void LeaveGame()
    {
        Debug.Log("Leave Game button pressed");
        
        NetcodeManager.Instance.LeaveGame();
        sceneChange.ChangeScene(soloLobbySceneName);
    }
    public void StartGame()
    {
        Debug.Log("Start Game button pressed");
        GameManager.Instance.StartGameServerRpc();
        startButtonGroup.interactable = false;
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
        
        // startButtonGroup.interactable = enoughPlayers && NetworkManager.Singleton.IsHost;
        startButtonGroup.interactable = NetworkManager.Singleton.IsHost;
        
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
    public void ChangePlayerState()
    {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        PlayerData.PlayerState state = GameManager.Instance.gameData.myPlayerData.CurrentPlayerState;
        if (state == PlayerData.PlayerState.SpectatingGame)
        {
            GameManager.Instance.BecomePlayer(clientID);
            changeStateText.text = "Spectate";
            
        }
        else
        {
            GameManager.Instance.Spectate(clientID);
            changeStateText.text = "Be Player";
        }
    }
    
    public void CopyToClipboardJoinCode()
    {
        GUIUtility.systemCopyBuffer = NetcodeManager.Instance.CurrentServerJoinCode;
    }
}
