using System;
using System.Linq;
using Game.Data.Extensions;
using Unity.Netcode;
using UnityEngine;
namespace Game.Game_Loop
{
    public class GameReportManager : NetworkBehaviour
    {
        // ----- Données stockées côté serveur -----
        private EndGameReport _endGameReport;

        public static event Action<EndGameReport> OnEndGameReportReadyClient;
        [ClientRpc] private void OnEndGameReportReadyClientRpc(EndGameReport report)
        {
            OnEndGameReportReadyClient?.Invoke(report);
        }
        
        private bool _isRecording = false;
        public bool IsRecording => _isRecording;

        public void StartRecordingServer()
        {
            if (!IsServer) throw new InvalidOperationException("StartRecordingServer can only be called on the server.");
            if (_isRecording) return;
            PlayerHealthHelper.OnPlayerDeathServer += RecordPlayerDeathServer;
            _isRecording = true;
        }

        public void StopRecordingServer()
        {
            if (!IsServer) throw new InvalidOperationException("StopRecordingServer can only be called on the server.");
            if (!_isRecording) return;
            PlayerHealthHelper.OnPlayerDeathServer -= RecordPlayerDeathServer;
            _isRecording = false;
        }

        public void ResetReportServer()
        {
            if (!IsServer) throw new InvalidOperationException("ResetReportServer can only be called on the server.");
            _endGameReport = new(ulong.MaxValue, new());
        }

        private void RecordPlayerDeathServer(ulong clientId)
        {
            if (!IsServer) throw new InvalidOperationException("RecordPlayerDeathServer can only be called on the server.");

            _endGameReport.Timeline ??= new();
            
            if (_endGameReport.Timeline.Any(x => x.ClientId == clientId)) Debug.LogError($"[GameReportManager] Player {clientId} death already recorded in timeline!");
            else _endGameReport.Timeline.Add(new PlayerDeathEntry(clientId, GameManager.Instance.GameLoop.CurrentRound.Value));
        }

        public void RecordWinnerServer(ulong clientId)
        {
            if (!IsServer) throw new InvalidOperationException("RecordWinnerServer can only be called on the server.");
            _endGameReport.WinnerClientId = clientId;
        }
        
        public void SetEndGameReportServer() => OnEndGameReportReadyClientRpc(_endGameReport);
    }
}