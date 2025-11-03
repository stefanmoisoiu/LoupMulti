using System;
using Base_Scripts;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PAnimSpeedTilt : PAnimBehaviour
    {
        private AnimComponent _animComponent = new() { Target = PAnimManager.Target.Body, IsLocal = false };

        [SerializeField] private bool invertDirection = false;
        
        private Vector3 _previousPosition;
        [SerializeField] private float maxAngle = 45f;
        [SerializeField] private float maxAngleSpeed = 2f;
        
        private float UpdateRate => 1f / 60f;
        private float _timer;
    
        [SerializeField] private float springConstant = 0.1f;
        [SerializeField] private float dampingFactor = 0.1f;
    
        private Vector2 _worldTiltTarget;
        private Vector2 _worldTilt;
        private Vector2 _worldTiltVelocity;

        private void Start()
        {
            _previousPosition = transform.position;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= UpdateRate)
            {
                Vector3 deltaPosition = transform.position - _previousPosition;
                _previousPosition = transform.position;
                
                Vector2 delta = (invertDirection ? -1 : 1) * new Vector2(deltaPosition.x, deltaPosition.z) / _timer;
                _worldTiltTarget = new(
                    Mathf.Clamp(delta.x, -maxAngleSpeed, maxAngleSpeed) / maxAngleSpeed * maxAngle,
                    Mathf.Clamp(delta.y, -maxAngleSpeed, maxAngleSpeed) / maxAngleSpeed * maxAngle
                );
                _timer = 0;
            }
            Vector2 force = Spring.CalculateSpringForce(_worldTilt, _worldTiltTarget, _worldTiltVelocity, springConstant, dampingFactor);
            _worldTiltVelocity += force * Time.deltaTime;
            _worldTilt += _worldTiltVelocity * Time.deltaTime;
            
            _animComponent.Rotation = Quaternion.Euler(_worldTilt.y, _worldTilt.x, 0);
        }

        public override AnimComponent[] GetAnimComponents() => new[] {_animComponent};
    }
}
