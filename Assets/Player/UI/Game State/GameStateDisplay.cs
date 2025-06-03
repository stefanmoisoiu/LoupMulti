using System.Collections;
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Game.Game_Loop.Round.Collect;
using Game.Game_Loop.Round.Upgrade;
using Player.Networking;
using TMPro;
using UnityEngine;

namespace Player.UI.Game_State
{
    public class GameStateDisplay : PNetworkBehaviour
    {
        private const string GameStateBarTag = "GameStateBar";
    
        private GameObject gameStateBar;
        private CanvasGroup canvasGroup;
        private TMP_Text gameStateText;
        private TMP_Text timeLeftText;
    
        private Coroutine textTimerCoroutine;

        protected override void StartOnlineOwner()
        {
            gameStateBar = PCanvas.CanvasObjects[GameStateBarTag];
            canvasGroup = gameStateBar.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            gameStateText = gameStateBar.transform.GetChild(0).GetComponent<TMP_Text>();
            timeLeftText = gameStateBar.transform.GetChild(1).GetComponent<TMP_Text>();
        
            GameLoopEvents.OnRoundStateChangedAll += UpdateGameStateUI;
        }

        private void UpdateGameStateUI(GameRoundState state, float serverTime)
        {
            canvasGroup.alpha = 1;
            int timer = 0;
            string text = string.Empty;
            switch (state)
            {
                case GameRoundState.Countdown:
                    text = "Starting";
                    timer = CountdownRound.CountdownTime;
                    break;
                case GameRoundState.Collect:
                    text = "Collecting";
                    timer = CollectRound.CollectTime;
                    break;
                case GameRoundState.Upgrade:
                    text = "Upgrading";
                    timer = UpgradeRound.UpgradeTime;
                    break;
            }
            gameStateText.text = text;
            timeLeftText.text = timer > 0 ? timer.ToString() : "";
            if (timer <= 0) return;
            float delay = NetworkManager.ServerTime.TimeAsFloat - serverTime;
            
            if (textTimerCoroutine != null) StopCoroutine(textTimerCoroutine);
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
