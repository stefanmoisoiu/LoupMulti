using System;
using System.Collections.Generic;
using Base_Scripts;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class CamShake : MonoBehaviour
    {
        private CamEffects.Effect _effect = new();
        [SerializeField] private Shake shake;

        public void AddShake(Shake.ShakeSettings settings)
        {
            shake.AddShake(settings);
        }
        public void RemoveShake(Shake.ShakeSettings settings)
        {
            shake.RemoveShake(settings);
        }
        
        private void OnEnable()
        {
            CamEffects.Effects.Add(_effect);
        }
        
        private void OnDisable()
        {
            CamEffects.Effects.Remove(_effect);
        }

        private void Update()
        {
            shake.Update(Time.deltaTime);
            _effect.AddedPosition = shake.GetShake2D();
        }
    }
}