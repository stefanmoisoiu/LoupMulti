using System.Collections;
using Base_Scripts;
using Game.Common;
using Game.Data.Extensions;
using UnityEngine;

namespace Game.Game_Loop.Round.End
{
    public class EndRound : GameRound
    {
        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.End, NetworkManager.ServerTime.TimeAsFloat);
            PlayerData[] alivePlayers = PlayerHealth.AlivePlayingPlayers();
            if (alivePlayers == null || alivePlayers.Length == 0)
            {
                Debug.LogError("Finished game with no alive players!");
            }
            else
            {
                PlayerData winner = alivePlayers[0]; 
                NetcodeLogger.Instance.LogRpc($"Game ended! Winner: {winner.ClientId}", NetcodeLogger.LogType.GameLoop);
            }
            yield return new WaitForSeconds(3f);
            
            NetcodeLogger.Instance.LogRpc("Returning to lobby...", NetcodeLogger.LogType.GameLoop);
            
        }
    }
}