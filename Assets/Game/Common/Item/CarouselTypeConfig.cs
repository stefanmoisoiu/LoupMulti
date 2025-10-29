using Sirenix.OdinInspector;
using UnityEngine;
namespace Game.Common
{
    [CreateAssetMenu(fileName = "CarouselConfig", menuName = "Game/Carousel Config")]
    public class CarouselTypeConfig : ScriptableObject
    {
        public int numberOfChoices = 3;
        [Space]
        public bool allowNewItems = true;
        [ShowIf("allowNewItems")] [BoxGroup("New Items")]
        public bool allowNewPerks = true;
        [ShowIf("allowNewItems")] [BoxGroup("New Items")]
        public bool allowNewAbilities = true;
        [InfoBox("Upgrades of both abilities and perks")]
        public bool allowUpgrades = true;
        [InfoBox("One of the choices will be an ability, if the player does not own all his abilities")]
        public bool forceActiveIfNotMax = false;
    }
}