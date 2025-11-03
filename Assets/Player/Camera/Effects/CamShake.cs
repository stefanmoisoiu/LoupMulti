using System;
using System.Collections.Generic;
using Base_Scripts;
using Player.Networking;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class CamShake : PNetworkBehaviour
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

        protected override void StartAnyOwner()
        {
            CamEffects.Effects.Add(_effect);
        }
        protected override void DisableAnyOwner()
        {
            CamEffects.Effects.Remove(_effect);
        }

        protected override void UpdateAnyOwner()
        {
            shake.Update(Time.deltaTime);
            _effect.AddedPosition = shake.GetShake2D();
        }
    }
}