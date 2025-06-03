using System.Collections;
using UnityEngine;

namespace Game.Game_Loop.Round.Upgrade
{
    public class UpgradeRound : GameRound
    {
        public const int UpgradeTime = 10;
    
        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.Upgrade, NetworkManager.ServerTime.TimeAsFloat);
        
            gameManager.ShopManager.SetOpened(true);
            gameManager.PerkSelectionManager.ChoosePerksForPlayersServer();
            yield return new WaitForSeconds(UpgradeTime);
            gameManager.PerkSelectionManager.GiveChosenPerksToPlayers();
            gameManager.ShopManager.SetOpened(false);
        }
    }
}