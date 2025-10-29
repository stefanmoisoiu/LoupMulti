using Game.Common;
using Game.Data;
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Player.Networking;
using TMPro;
using UnityEngine;

namespace Player.General_UI.End_Game
{
    public class EndGameDisplay : PNetworkBehaviour
    {
        private CanvasGroup _canvasGroup;
        private TMP_Text _winnerText;
        private TMP_Text _timelineText;
        
        protected override void StartAnyOwner()
        {
            _canvasGroup = PCanvas.CanvasObjects["EndGame"].GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _winnerText = PCanvas.CanvasObjects["EndGameWinner"].GetComponent<TMP_Text>();
            _timelineText = PCanvas.CanvasObjects["EndGameTimeline"].GetComponent<TMP_Text>();

            GameReportManager.OnEndGameReportReadyClient += OnEndGameReportReady;
            GameLoopEvents.OnRoundStateChangedAll += OnRoundStateChanged;
        }

        protected override void DisableAnyOwner()
        {
            GameReportManager.OnEndGameReportReadyClient -= OnEndGameReportReady;
            GameLoopEvents.OnRoundStateChangedAll -= OnRoundStateChanged;
        }
        
        private void OnRoundStateChanged(GameRoundState newState, float serverTime)
        {
            SetVisible(newState == GameRoundState.End);
        }

        private void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
        }

        private void OnEndGameReportReady(EndGameReport report)
        {
            ulong winnerClient = report.WinnerClientId;
            string winnerString = "";

            if (winnerClient != ulong.MaxValue)
            {
                PlayerData winnerData = DataManager.Instance[winnerClient];
                winnerString = $"Client {winnerData.clientId}";
            }
            else winnerString = "No winner";
            
            string timelineString = "";

            foreach (PlayerDeathEntry deathEntry in report.Timeline)
            {
                timelineString += $"[Client {deathEntry.ClientId} Round {deathEntry.Round}]  ";
            }
            
            _winnerText.text = winnerString;
            _timelineText.text = timelineString;
        }
    }
}