using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Base_Scripts
{
    public class StatModifier<T> where T : struct
    {
        public List<ModifierComponent> _modifiers = new();
        
        public float GetFactor()
        {
            float factor = 1;
            foreach (var modifier in _modifiers) factor *= Mathf.Pow(modifier.factor, modifier.power);
            return factor;
        }

        public T GetAdded()
        {
            float added = 0;
            foreach (var modifier in _modifiers)
            {
                if (modifier.added is float floatValue) added += floatValue;
                else if (modifier.added is int intValue) added += intValue;
                else throw new InvalidOperationException($"Unsupported type {typeof(T)} for added value.");
            }
            if (typeof(T) == typeof(float)) return (T)(object)added;
            if (typeof(T) == typeof(int)) return (T)(object)(int)added;
            throw new InvalidOperationException($"Unsupported type {typeof(T)} for added value.");
        }

        public T Apply(T value)
        {
            float factor = GetFactor();
            T added = GetAdded();
            if (typeof(T) == typeof(float)) return (T)(object)((float)(object)value * factor + (float)(object)added);
            if (typeof(T) == typeof(int)) return (T)(object)(int)((int)(object)value * factor + (int)(object)added);
            throw new InvalidOperationException($"Unsupported type {typeof(T)} for Apply method.");
        }
    
        public void AddModifier(ModifierComponent modifier)
        {
            _modifiers.Add(modifier);
        }
        public void RemoveModifier(ModifierComponent modifier)
        {
            _modifiers.Remove(modifier);
        }

        [Serializable]
        public class ModifierComponent
        {
            public float factor;
            public T added;
            public float power;

            public ModifierComponent(float factor, T added, float power = 1)
            {
                this.factor = factor;
                this.added = added;
                this.power = power;
            }
        }
    }
}