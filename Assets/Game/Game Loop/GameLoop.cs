using System.Collections;
using Base_Scripts;
using Game.Common;
using Game.Data.Extensions;
using Game.Game_Loop.Round;
using Game.Game_Loop.Round.Collect;
using Game.Game_Loop.Round.End;
using Game.Game_Loop.Round.Tag;
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

        public IEnumerator MainLoop(GameManager manager)
        {
            // Choose Upgrade -> Play Round -> Repeat
            
            gameTickManager.StartTickLoop();

            int round = 0;


            if (GameSettings.Instance.DebugMode)
                Debug.LogWarning("Debug mode is enabled, never stopping game /!/");

            while (PlayerHealth.AlivePlayerCount() > 1 || GameSettings.Instance.DebugMode)
            {
                round++;
                NetcodeLogger.Instance.LogRpc("Round " + round, NetcodeLogger.LogType.GameLoop);

                yield return upgradeRound.Execute(manager, gameLoopEvents);
                yield return countdownRound.Execute(manager, gameLoopEvents);
                yield return collectRound.Execute(manager, gameLoopEvents);
            }
            yield return endRound.Execute(manager, gameLoopEvents);
        
            gameTickManager.StopTickLoop();
        
            NetcodeLogger.Instance.LogRpc("Game ended", NetcodeLogger.LogType.GameLoop);
            gameLoopEvents.RoundStateChanged(GameRoundState.None, NetworkManager.ServerTime.TimeAsFloat);
            gameLoopEvents.GameEnded();
        }
    }
}