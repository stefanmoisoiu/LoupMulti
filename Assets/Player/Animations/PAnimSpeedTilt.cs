using System;
using Base_Scripts;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PAnimSpeedTilt : PAnimBehaviour
    {
        private AnimComponent _animComponent = new() { Target = PAnimManager.Target.Body };
        
        private Vector3 _previousPosition;
        [SerializeField] private float tiltMultiplier = 10f;
    
        [SerializeField] private float springConstant = 0.1f;
        [SerializeField] private float dampingFactor = 0.1f;
    
        private Vector2 _currentTurn;
        private Vector2 _currentTurnVelocity;

        private void Start()
        {
            _previousPosition = transform.position;
        }

        private void CalculateTilt()
        {
            Vector3 deltaPosition = transform.position - _previousPosition;
            _previousPosition = transform.position;
        
            Vector2 targetTurn = new Vector2(deltaPosition.x, deltaPosition.z);
            Vector2 springForce = Spring.CalculateSpringForce(_currentTurn, targetTurn, _currentTurnVelocity, springConstant, dampingFactor);
            _currentTurnVelocity += springForce * Time.deltaTime;
            _currentTurn += _currentTurnVelocity * Time.deltaTime;
        }

        private void Update()
        {
            CalculateTilt();
            _animComponent.Rotation = Quaternion.Euler(_currentTurn.y * tiltMultiplier, 0, 0);
        }

        public override AnimComponent[] GetAnimComponents() => new[] {_animComponent};
    }
}
