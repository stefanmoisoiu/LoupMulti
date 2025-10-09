using Player.Stats;
using UnityEngine;

namespace Player.Perks.Drilling_Addiction
{
    public class DrillingAddictionPerk : PerkEffect
    {
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private Vector2 healPerSecRange;
        [SerializeField] private Vector2 speedFactorRange;
        [SerializeField] private float curveEnd;
        private float timeSinceLastMine;
        

        private PlayerStatComponent statComponent = new();
        
        
        internal override void StartApply()
        {
            statComponent.Add();
            Debug.LogError("Drilling Addiction not implemented");
        }

        protected override void UpdateOnlineOwner()
        {
            if (!Applied) return;
            
            return;
            UpdateTimer();
            UpdateApply();
        }

        private void UpdateTimer()
        {
            // if (playerReferences.Drill.drillingTarget.Value.TryGet(out _)) timeSinceLastMine = 0;
            // else timeSinceLastMine += Time.deltaTime;
        }

        private void UpdateApply()
        {
            float adv = Mathf.Clamp01(timeSinceLastMine / curveEnd);
            float value = curve.Evaluate(adv);
            
            float healPerSec = Mathf.Lerp(healPerSecRange.x, healPerSecRange.y, value);
            float speedPerSec = Mathf.Lerp(speedFactorRange.x, speedFactorRange.y, value);
            
            statComponent.healthPerSecond.added = healPerSec;
            statComponent.maxSpeed.factor = speedPerSec;
            statComponent.acceleration.factor = speedPerSec;
        }

        internal override void StopApply()
        {
            statComponent.Remove();
        }
    }
}