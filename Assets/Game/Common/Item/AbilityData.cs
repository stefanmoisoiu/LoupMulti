using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common
{
    [Serializable]
    public class AbilityData
    {
        [SerializeField] private float baseCooldown = 1f;
        public float BaseCooldown => baseCooldown;
    }
}
