using System;
using Game.Common;
using Game.Common.Hitbox;
using Unity.Netcode;
using UnityEngine;

namespace Player.Target
{
    public class EnvironmentDamager : MonoBehaviour
    {
        [SerializeField] private TargetDetector targetDetector;
        [SerializeField] private ushort damage = 20;
        [SerializeField] private float damageCooldown = 0.5f;
        private bool _canDamage = true;

        private void Update()
        {
            if(_canDamage)
                CheckDamage();
        }

        private void CheckDamage()
        {
            Targetable target = targetDetector.CalculateClosestTarget();
            if (target == null) return;
            if (!target.transform.root.TryGetComponent(out NetworkObject obj)) throw new Exception("No NetworkObject found on target root");
            if (!obj.IsLocalPlayer) return;
                
            if (target.GetComponentInParent<IDamageable>() is { } damageableTarget)
            {
                IDamageable.DamageInfo info = new()
                {
                    DamageAmount = damage,
                    Origin = obj.OwnerClientId
                };
                    
                damageableTarget.TakeDamage(info);
            }
            else throw new Exception("Target is not IDamageable");

            _canDamage = false;
            Invoke(nameof(ResetCanDamage), damageCooldown);
        }
        private void ResetCanDamage() => _canDamage = true;
    }
}