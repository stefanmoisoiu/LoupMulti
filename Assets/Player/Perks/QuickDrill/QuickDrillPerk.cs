using Input;
using Player.Abilities;
using Player.Abilities.Drill;
using Player.Hitbox;
using UnityEngine;

namespace Player.Perks.QuickDrill
{
    public class QuickDrillPerk : PerkEffect
    {
        [SerializeField] private GameObject weakPointPrefab;
        [SerializeField] private Transform parentForWeakPoint;
        
        [SerializeField] private float cooldownReduction = 1;
        [SerializeField] private float minRotationSpread = 30;
        [SerializeField] private float maxRotationSpread = 45;

        [SerializeField] private LayerMask weakPointLayer;

        [SerializeField] private AbilityManager _abilityManager;
        
        
        internal override void StartApply()
        {
            DrillAbility.OnDrillUsedOwner += OnDrillUsedOwner;
            InputManager.OnDrillUse += TryHitWeakPoint;
        }
        internal override void StopApply()
        {
            DrillAbility.OnDrillUsedOwner -= OnDrillUsedOwner;
            InputManager.OnDrillUse -= TryHitWeakPoint;
        }

        private void OnDrillUsedOwner(HitboxTarget target)
        {
            GameObject weakPoint = Instantiate(weakPointPrefab, transform);
            if (!weakPoint.TryGetComponent(out WeakPoint weakPointComponent)) throw new System.Exception("WeakPoint prefab does not have a WeakPoint component");
            weakPointComponent.Setup(parentForWeakPoint, minRotationSpread, maxRotationSpread);
        }

        private void TryHitWeakPoint()
        {
            if (!Physics.Raycast(parentForWeakPoint.position, parentForWeakPoint.forward, out RaycastHit hit, 4, layerMask:weakPointLayer, QueryTriggerInteraction.Collide)) return;
            
            
            if (!hit.collider.TryGetComponent(out WeakPoint weakPointComponent)) throw new System.Exception("Hit weak point does not have a WeakPoint component");
            weakPointComponent.Hit();

            if (_abilityManager.DrillSlot == null) return;
            _abilityManager.DrillSlot.ReduceCooldown(cooldownReduction);
        }
    }
}