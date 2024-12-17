using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PScoreboard : PNetworkBehaviour
{
    [SerializeField] private GameObject scorePrefab;
    private List<GameObject> scores = new();
    private const string ScoreboardTag = "Scoreboard";
    
    private Transform _scoreboard;
    
    private Transform _scoreboardLayout;
    private CanvasGroup _scoreboardCanvasGroup;
    private TMP_Text _spectatorText;
    private TMP_Text _spectatorList;
    

    protected override void StartOnlineOwner()
    {
        _scoreboard = GameObject.FindGameObjectWithTag(ScoreboardTag).transform;
        _scoreboardCanvasGroup = _scoreboard.GetComponent<CanvasGroup>();
        _scoreboardLayout = _scoreboard.GetChild(0);
        _spectatorText = _scoreboard.GetChild(1).GetComponent<TMP_Text>();
        _spectatorList = _scoreboard.GetChild(2).GetComponent<TMP_Text>();
        
        InputManager.instance.OnScoreboard += ShowScoreboard;
        InputManager.instance.OnStopScoreboard += HideScoreboard;
        
        HideScoreboard();
    }

    protected override void DisableOnlineOwner()
    {
        InputManager.instance.OnScoreboard -= ShowScoreboard;
        InputManager.instance.OnStopScoreboard -= HideScoreboard;
        
        HideScoreboard();
    }

    private void ShowScoreboard()
    {
        _scoreboardCanvasGroup.alpha = 1;
    }
    private void HideScoreboard()
    {
        _scoreboardCanvasGroup.alpha = 0;
    }
    private void UpdateScoreboard(List<PlayerData> newPlayerData)
    {
        int spectatingPlayers = 0;
        for (int i = 0; i < newPlayerData.Count; i++)
        {
            if (newPlayerData[i].OuterData.CurrentPlayerState == PlayerOuterData.PlayerState.SpectatingGame || newPlayerData[i].OuterData.CurrentPlayerState == PlayerOuterData.PlayerState.SpectatingUntilNextRound)
            {
                spectatingPlayers++;
            }
        }
        
        int playingPlayers = newPlayerData.Count - spectatingPlayers;

        if (scores.Count < playingPlayers)
        {
            for (int i = scores.Count; i < playingPlayers; i++)
            {
                GameObject score = Instantiate(scorePrefab, _scoreboardLayout);
                scores.Add(score);
            }
        }
        else if (scores.Count > playingPlayers)
        {
            for (int i = scores.Count - 1; i >= playingPlayers; i--)
            {
                Destroy(scores[i]);
                scores.RemoveAt(i);
            }
        }
        
        _spectatorText.text = spectatingPlayers > 0 ? "Spectators:" : "";
        _spectatorList.text = "";

        int a = 0;
        for (int i = 0; i < newPlayerData.Count; i++)
        {
            if (newPlayerData[i].OuterData.CurrentPlayerState == PlayerOuterData.PlayerState.SpectatingGame || newPlayerData[i].OuterData.CurrentPlayerState == PlayerOuterData.PlayerState.SpectatingUntilNextRound)
            {
                _spectatorList.text += $"{newPlayerData[i].ClientId}  ";
            }
            else
            {
                SetScorePrefabInfo(newPlayerData[i], scores[a]);
                a++;
            }
        }
    }

    private void SetScorePrefabInfo(PlayerData data, GameObject score)
    {
        Transform scoreTransform = score.transform;
        scoreTransform.GetChild(0).GetComponent<TMP_Text>().text = $"{data.ClientId}";
        scoreTransform.GetChild(1).GetComponent<TMP_Text>().text = $"{data.InGameData.score}";
    }
}