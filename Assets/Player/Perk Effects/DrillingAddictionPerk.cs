using System;
using Game.Game_Loop;
using Player.Stats;
using UnityEngine;

namespace Player.Perk_Effects
{
    public class DrillingAddictionPerk : PerkEffect
    {
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private Vector2 healPerSecRange;
        [SerializeField] private Vector2 speedFactorRange;
        [SerializeField] private float curveEnd;
        private float timeSinceLastMine;
        
        private PlayerReferences playerReferences;

        private PlayerStatComponent statComponent = new();
        
        
        internal override void StartApply(PlayerReferences playerReferences, int perkCount = 1)
        {
            statComponent.Add();
            this.playerReferences = playerReferences;
            
        }

        protected override void UpdateOnlineOwner()
        {
            if (!Applied) return;
            UpdateTimer();
            UpdateApply();
        }

        private void UpdateTimer()
        {
            if (playerReferences.Drill.drillingTarget.Value.TryGet(out _)) timeSinceLastMine = 0;
            else timeSinceLastMine += Time.deltaTime;
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

        internal override void StopApply(PlayerReferences playerReferences)
        {
            statComponent.Remove();
            this.playerReferences = null;
        }
    }
}