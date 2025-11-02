using Game.Common;
using Input;
using Player.Networking;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Player.Camera
{
    public class PCamera : PNetworkBehaviour
    {
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform head;
        [SerializeField] private float lookDirSmoothSpeed = 50;
        [SerializeField] private float netLookDirSmoothSpeed = 35;
        [SerializeField] private CinemachineCamera cam;
    
        [SerializeField] [Range(0,3)] private float sensMult = 1;
        
        public Vector2 LookTarget { get; private set; } 
        public Vector2 LookDir { get; private set; }
        public Vector2 LookDelta { get; private set; }

        public const float MaxTilt = 90;

        private const int SendLookTargetRate = 20;
        private const float TimeBtwLookTargetSend = 1f / SendLookTargetRate;
        private float _sendLookTargetTimer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner) return;
            cam.enabled = false;
            return;
        }

        protected override void UpdateAnyOwner()
        {
            if (CursorManager.Instance.IsCursorLocked) UpdateLookOwner();
            SmoothLookDir(LookTarget, lookDirSmoothSpeed);
            Look(LookDir);
        }

        protected override void UpdateOnlineNotOwner()
        {
            SmoothLookDir(LookTarget, netLookDirSmoothSpeed);
            Look(LookDir);
        }

        private void SmoothLookDir(Vector2 target, float smoothSpeed)
        {
            LookDir = Vector2.Lerp(LookDir, target, smoothSpeed * Time.deltaTime);
        }
        private void Look(Vector2 dir)
        {
            orientation.localRotation = Quaternion.Euler(0, dir.x, 0);
            head.localRotation = Quaternion.Euler(-dir.y, 0, 0);
        }

        private void UpdateLookOwner()
        {
            LookDelta = InputManager.Look * sensMult;
            LookTarget += LookDelta;
            LookTarget = new(LookTarget.x,Mathf.Clamp(LookTarget.y, -MaxTilt, MaxTilt));

            if (!IsOnline) return;
            
            _sendLookTargetTimer += Time.deltaTime;
            if (_sendLookTargetTimer < TimeBtwLookTargetSend) return;
            
            _sendLookTargetTimer -= TimeBtwLookTargetSend;
            LookTargetServerRpc(LookTarget);
        }
        
        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        private void LookTargetServerRpc(Vector2 lookTarget) => LookTargetClientRpc(lookTarget);

        [ClientRpc(Delivery = RpcDelivery.Unreliable)]
        private void LookTargetClientRpc(Vector2 lookTarget)
        {
            if (IsOwner) return;
            LookTarget = lookTarget;
        }
        public void AddRotation(Vector2 rotation)
        {
            LookTarget += rotation;
            LookTarget = new(LookTarget.x, Mathf.Clamp(LookTarget.y, -MaxTilt, MaxTilt));
        }
    }
}
