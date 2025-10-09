using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Game/Upgrade/Abilities/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        [SerializeField]  [InlineEditor] private ObjectInfo info;
        public ObjectInfo Info => info;
        
        [SerializeField] private float baseCooldown = 1f;
        public float BaseCooldown => baseCooldown;
    }
}
