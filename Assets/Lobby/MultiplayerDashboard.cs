using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;

public class MultiplayerDashboard : MonoBehaviour
{
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
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong id) => UpdateLobbyDashboardInfo();
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong id) => UpdateLobbyDashboardInfo();
        
        joinCodeInput.characterLimit = _maxCodeSize;
    }

    public async void CreateGame()
    {
        Debug.Log("Create Game button pressed");
        
        try
        {
            SetDashboardEnabled(false);
            await NetcodeManager.Instance.CreateGame();
            ChangeDashboardState(DashboardState.LobbyInfo);
            SetDashboardEnabled(true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            SetDashboardEnabled(true);
        }
    }

    public async void JoinGame()
    {
        Debug.Log("Join Game button pressed");

        try
        {
            SetDashboardEnabled(false);
            await NetcodeManager.Instance.JoinGame(joinCodeInput.text);
            SetDashboardEnabled(true);
            ChangeDashboardState(DashboardState.LobbyInfo);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            SetDashboardEnabled(true);
        }
    }
    
    public void LeaveGame()
    {
        Debug.Log("Leave Game button pressed");
        
        NetcodeManager.Instance.LeaveGame();
        ChangeDashboardState(DashboardState.CreateJoin);
    }

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
