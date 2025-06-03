using System.Collections;
using Base_Scripts;
using Game.Common;
using Game.Data.Extensions;
using Game.Game_Loop.Round.Tag.Hot_Potato;
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
                NetcodeLogger.Instance.LogRpc("Not enough players but playing anyway", NetcodeLogger.LogType.GameLoop);
                // NetcodeLogger.Instance.LogRpc("Not enough players to start Tag round. skipping", NetcodeLogger.LogType.GameLoop);
                // yield return new WaitForSeconds(3);
                // yield break;
            }

            bool onePlayerLeft = false;
            void OnePlayerLeft(ulong id) => onePlayerLeft = true;
            if (!GameSettings.Instance.DebugMode || NetworkManager.ConnectedClients.Count > 1)
                PlayerHealth.OnOnePlayerLeftServer += OnePlayerLeft;

            float timer = 0;
            while (timer < CollectTime && !onePlayerLeft)
            {
                timer += Time.deltaTime;
                if (onePlayerLeft)
                {
                    NetcodeLogger.Instance.LogRpc("One player left, stopping collect round",
                        NetcodeLogger.LogType.GameLoop);
                    break;
                }

                yield return null;
            }

            PlayerHealth.OnOnePlayerLeftServer -= OnePlayerLeft;

            //PlayerHealth.ResetPlayersHealth();
        }
    }
}