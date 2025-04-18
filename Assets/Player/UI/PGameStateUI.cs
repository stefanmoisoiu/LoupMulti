using System.Collections;
using TMPro;
using UnityEngine;

public class PGameStateUI : PNetworkBehaviour
{
    private const string StaminaSliderTag = "GameStateBar";
    
    private GameObject gameStateBar;
    private CanvasGroup canvasGroup;
    private TMP_Text gameStateText;
    private TMP_Text timeLeftText;
    
    private Coroutine textTimerCoroutine;

    protected override void StartOnlineOwner()
    {
        gameStateBar = GameObject.FindGameObjectWithTag(StaminaSliderTag);
        canvasGroup = gameStateBar.GetComponent<CanvasGroup>();
        gameStateText = gameStateBar.transform.GetChild(0).GetComponent<TMP_Text>();
        timeLeftText = gameStateBar.transform.GetChild(1).GetComponent<TMP_Text>();
        
        if(GameManager.Instance != null)
            GameManager.Instance.gameLoop.OnRoundStateChanged += UpdateGameStateUI;
        else
            GameManager.OnCreated += gm => gm.gameLoop.OnRoundStateChanged += UpdateGameStateUI;
    }

    private void UpdateGameStateUI(GameLoop.RoundState state, float serverTime)
    {
        int timer = 0;
        string text = string.Empty;
        switch (state)
        {
            case GameLoop.RoundState.Countdown:
                text = "Starting";
                timer = GameLoop.Countdown;
                break;
            case GameLoop.RoundState.InRound:
                text = "Playing Round";
                timer = GameLoop.GameLength;
                break;
            case GameLoop.RoundState.ChoosingUpgrade:
                text = "Upgrading";
                timer = GameLoop.TimeToUpgrade;
                break;
            case GameLoop.RoundState.None:
                text = "";
                timer = 0;
                break;
        }
        gameStateText.text = text;
        
        timeLeftText.text = timer > 0 ? timer.ToString() : "";
        if (timer <= 0) return;
        if (textTimerCoroutine != null) StopCoroutine(textTimerCoroutine);

        float delay = NetworkManager.ServerTime.TimeAsFloat - serverTime;
        textTimerCoroutine = StartCoroutine(UpdateTextTimer(timer - delay));
    }
    
    private IEnumerator UpdateTextTimer(float time)
    {
        float timer = time;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            timeLeftText.text = timer.ToString("F1");
            yield return null;
        }
        timeLeftText.text = "0";
    }
}
