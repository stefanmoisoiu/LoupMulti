using System;
using Base_Scripts;
using Networking;
using Networking.Connection;
using Player.Camera;
using Player.Networking;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PAnimVerticalTilt : PAnimBehaviour
    {
        private AnimComponent _animComponent = new() { Target = PAnimManager.Target.Body };
        
        [Header("References")]
        [SerializeField] private Camera.PCamera cam;

        [Header("Tilt properties")]
        [SerializeField] [Range(0,1)] private float tiltMult = 0.5f;

    
    
        [Header("Properties")]
        [SerializeField] private float springConstant = 15f;
        [SerializeField] private float dampingFactor = 3f;
    
        private float _currentTilt;
        private float _currentVelocity;

        protected override void StartOnlineNotOwner()
        {
            _currentTilt = cam.LookDir.y;
        }

        private void Update()
        {
            CalculateTilt();
            _animComponent.Rotation = Quaternion.Euler(_currentTilt*tiltMult, 0, 0);
        }
        

        private void CalculateTilt()
        {
            float targetTilt = cam.LookDir.y;
            float force = Spring.CalculateSpringForce(_currentTilt, targetTilt , _currentVelocity, springConstant, dampingFactor);
            _currentVelocity += force * Time.fixedDeltaTime;
            _currentTilt += _currentVelocity * Time.fixedDeltaTime;
        }
        
        public override AnimComponent[] GetAnimComponents() => new[] {_animComponent};
    }
}
