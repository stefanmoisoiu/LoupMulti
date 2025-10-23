using Base_Scripts;
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


        [SerializeField] private FloatStat maxSpeedStat;
        [SerializeField] private FloatStat accelerationStat;
        private StatModifier<float>.ModifierComponent _maxSpeedModifier = new (1,0);
        private StatModifier<float>.ModifierComponent _accelerationModifier = new (1,0);
        
        internal override void StartApply()
        {
            PlayerReferences.StatManager.GetFloatStat(maxSpeedStat).AddModifier(_maxSpeedModifier);
            PlayerReferences.StatManager.GetFloatStat(accelerationStat).AddModifier(_accelerationModifier);
            Debug.LogError("Drilling Addiction not implemented");
        }
        internal override void StopApply()
        {
            PlayerReferences.StatManager.GetFloatStat(maxSpeedStat).RemoveModifier(_maxSpeedModifier);
            PlayerReferences.StatManager.GetFloatStat(accelerationStat).RemoveModifier(_accelerationModifier);
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
            
            
            _maxSpeedModifier.factor = speedPerSec;
            _accelerationModifier.factor = speedPerSec;
            
            PlayerReferences.StatManager.GetFloatStat(maxSpeedStat)?.MarkDirty();
            PlayerReferences.StatManager.GetFloatStat(accelerationStat)?.MarkDirty();
        }
    }
}