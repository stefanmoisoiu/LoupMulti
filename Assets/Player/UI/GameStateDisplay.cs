using System.Collections;
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Game.Game_Loop.Round.Upgrade;
using Player.Networking;
using TMPro;
using UnityEngine;

namespace Player.UI
{
    public class GameStateDisplay : PNetworkBehaviour
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
        
            GameLoopEvents.OnRoundStateChangedAll += UpdateGameStateUI;
        }

        private void UpdateGameStateUI(GameRoundState state, float serverTime)
        {
            int timer = 0;
            string text = string.Empty;
            switch (state)
            {
                case GameRoundState.Countdown:
                    text = "Starting";
                    timer = CountdownRound.CountdownTime;
                    break;
                case GameRoundState.ChoosingUpgrade:
                    text = "Upgrading";
                    timer = UpgradeRound.UpgradeTime;
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
}
