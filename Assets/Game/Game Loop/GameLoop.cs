using System;
using System.Collections;
using Base_Scripts;
using Game.Common;
using Game.Data;
using Game.Data.Extensions;
using Game.Game_Loop.Round;
using Game.Game_Loop.Round.Collect;
using Game.Game_Loop.Round.End;
using Game.Game_Loop.Round.Upgrade;
using Unity.Netcode;
using UnityEngine;

namespace Game.Game_Loop
{
    public class GameLoop : NetworkBehaviour
    {
        [SerializeField] private GameTickManager gameTickManager;
        public GameTickManager GameTickManager => gameTickManager;
        [SerializeField] private GameLoopEvents gameLoopEvents;
        public GameLoopEvents GameLoopEvents => gameLoopEvents;

        [SerializeField] private CollectRound collectRound;
        public CollectRound CollectRound => collectRound;
        
        [SerializeField] private UpgradeRound upgradeRound;
        public UpgradeRound UpgradeRound => upgradeRound;
        [SerializeField] private CountdownRound countdownRound;
        public CountdownRound CountdownRound => countdownRound;
        [SerializeField] private EndRound endRound;

        [SerializeField] private GameReportManager gameReportManager;
        public GameReportManager GameReportManager => gameReportManager;

        public NetworkVariable<ushort> CurrentRound = new();
        
        public IEnumerator MainLoop(GameManager manager)
        {
            if (GameSettings.Instance.NeverStopGame)
                Debug.LogWarning("Never stopping game /!/");
            else
                PlayerHealthHelper.OnPlayerDeathServer += PlayerDiedServer;
            
            bool gameEnded = false;
            void PlayerDiedServer(ulong clientId)
            {
                int alivePlayerCount = PlayerHealthHelper.AlivePlayingPlayerCount();
                if (alivePlayerCount > 1) return;
                NetcodeLogger.Instance.LogRpc($"{alivePlayerCount} player(s) alive, ending game", NetcodeLogger.LogType.GameLoop);
                gameEnded = true;
            }
            
            gameTickManager.StartTickLoop();
            gameReportManager.ResetReportServer();
            gameReportManager.StartRecordingServer();
            
            Coroutine c = StartCoroutine(PlayLoop(manager));
            while (!gameEnded) yield return null;
            StopCoroutine(c);
            
            gameReportManager.StopRecordingServer();
            gameReportManager.SetEndGameReportServer();
            
            yield return endRound.Execute(manager, gameLoopEvents);
        
            gameTickManager.StopTickLoop();

            PlayerData[] alivePlayers = PlayerHealthHelper.AlivePlayingPlayers();
            if (alivePlayers.Length == 1)
            {
                ulong winnerClientId = alivePlayers[0].clientId;
                NetcodeLogger.Instance.LogRpc("Player " + winnerClientId + " is the winner!", NetcodeLogger.LogType.GameLoop);
                gameReportManager.RecordWinnerServer(winnerClientId);
            }
            else
            {
                NetcodeLogger.Instance.LogRpc("No winner could be determined.", NetcodeLogger.LogType.GameLoop);
            }
        
            gameLoopEvents.RoundStateChanged(GameRoundState.None, NetworkManager.ServerTime.TimeAsFloat);
        }

        private IEnumerator PlayLoop(GameManager manager)
        {
            CurrentRound.Value = 0;
            while (true)
            {
                CurrentRound.Value++;
                
                NetcodeLogger.Instance.LogRpc("Round " + CurrentRound.Value, NetcodeLogger.LogType.GameLoop);

                yield return countdownRound.Execute(manager, gameLoopEvents);
                yield return collectRound.Execute(manager, gameLoopEvents);
                NetcodeLogger.Instance.LogRpc("Collect round ended. Starting upgrade round.", NetcodeLogger.LogType.GameLoop);
                yield return upgradeRound.Execute(manager, gameLoopEvents);
            }
        }
    }
}