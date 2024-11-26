using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PScoreboard : PNetworkBehaviour
{
    [SerializeField] private GameObject scorePrefab;
    private List<GameObject> scores = new();
    [SerializeField] private GameObject scoreboardPrefab;
    
    private Transform _scoreboard;
    
    private Transform _scoreboardLayout;
    private TMP_Text _spectatorText;
    private TMP_Text _spectatorList;
    

    protected override void StartOnlineOwner()
    {
        _scoreboard = Instantiate(scoreboardPrefab, PCanvas.Canvas.transform).transform;
        _scoreboardLayout = _scoreboard.GetChild(0);
        _spectatorText = _scoreboard.GetChild(1).GetComponent<TMP_Text>();
        _spectatorList = _scoreboard.GetChild(2).GetComponent<TMP_Text>();
        
        GameManager.OnClientPlayerDataChanged += UpdateScoreboard;
        
        InputManager.instance.OnScoreboard += ShowScoreboard;
        InputManager.instance.OnStopScoreboard += HideScoreboard;
        
        HideScoreboard();
    }

    protected override void DisableOnlineOwner()
    {
        GameManager.OnClientPlayerDataChanged -= UpdateScoreboard;
        
        InputManager.instance.OnScoreboard -= ShowScoreboard;
        InputManager.instance.OnStopScoreboard -= HideScoreboard;
        
        HideScoreboard();
    }

    private void ShowScoreboard()
    {
        _scoreboard.gameObject.SetActive(true);
    }
    private void HideScoreboard()
    {
        _scoreboard.gameObject.SetActive(false);
    }
    private void UpdateScoreboard(List<PlayerData> newPlayerData)
    {
        int spectatingPlayers = 0;
        for (int i = 0; i < newPlayerData.Count; i++)
        {
            if (newPlayerData[i].CurrentPlayerState == PlayerData.PlayerState.Spectating)
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
            if (newPlayerData[i].CurrentPlayerState == PlayerData.PlayerState.Spectating)
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