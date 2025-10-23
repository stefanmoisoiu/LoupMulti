using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Base_Scripts
{
    [Serializable]
    public class StatModifier<T> where T : struct
    {
        private List<ModifierComponent> _modifiers = new();
        
        // Cache
        private bool _isDirty = true;
        private float _cachedFactor = 1f;
        private T _cachedAdded;

        // Appelée pour forcer un recalcul (très important)
        public void MarkDirty() => _isDirty = true;

        private void Recalculate()
        {
            _cachedFactor = 1f;
            float added = 0;

            foreach (ModifierComponent modifier in _modifiers)
            {
                _cachedFactor *= modifier.factor;
                added +=
                    modifier.added switch
                    {
                        float floatValue => floatValue,
                        int intValue => intValue,
                        _ => throw new InvalidOperationException($"Unsupported type {typeof(T)} for added value.")
                    };
            }

            if (typeof(T) == typeof(float)) _cachedAdded = (T)(object)added;
            else if (typeof(T) == typeof(int)) _cachedAdded = (T)(object)(int)added;
            else throw new InvalidOperationException($"Unsupported type {typeof(T)} for added value.");
            
            _isDirty = false;
        }

        public float GetFactor()
        {
            if (_isDirty) Recalculate();
            return _cachedFactor;
        }

        public T GetAdded()
        {
            if (_isDirty) Recalculate();
            return _cachedAdded;
        }

        public T Apply(T value)
        {
            if (_isDirty) Recalculate();
            
            if (typeof(T) == typeof(float)) return (T)(object)((float)(object)value * _cachedFactor + (float)(object)_cachedAdded);
            if (typeof(T) == typeof(int)) return (T)(object)(int)((int)(object)value * _cachedFactor + (int)(object)_cachedAdded);
            throw new InvalidOperationException($"Unsupported type {typeof(T)} for Apply method.");
        }
    
        public void AddModifier(ModifierComponent modifier)
        {
            _modifiers.Add(modifier);
            MarkDirty();
        }
        
        public void RemoveModifier(ModifierComponent modifier)
        {
            _modifiers.Remove(modifier);
            MarkDirty();
        }

        [Serializable]
        public class ModifierComponent
        {
            public float factor;
            public T added;

            public ModifierComponent(float factor, T added, float power = 1)
            {
                this.factor = factor;
                this.added = added;
            }
        }
    }
}