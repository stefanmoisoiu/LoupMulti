using System.Collections;
using UnityEngine;

namespace Game.Game_Loop.Round
{
    public class CountdownRound : GameRound
    {
        public const int CountdownTime = 1;
    
        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.Countdown, NetworkManager.ServerTime.TimeAsFloat);
            yield return new WaitForSeconds(CountdownTime);
        }
    }
}