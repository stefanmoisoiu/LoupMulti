using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base_Scripts
{
    [Serializable]
    public class Shake
    {
        private List<ShakeInstance> _activeShakes = new();
        
        [SerializeField] private float moveFrequency = 6f;
        [SerializeField] private float amplitudeMult = 0.2f;
        
        [SerializeField] private float lerpSpeed = 20;
        
        private float _shakeAmplitude;
        
        public void AddShake(ShakeSettings settings)
        {
            _activeShakes.Add(new ShakeInstance(settings));
        }

        public void RemoveShake(ShakeSettings settings)
        {
            for (int i = _activeShakes.Count - 1; i >= 0; i--)
            {
                if (_activeShakes[i].Settings.Equals(settings))
                {
                    _activeShakes.RemoveAt(i);
                    return;
                }
            }
        }
        
        public void Update(float deltaTime)
        {
            UpdateShakeInstances(deltaTime);
            _shakeAmplitude = Mathf.Lerp(_shakeAmplitude, GetShakeAmount(), lerpSpeed * deltaTime);
        }

        public float GetShake1D() => (Mathf.PerlinNoise(Time.time * moveFrequency, 0) - 0.5f) * 2 * _shakeAmplitude;
        public Vector2 GetShake2D() => new Vector2(
            Mathf.PerlinNoise(Time.time * moveFrequency, 0) - 0.5f,
            Mathf.PerlinNoise(0, Time.time * moveFrequency) - 0.5f) * (2 * _shakeAmplitude);
        public Vector3 GetShake3D() => new Vector3(
            Mathf.PerlinNoise(Time.time * moveFrequency, 0) - 0.5f,
            Mathf.PerlinNoise(0, Time.time * moveFrequency) - 0.5f,
            Mathf.PerlinNoise(Time.time * moveFrequency + 9999, 0) - 0.5f) * 2 * _shakeAmplitude;
        
        private float GetShakeAmount()
        {
            float totalShake = 0;
            for (int i = _activeShakes.Count - 1; i >= 0; i--) totalShake += _activeShakes[i].GetCurrentShakeAmount();
            return totalShake * amplitudeMult;
        }
        
        private void UpdateShakeInstances(float deltaTime)
        {
            for (int i = _activeShakes.Count - 1; i >= 0; i--)
            {
                if (!_activeShakes[i].Update(deltaTime))
                {
                    _activeShakes.RemoveAt(i);
                }
            }
        }
        
        [Serializable]
        public struct ShakeSettings
        {
            
            public float Duration;
            public float Amplitude;
            public AnimationCurve Curve;
            
            
            public ShakeSettings(float duration = 1, float amplitude = 1, AnimationCurve curve = null)
            {
                curve ??= AnimationCurve.EaseInOut(0, 1, 1, 0);
                Duration = duration;
                Amplitude = amplitude;
                Curve = curve;
            }
        }
        
        private class ShakeInstance
        {
            public ShakeSettings Settings;
            public float ElapsedTime;

            public ShakeInstance(ShakeSettings settings)
            {
                Settings = settings;
                ElapsedTime = 0;
            }

            public bool Update(float deltaTime)
            {
                ElapsedTime += deltaTime;
                return ElapsedTime < Settings.Duration;
            }

            public float GetCurrentShakeAmount()
            {
                return Settings.Curve.Evaluate(ElapsedTime / Settings.Duration) * Settings.Amplitude;
            }
        }
    }
}