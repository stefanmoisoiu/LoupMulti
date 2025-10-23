using System;
using Player.Target;
using UnityEngine;

namespace Player.Events
{
    public class PlayerEventHub : MonoBehaviour
    {
        public Action OnAbilityUsed;
        public Action<Targetable> OnDrillUsed;
    }
}