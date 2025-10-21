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
        private AnimComponent _bodyAnimComponent = new() { Target = PAnimManager.Target.Body };
        private AnimComponent _bodyCogAnimComponent = new() { Target = PAnimManager.Target.BodyCog };
        private AnimComponent _headCogAnimComponent = new() { Target = PAnimManager.Target.HeadCog };
        
        [SerializeField] private Camera.PCamera cam;
        [Header("Tilt properties")]
        [SerializeField] private float turnSpringConstant = 0.1f;
        [SerializeField] private float turnDampingFactor = 0.1f;
    
        private float _currentTurn;
        private float _currentTurnVelocity;

        [SerializeField] [Range(0,2)] private float cogMult = 1f;
        

        protected override void StartOnlineNotOwner()
        {
            _currentTurn = cam.lookTargetNet.Value.x;
            animManager.AddAnim(_bodyAnimComponent);
            animManager.AddAnim(_bodyCogAnimComponent);
            animManager.AddAnim(_headCogAnimComponent);
        }

        protected override void StartAnyOwner()
        {
            animManager.AddAnim(_bodyAnimComponent);
            animManager.AddAnim(_bodyCogAnimComponent);
            animManager.AddAnim(_headCogAnimComponent);
        }

        private void Update()
        {
            CalculateTilt();

            _bodyAnimComponent.Rotation = Quaternion.Euler(0, 0, _currentTurn);
            _bodyCogAnimComponent.Rotation = Quaternion.Euler(0, 0, -_currentTurn * cogMult);
            _headCogAnimComponent.Rotation = Quaternion.Euler(0, 0, -_currentTurn * cogMult);
            
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
