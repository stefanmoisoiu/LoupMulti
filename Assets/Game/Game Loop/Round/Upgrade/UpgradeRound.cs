using System.Collections;
using Game.Manager;
using UnityEngine;

namespace Game.Game_Loop.Round.Upgrade
{
    public class UpgradeRound : GameRound
    {
        public const int UpgradeTime = 10;
    
        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.ChoosingUpgrade, NetworkManager.ServerTime.TimeAsFloat);
        
            gameManager.UpgradesManager.ChooseUpgradesForPlayersServer();
            yield return new WaitForSeconds(UpgradeTime);
            gameManager.UpgradesManager.ApplyUpgrades();
        }
    }
}