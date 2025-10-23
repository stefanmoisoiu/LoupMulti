using System;
using System.Collections;
using Base_Scripts;
using Game.Common;
using Player.Camera.Effects;
using Player.Target;
using Player.Model.Procedural_Anims;
using Player.Networking;
using UnityEngine;

namespace Player.Abilities.Drill
{
    public class DrillUseEffect : PNetworkBehaviour
    {

        [SerializeField] private Ability drillAbility;
        [SerializeField] private CamShake camShake;
        [SerializeField] private Shake.ShakeSettings shakeSettings;
        [SerializeField] private ArmAnim drillArm;
        [SerializeField] private PDrillHandTilt drillTilt;
        [SerializeField] private TargetHighlighter highlighter;
        
        [SerializeField] private Vector3 drillArmOffsetRot;
        [SerializeField] private float drillArmDistance = 0.5f;
        [SerializeField] private float drillWaitTime = 0.5f;
        
        protected override void StartOnlineOwner()
        {
            drillAbility.OnAbilityAvailableOwner += highlighter.EnableHighlight;
            drillAbility.OnAbilityUsedOwner += highlighter.DisableHighlight;
        }

        protected override void DisableAnyOwner()
        {
            drillAbility.OnAbilityAvailableOwner -= highlighter.EnableHighlight;
            drillAbility.OnAbilityUsedOwner -= highlighter.DisableHighlight;
            highlighter?.DisableHighlight();
        }
        
        public IEnumerator DrillEffectStart(Targetable target)
        {
            Vector3 drillRayDir = target.transform.position - transform.position;
            drillRayDir.y = 0;
            drillRayDir.Normalize();
            drillRayDir = Quaternion.Euler(drillArmOffsetRot) * drillRayDir;
            if (!target.Collider.TryGetPointOnCollider(drillRayDir, out Vector3 hitPoint)) throw new Exception("Drill Raycast failed");
            hitPoint -= drillRayDir.normalized * drillArmDistance;
            drillTilt.SetRotatingOwner(true);
            
            yield return drillArm.ShootOwner(hitPoint, drillRayDir);
            
            camShake?.AddShake(shakeSettings);
        }
        
        public IEnumerator DrillEffectEnd()
        {
            yield return new WaitForSeconds(drillWaitTime);
            yield return drillArm.RetractOwner();
            drillTilt.SetRotatingOwner(false);
        }
    }
}