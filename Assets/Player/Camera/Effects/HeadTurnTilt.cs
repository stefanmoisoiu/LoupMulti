using System;
using Player.Networking;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class HeadTurnTilt : PNetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
    
        [SerializeField] private float tiltLerpBackSpeed = 1;
        [SerializeField] private float minSpeedTiltFactor = 0.1f;
        [SerializeField] private float maxSpeedTiltFactor = 0.3f;
        [SerializeField] private float maxSpeedTiltSpeed = 20;
    
        private float _tilt = 0;
        private CamEffects.Effect _effect = new();

        [SerializeField] private PCamera cam;

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
            Tilt();
        }

        private void Tilt()
        {
            _tilt -= cam.LookDelta.x * Mathf.Lerp(minSpeedTiltFactor, maxSpeedTiltFactor, rb.linearVelocity.magnitude / maxSpeedTiltSpeed);
            _tilt = Mathf.Lerp(_tilt, 0, tiltLerpBackSpeed * Time.deltaTime);
        
            // headTurn.localRotation = Quaternion.Euler(0, 0, _tilt);
            _effect.Tilt.z = _tilt;
        }
    }
}
