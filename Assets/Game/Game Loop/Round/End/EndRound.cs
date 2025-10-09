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
            yield return new WaitForSeconds(3f);
        }
    }
}