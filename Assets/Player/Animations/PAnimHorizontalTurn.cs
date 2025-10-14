using Base_Scripts;
using Networking;
using Networking.Connection;
using Player.Camera;
using Player.Networking;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PAnimHorizontalTurn : PNetworkBehaviour
    {
        [SerializeField] private PAnimManager animManager;
        private AnimComponent _animComponent = new() { Target = PAnimManager.Target.Body };
        
        [SerializeField] private Camera.PCamera cam;
        [Header("Tilt properties")]
        [SerializeField] private float turnSpringConstant = 0.1f;
        [SerializeField] private float turnDampingFactor = 0.1f;
    
        private float _currentTurn;
        private float _currentTurnVelocity;

        protected override void StartOnlineNotOwner()
        {
            _currentTurn = cam.lookTargetNet.Value.x;
            animManager.AddAnim(_animComponent);
        }

        protected override void StartAnyOwner()
        {
            animManager.AddAnim(_animComponent);
        }

        private void Update()
        {
            CalculateTilt();

            _animComponent.Rotation = Quaternion.Euler(0, 0, _currentTurn);
        }

        private void CalculateTilt()
        {
            float targetTilt = IsOnline && !IsOwner ? cam.lookTargetNet.Value.x : cam.LookTarget.x;
            float headForce = Spring.CalculateSpringForce(_currentTurn, targetTilt , _currentTurnVelocity, turnSpringConstant, turnDampingFactor);
            _currentTurnVelocity += headForce * Time.fixedDeltaTime;
            _currentTurn += _currentTurnVelocity * Time.fixedDeltaTime;
        }
    }
}
