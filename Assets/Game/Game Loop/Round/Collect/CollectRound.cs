using System.Collections;
using Base_Scripts;
using Game.Data.Extensions;
using Game.Game_Loop.Round.Tag.Hot_Potato;
using Game.Manager;
using UnityEngine;

namespace Game.Game_Loop.Round.Collect
{
    public class CollectRound : GameRound
    {
        public const int CollectTime = 60;
        
        [SerializeField] private PlayerHealth playerHealth;
        public PlayerHealth PlayerHealth => playerHealth;
    
    
        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.Collect, NetworkManager.ServerTime.TimeAsFloat);
        
            if (PlayerHealth.AlivePlayerCount() < 2)
            {
                NetcodeLogger.Instance.LogRpc("Not enough players to start Tag round. skipping", NetcodeLogger.LogType.GameLoop);
                yield return new WaitForSeconds(3);
                yield break;
            }
            
            bool onePlayerLeft = false;
            void OnePlayerLeft() => onePlayerLeft = true;
            PlayerHealth.OnOnePlayerLeftServer += OnePlayerLeft;

            float timer = 0;
            while (timer < CollectTime && !onePlayerLeft) timer += Time.deltaTime;
            PlayerHealth.OnOnePlayerLeftServer -= OnePlayerLeft;
        
            //PlayerHealth.ResetPlayersHealth();
        }
    }
}