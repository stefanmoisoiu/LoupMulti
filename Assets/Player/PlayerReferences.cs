using System;
using Game.Common;
using Player.Events;
using Player.Health;
using Player.Stats;
using UnityEngine;

namespace Player
{
    public class PlayerReferences : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerHealthComponent playerHealthComponent;
        public PlayerHealthComponent PlayerHealthComponent => playerHealthComponent;
        [SerializeField] private StatManager statManager;
        public StatManager StatManager => statManager;
        [SerializeField] private PlayerEventHub playerEventHub;
        public PlayerEventHub PlayerEventHub => playerEventHub;


        public event Action<ushort> OnDamaged;

        public void TakeDamage(IDamageable.DamageInfo info)
        {
            playerHealthComponent.TakeDamage(info);
            OnDamaged?.Invoke(info.DamageAmount);
        }
    }
}