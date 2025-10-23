using System.Collections;
using Base_Scripts;
using Game.Common;
using Game.Data.Extensions;
using UnityEngine;

namespace Game.Game_Loop.Round.Collect
{
    public class CollectRound : GameRound
    {

        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.Collect, NetworkManager.ServerTime.TimeAsFloat);
            yield return new WaitForSeconds(GameSettings.Instance.UpgradeRoundLength);
        }
    }
}