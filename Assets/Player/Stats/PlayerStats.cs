using System;
using Base_Scripts;
using UnityEngine;

namespace Player.Stats
{
    public static class PlayerStats
    {
        public static StatModifier<float> Acceleration { get; private set; } = new();
        public static StatModifier<float> MaxSpeed  { get; private set; } = new();
        public static StatModifier<float> JumpHeight  { get; private set; } = new();
        public static StatModifier<float> StaminaRecovery  { get; private set; } = new();
        public static StatModifier<int> MaxStamina  { get; private set; } = new();
        public static StatModifier<float> HealthPerSecond  { get; private set; } = new();
        public static StatModifier<float> DrillSpeed  { get; private set; } = new();
        public static StatModifier<int> DrillExtractAmount { get; private set; } = new();

        
        
        public enum StatType
        {
            Acceleration,
            MaxSpeed,
            JumpHeight,
            StaminaRecovery,
            MaxStamina,
            HealthPerSecond,
            DrillSpeed,
            DrillExtractAmount,
        }

        private static StatModifier<T> GetStatModifier<T>(StatType statType) where T : struct
        {
            return statType switch
            {
                StatType.Acceleration => Acceleration as StatModifier<T>,
                StatType.MaxSpeed => MaxSpeed as StatModifier<T>,
                StatType.MaxStamina => MaxStamina as StatModifier<T>,
                StatType.JumpHeight => JumpHeight as StatModifier<T>,
                StatType.StaminaRecovery => StaminaRecovery as StatModifier<T>,
                StatType.HealthPerSecond => HealthPerSecond as StatModifier<T>,
                StatType.DrillSpeed => DrillSpeed as StatModifier<T>,
                StatType.DrillExtractAmount => DrillExtractAmount as StatModifier<T>,
                _ => throw new ArgumentOutOfRangeException(nameof(statType), statType, null)
            };
        }
        
        public static void AddComponent<T>(StatType type, StatModifier<T>.ModifierComponent modifier) where T : struct
        {
            StatModifier<T> statModifier = GetStatModifier<T>(type);
            statModifier.AddModifier(modifier);
        }
        
        public static void RemoveComponent<T>(StatType type, StatModifier<T>.ModifierComponent modifier) where T : struct
        {
            StatModifier<T> statModifier = GetStatModifier<T>(type);
            statModifier.RemoveModifier(modifier);
        }


    }
}