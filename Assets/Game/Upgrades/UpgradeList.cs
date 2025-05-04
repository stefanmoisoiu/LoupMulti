using UnityEngine;

namespace Game.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade List", menuName = "Game/Upgrade/Upgrade List")]
    public class UpgradeList : ScriptableObject
    {
        public ScriptableUpgrade[] upgrades;
    }
}