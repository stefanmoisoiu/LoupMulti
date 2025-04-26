using System.Collections;
using Base_Scripts;
using Game.Data.Extensions;
using Game.Game_Loop.Round;
using Game.Game_Loop.Round.Collect;
using Game.Game_Loop.Round.Tag;
using Game.Game_Loop.Round.Upgrade;
using Game.Manager;
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
    
        public const int RoundCount = 2;
    
        private Coroutine _gameLoopCoroutine;

        public void StartGameLoop(GameManager manager)
        {
            if (_gameLoopCoroutine != null) StopCoroutine(_gameLoopCoroutine);
            _gameLoopCoroutine = StartCoroutine(MainLoop(manager));
        }

        private IEnumerator MainLoop(GameManager manager)
        {
            // Choose Upgrade -> Play Round -> Repeat
            int round = 1;
        
            gameTickManager.StartTickLoop();

            while (round <= RoundCount)
            {
                NetcodeLogger.Instance.LogRpc("Round " + round, NetcodeLogger.LogType.GameLoop);

                manager.MapManager.SetPlayerSpawnPositions();
                yield return upgradeRound.Execute(manager, gameLoopEvents);
                yield return countdownRound.Execute(manager, gameLoopEvents);
                yield return collectRound.Execute(manager, gameLoopEvents);
                round++;
            }
        
            gameTickManager.StopTickLoop();
        
            NetcodeLogger.Instance.LogRpc("Game ended", NetcodeLogger.LogType.GameLoop);
            gameLoopEvents.RoundStateChanged(GameRoundState.None, NetworkManager.ServerTime.TimeAsFloat);
            gameLoopEvents.GameEnded();
        }
    }
}