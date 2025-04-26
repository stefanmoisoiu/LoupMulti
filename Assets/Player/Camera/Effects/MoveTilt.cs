using Player.Movement;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class MoveTilt : MonoBehaviour
    {
        [SerializeField] private float tiltLerpSpeed = 1;
    
        [SerializeField] private float maxTiltSpeed = 15;
        [SerializeField] private float maxTiltAngle = 15;
    
        // [SerializeField] private Transform headMoveTilt;
        [SerializeField] private Transform orientation;

        private Vector2 _tilt;
        private CamEffects.Effect _effect = new();
    
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Grounded grounded;

        private void Start()
        {
            CamEffects.Effects.Add(_effect);
        }

        private void Update()
        {
            Tilt();
        }
    
        private void Tilt()
        {
            Vector3 vel = rb.linearVelocity;
            if (grounded.FullyGrounded()) vel = grounded.WorldToLocalUp * vel;
            vel.y = 0;
            Quaternion hRot = Quaternion.LookRotation(orientation.forward, Vector3.up);
            Vector3 velRelToOrientation = Quaternion.Inverse(hRot) * vel;

            float hTilt = Mathf.Lerp(-maxTiltAngle, maxTiltAngle, (Mathf.Clamp(-velRelToOrientation.x / maxTiltSpeed, -1, 1) + 1) / 2);
            float vTilt = Mathf.Lerp(-maxTiltAngle, maxTiltAngle, (Mathf.Clamp(velRelToOrientation.z / maxTiltSpeed, -1, 1) + 1) / 2);
        
            _tilt = Vector2.Lerp(_tilt, new Vector2(hTilt, vTilt), tiltLerpSpeed * Time.deltaTime);
        
            // headMoveTilt.localRotation = Quaternion.Euler(_tilt.y, 0, _tilt.x);
            _effect.Tilt = new Vector3(_tilt.y, 0, _tilt.x);
        }
    }
}
