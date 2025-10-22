using System;
using System.Collections;
using Base_Scripts;
using Game.Collect.Resource.Structure;
using Game.Common;
using Player.Camera.Effects;
using Player.Hitbox;
using Player.Model.Procedural_Anims;
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
        [SerializeField] private ArmAnim drillArm;
        [SerializeField] private PDrillHandTilt drillTilt;
        [SerializeField] private Vector3 drillArmOffsetRot;
        [SerializeField] private float drillArmDistance = 0.5f;
        [SerializeField] private float drillWaitTime = 0.5f;
        
        
        
        private Coroutine _useDrillCoroutine;
        
    
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
            if (!target.TryGetComponent(out IDamageable damageableTarget)) throw new Exception("Target is not IDamageable");
        
            success = true;
            ApplyCooldown(Item.AbilityData.BaseCooldown/PlayerStats.DrillSpeed.Apply(1));
            
            if (_useDrillCoroutine != null) StopCoroutine(_useDrillCoroutine);
            _useDrillCoroutine = StartCoroutine(UseDrillCoroutine(target, damageableTarget));
        }

        private IEnumerator UseDrillCoroutine(HitboxTarget target, IDamageable damageableTarget)
        {
            IDamageable.DamageInfo info = new() 
            {
                DamageAmount = damage,
                ExtractAmount = resourceExtractAmount,
                Origin = OwnerClientId
            };
            
            Vector3 drillRayDir = target.transform.position - transform.position;
            drillRayDir.y = 0;
            drillRayDir.Normalize();
            drillRayDir = Quaternion.Euler(drillArmOffsetRot) * drillRayDir;
            if (!target.Collider.TryGetPointOnCollider(drillRayDir, out Vector3 hitPoint)) throw new Exception("Drill Raycast failed");
            hitPoint -= drillRayDir.normalized * drillArmDistance;
            drillTilt.SetRotatingOwner(true);
            
            yield return drillArm.ShootOwner(hitPoint, drillRayDir);
            
            damageableTarget.TakeDamage(info);
            camShake?.AddShake(shakeSettings);
            OnAbilityUsedOwner?.Invoke();
            OnDrillUsedOwner?.Invoke(target);
            
            yield return new WaitForSeconds(drillWaitTime);
            
            yield return drillArm.RetractOwner();
            
            drillTilt.SetRotatingOwner(false);
        }

        public override bool CanUseAbility()
        {
            if (Cooldown > 0) return false;
            
            HitboxTarget target = hitbox.CalculateClosestHitbox();
            return target != null;
        }
    }
}
