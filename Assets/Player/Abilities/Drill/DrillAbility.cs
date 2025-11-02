using System;
using System.Collections;
using Game.Common;
using Game.Common.Hitbox;
using Player.Target;
using Player.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Abilities.Drill
{
    public class DrillAbility : Ability
    {
        [Title("References")]
        [SerializeField] private TargetDetector targetDetector;
        [SerializeField] private DrillUseEffect drillUseEffect;
        
        private Coroutine _useDrillCoroutine;
        
        [BoxGroup("Drill Ability Settings")] [SerializeField]
        private ushort damage = 15;
        [BoxGroup("Drill Ability Settings")] [SerializeField]
        private ushort resourceExtractAmount = 1;

        public override void TryUseAbility(out bool success)
        {
            success = false;
            if (Cooldown > 0) return;
            
            Targetable target = targetDetector.CalculateClosestTarget();
            if (target == null) return;
            if (!target.TryGetComponent(out IDamageable damageableTarget)) throw new Exception("Target is not IDamageable");
        
            success = true;

            float maxCooldown = Item.AbilityData.BaseCooldown / PlayerReferences.StatManager.GetStat(StatType.DrillCooldownSpeed).GetValue(1);
            ApplyCooldown(maxCooldown,maxCooldown);
            
            if (_useDrillCoroutine != null) StopCoroutine(_useDrillCoroutine);
            _useDrillCoroutine = StartCoroutine(UseDrillCoroutine(target, damageableTarget));
        }

        private IEnumerator UseDrillCoroutine(Targetable target, IDamageable damageableTarget)
        {
            IDamageable.DamageInfo info = new() 
            {
                DamageAmount = damage,
                ExtractAmount = resourceExtractAmount,
                Origin = OwnerClientId
            };

            yield return drillUseEffect.DrillEffectStart(target);
            
            damageableTarget.TakeDamage(info);
            OnAbilityUsedOwner?.Invoke();
            PlayerReferences.PlayerEventHub.OnAbilityUsed?.Invoke();
            PlayerReferences.PlayerEventHub.OnDrillUsed?.Invoke(target);
            
            yield return drillUseEffect.DrillEffectEnd();
        }

        public override bool CanUseAbility()
        {
            if (Cooldown > 0) return false;
            
            Targetable target = targetDetector.CalculateClosestTarget();
            return target != null;
        }
    }
}
