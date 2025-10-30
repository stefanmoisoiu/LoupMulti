using Game.Common;
using Input;
using Player.Networking;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Player.Camera
{
    public class PCamera : PNetworkBehaviour
    {
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform head;
        [SerializeField] private float cameraSmoothSpeed = 35;
        [SerializeField] private CinemachineCamera cam;
    
        [SerializeField] [Range(0,3)] private float sensMult = 1;

        public NetworkVariable<Vector2> lookTargetNet = new(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
        public Vector2 LookTarget { get; private set; } 
        public Vector2 LookDir { get; private set; }
        public Vector2 LookDelta { get; private set; }

        public const float MaxTilt = 90;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner) return;
            cam.enabled = false;
            return;
        }

        protected override void UpdateAnyOwner()
        {
            if (CursorManager.Instance.IsCursorLocked) GetLookTarget();
            Look();
        }

        protected override void UpdateOnlineOwner()
        {
            lookTargetNet.Value = LookTarget;
        }

        private void Look()
        {
            LookDir = Vector2.Lerp(LookDir, LookTarget, cameraSmoothSpeed * Time.deltaTime);
            orientation.localRotation = Quaternion.Euler(0, LookDir.x, 0);
            head.localRotation = Quaternion.Euler(-LookDir.y, 0, 0);
        }

        private void GetLookTarget()
        {
            LookDelta = InputManager.Look * sensMult;
            LookTarget += LookDelta;
            LookTarget = new(LookTarget.x,Mathf.Clamp(LookTarget.y, -MaxTilt, MaxTilt));
        }
    
        public void AddRotation(Vector2 rotation)
        {
            LookTarget += rotation;
            LookTarget = new(LookTarget.x, Mathf.Clamp(LookTarget.y, -MaxTilt, MaxTilt));
        }
    }
}
