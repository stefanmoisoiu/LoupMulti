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

        //[SerializeField] private TagRound tagRound;
        //public TagRound TagRound => tagRound;
        [SerializeField] private CollectRound collectRound;
        public CollectRound CollectRound => collectRound;
        
        [SerializeField] private UpgradeRound upgradeRound;
        public UpgradeRound UpgradeRound => upgradeRound;
        [SerializeField] private CountdownRound countdownRound;
        public CountdownRound CountdownRound => countdownRound;
        [SerializeField] private EndRound endRound;
        
    
        private Coroutine _gameLoopCoroutine;

        private event Action OnGameEnded;

        private void Start()
        {
            if (!IsServer) return;
            if (GameSettings.Instance.DebugMode)
                Debug.LogWarning("Debug mode is enabled, never stopping game /!/");
            else
                PlayerDataHealth.OnPlayerDeath += PlayerDiedServer;
        }

        private void OnDisable()
        {
            PlayerDataHealth.OnPlayerDeath -= PlayerDiedServer;
        }


        public IEnumerator MainLoop(GameManager manager)
        {
            _gameLoopCoroutine = StartCoroutine(PlayLoop(manager));
            
            gameTickManager.StartTickLoop();
            
            bool gameEnded = false;
            void GameEnded()
            {
                gameEnded = true;
                OnGameEnded -= GameEnded;
            }
            OnGameEnded += GameEnded;
            yield return new WaitUntil(() => gameEnded);
            
            if (_gameLoopCoroutine != null)
                StopCoroutine(_gameLoopCoroutine);
            
            yield return GameEnd(manager);
        }

        private IEnumerator PlayLoop(GameManager manager)
        {
            // Play Round -> Choose Upgrade -> Repeat
            
            int round = 0;

            while (true)
            {
                round++;
                NetcodeLogger.Instance.LogRpc("Round " + round, NetcodeLogger.LogType.GameLoop);

                yield return countdownRound.Execute(manager, gameLoopEvents);
                yield return collectRound.Execute(manager, gameLoopEvents);
                yield return upgradeRound.Execute(manager, gameLoopEvents);
            }
        }

        private IEnumerator GameEnd(GameManager manager)
        {
            yield return endRound.Execute(manager, gameLoopEvents);
        
            gameTickManager.StopTickLoop();
        
            NetcodeLogger.Instance.LogRpc("Game ended", NetcodeLogger.LogType.GameLoop);
            gameLoopEvents.RoundStateChanged(GameRoundState.None, NetworkManager.ServerTime.TimeAsFloat);
            gameLoopEvents.GameEnded();
        }

        private void PlayerDiedServer(ulong clientId)
        {
            Debug.Log("Checking for end game conditions");
            int alivePlayerCount = PlayerDataHealth.AlivePlayingPlayerCount();
            Debug.Log("There are " + alivePlayerCount + " players alive");
            if (alivePlayerCount <= 1)
            {
                NetcodeLogger.Instance.LogRpc("Only " + alivePlayerCount + " player(s) alive, ending game", NetcodeLogger.LogType.GameLoop);
                OnGameEnded?.Invoke();
            }
        }
    }
}