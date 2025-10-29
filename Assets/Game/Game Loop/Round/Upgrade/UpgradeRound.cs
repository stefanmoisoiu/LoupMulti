using System.Collections;
using Game.Common;
using Game.Upgrade.Carousel;
using UnityEngine;

namespace Game.Game_Loop.Round.Upgrade
{
    public class UpgradeRound : GameRound
    {
        [SerializeField] private MainCarousel mainCarousel;
        
        public override IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents)
        {
            gameLoopEvents.RoundStateChanged(GameRoundState.Upgrade, NetworkManager.ServerTime.TimeAsFloat);
            mainCarousel.TriggerMainCarousel();
            yield return new WaitForSeconds(GameSettings.Instance.UpgradeRoundLength);
            gameManager.CarouselManager.ForceApplyAllRemainingCarousels();
            gameManager.CarouselManager.ResetServerData();
        }
    }
}