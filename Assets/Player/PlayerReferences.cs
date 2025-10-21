using System;
using Game.Common;
using Player.Health;
using UnityEngine;

namespace Player
{
    public class PlayerReferences : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerHealthComponent playerHealthComponent;
        public PlayerHealthComponent PlayerHealthComponent => playerHealthComponent;


        public event Action<ushort> OnDamaged;

        public void TakeDamage(IDamageable.DamageInfo info)
        {
            playerHealthComponent.TakeDamage(info);
            OnDamaged?.Invoke(info.DamageAmount);
        }
    }
}