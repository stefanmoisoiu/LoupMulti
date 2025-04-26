using System.Collections;
using Base_Scripts;
using Game.Data.Extensions;
using Game.Game_Loop.Round.Tag.Hot_Potato;
using Game.Manager;
using UnityEngine;

namespace Game.Game_Loop.Round.Tag
{
    public class TagRound : GameRound
    {
        [SerializeField] private HotPotatoManager hotPotatoManager;
        public HotPotatoManager HotPotatoManager => hotPotatoManager;
        [SerializeField] private PlayerHealth playerHealth;
        public PlayerHealth PlayerHealth => playerHealth;
    
    
        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.Tag_OBSOLETE, NetworkManager.ServerTime.TimeAsFloat);
        
            if (PlayerHealth.AlivePlayerCount() < 2)
            {
                NetcodeLogger.Instance.LogRpc("Not enough players to start Tag round. skipping", NetcodeLogger.LogType.GameLoop);
                yield return new WaitForSeconds(3);
                yield break;
            }
        
            hotPotatoManager.Enable();

            bool roundFinished = false;
            void FinishRound() => roundFinished = true;
            PlayerHealth.OnOnePlayerLeftServer += FinishRound;
            while (!roundFinished) yield return null;
            PlayerHealth.OnOnePlayerLeftServer -= FinishRound;
        
            hotPotatoManager.Disable();
        
            PlayerHealth.ResetPlayersHealth();
        }
    }
}