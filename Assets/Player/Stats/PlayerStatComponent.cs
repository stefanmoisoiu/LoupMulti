using System;
using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Stats
{
    [Serializable]
    public class 
        PlayerStatComponent
    {
        [TitleGroup("Movement")] public StatModifier<float>.ModifierComponent acceleration = new(1,0);
        [TitleGroup("Movement")] public StatModifier<float>.ModifierComponent maxSpeed = new(1,0);
        [TitleGroup("Movement")] public StatModifier<float>.ModifierComponent jumpHeight = new(1,0);
        [TitleGroup("Stamina")] public StatModifier<float>.ModifierComponent staminaRecoveryMult = new(1,0);
        [TitleGroup("Health")] public StatModifier<float>.ModifierComponent healthPerSecond = new(1,0);
        [TitleGroup("Drill")] public StatModifier<float>.ModifierComponent drillSpeed = new(1,0);
        [TitleGroup("Drill")] public StatModifier<int>.ModifierComponent drillExtractAmount = new(1,0);
        
        private bool initialized = false;
        private float baseAcceleration;
        private float baseMaxSpeed;
        private float baseJumpHeight;
        private float baseStaminaRecoveryMult;
        private float baseHealthPerSecond;
        private float baseDrillSpeed;
        private float baseDrillExtractAmount;
        
        public void Add()
        {
            PlayerStats.AddComponent(PlayerStats.StatType.Acceleration, acceleration);
            PlayerStats.AddComponent(PlayerStats.StatType.MaxSpeed, maxSpeed);
            PlayerStats.AddComponent(PlayerStats.StatType.JumpHeight, jumpHeight);
            PlayerStats.AddComponent(PlayerStats.StatType.StaminaRecovery, staminaRecoveryMult);
            PlayerStats.AddComponent(PlayerStats.StatType.HealthPerSecond, healthPerSecond);
            PlayerStats.AddComponent(PlayerStats.StatType.DrillSpeed, drillSpeed);
            PlayerStats.AddComponent(PlayerStats.StatType.DrillExtractAmount, drillExtractAmount);
        }
        
        public void Remove()
        {
            PlayerStats.RemoveComponent(PlayerStats.StatType.Acceleration, acceleration);
            PlayerStats.RemoveComponent(PlayerStats.StatType.MaxSpeed, maxSpeed);
            PlayerStats.RemoveComponent(PlayerStats.StatType.JumpHeight, jumpHeight);
            PlayerStats.RemoveComponent(PlayerStats.StatType.StaminaRecovery, staminaRecoveryMult);
            PlayerStats.RemoveComponent(PlayerStats.StatType.HealthPerSecond, healthPerSecond);
            PlayerStats.RemoveComponent(PlayerStats.StatType.DrillSpeed, drillSpeed);
            PlayerStats.RemoveComponent(PlayerStats.StatType.DrillExtractAmount, drillExtractAmount);
        }

        public void Power(int pow)
        {
            acceleration.power = pow;
            maxSpeed.power = pow;
            jumpHeight.power = pow;
            staminaRecoveryMult.power = pow;
            healthPerSecond.power = pow;
            drillSpeed.power = pow;
            drillExtractAmount.power = pow;
        }
    }
}