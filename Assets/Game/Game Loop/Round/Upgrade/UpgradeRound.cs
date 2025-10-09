using System.Collections;
using Game.Common;
using UnityEngine;

namespace Game.Game_Loop.Round.Upgrade
{
    public class UpgradeRound : GameRound
    {
        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.Upgrade, NetworkManager.ServerTime.TimeAsFloat);
        
            gameManager.ItemSelectionManager.ChooseItemsForPlayersServer();
            yield return new WaitForSeconds(GameSettings.Instance.CollectRoundLength);
            gameManager.ItemSelectionManager.GiveChosenItemToPlayers();
        }
    }
}