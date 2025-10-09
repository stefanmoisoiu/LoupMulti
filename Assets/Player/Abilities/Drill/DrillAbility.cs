using System;
using Base_Scripts;
using Game.Collect.Resource.Structure;
using Game.Common;
using Player.Camera.Effects;
using Player.Hitbox;
using Player.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Abilities.Drill
{
    public class DrillAbility : Ability
    {
        [SerializeField] private Hitbox.Hitbox hitbox;
        [SerializeField] private HitboxHighlighter highlighter;
        [SerializeField] private CamShake camShake;
        [SerializeField] private Shake.ShakeSettings shakeSettings;
        
        
    
        [BoxGroup("Drill Ability Settings")] [SerializeField]
        private ushort damage = 15;
        [BoxGroup("Drill Ability Settings")] [SerializeField]
        private ushort resourceExtractAmount = 1;
    
        public static event Action<HitboxTarget> OnDrillUsedOwner;

        protected override void StartOnlineOwner()
        {
            OnAbilityAvailableOwner += highlighter.EnableHighlight;
            OnAbilityUsedOwner += highlighter.DisableHighlight;
        }

        protected override void DisableAnyOwner()
        {
            OnAbilityAvailableOwner -= highlighter.EnableHighlight;
            OnAbilityUsedOwner -= highlighter.DisableHighlight;
            highlighter?.DisableHighlight();
        }

        public override void TryUseAbility(out bool success)
        {
            success = false;
            if (Cooldown > 0) return;
            HitboxTarget target = hitbox.CalculateClosestHitbox();
            if (target == null) return;

            if (target.GetComponentInParent<IDamageable>() is { } damageableTarget)
            {
                IDamageable.DamageInfo info = new()
                {
                    DamageAmount = damage,
                    ExtractAmount = resourceExtractAmount,
                    Origin = OwnerClientId
                };
                
                damageableTarget.TakeDamage(info);
            }
            else throw new Exception("Target is not IDamageable");
        
            success = true;
            ApplyCooldown(AbilityData.BaseCooldown/PlayerStats.DrillSpeed.Apply(1));
            camShake?.AddShake(shakeSettings);
            OnAbilityUsedOwner?.Invoke();
        
            OnDrillUsedOwner?.Invoke(target);
        }

        public override bool CanUseAbility()
        {
            if (Cooldown > 0) return false;
            
            HitboxTarget target = hitbox.CalculateClosestHitbox();
            return target != null;
        }
    }
}
