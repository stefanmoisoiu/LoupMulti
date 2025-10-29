using Game.Common;
using Unity.Netcode;
using UnityEngine;

namespace Game.Upgrade.Carousel
{
    /// <summary>
    /// Gère le déclenchement du carousel principal au début de la phase d'amélioration.
    /// S'exécute uniquement côté serveur.
    /// </summary>
    public class MainCarousel : NetworkBehaviour
    {
        [Header("Dépendances")]
        [SerializeField] private CarouselManager carouselManager;
        [SerializeField] private CarouselTypeConfig mainCarouselConfig; // Fais glisser ton asset "MainCarouselConfig" ici
        
        public void TriggerMainCarousel() => carouselManager.TriggerCommonCarouselServer(mainCarouselConfig);
    }
}