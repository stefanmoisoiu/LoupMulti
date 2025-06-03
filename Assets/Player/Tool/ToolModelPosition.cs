using Base_Scripts;
using Player.Hitbox;
using Player.Networking;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Player.Tool
{
    public class ToolModelPosition : PNetworkBehaviour
    {
        public enum HoldPosition
        {
            Hands,
            Target
        }

        private Vector3 basePosition;
        private Quaternion baseRotation;
    
        [TitleGroup("References")]
        [SerializeField] private Transform head;
        [TitleGroup("References")]
        [SerializeField] private Transform model;
        [TitleGroup("Settings")] [SerializeField]
        private Vector3 rotationOffset;

    
        [TitleGroup("Spring")] [SerializeField]
        private float springDamping = 0.5f;
        [TitleGroup("Spring")] [SerializeField]
        private float springConstant = 0.5f;
        [TitleGroup("Spring")] [SerializeField]
        private float angleSpringDamping = 0.5f;
        [TitleGroup("Spring")] [SerializeField]
        private float angleSpringConstant = 0.5f;
        [TitleGroup("Noise")] [SerializeField]
        private float noiseOffsetSpeed = 0.5f;
        [TitleGroup("Noise")] [SerializeField]
        private float noiseScale = .2f;

    

        private Quaternion currentRotation;
        private Quaternion targetRotation;
        private Vector3 angleVelocity;
        private Vector3 currentPosition;
        private Vector3 targetPosition;
        private Vector3 springVelocity;
    
    

        [SerializeField] private float distanceToTarget = 2;
        private NetworkVariable<NetworkObjectReference> target = new (new(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<HoldPosition> holdPosition = new (HoldPosition.Hands, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private NetworkObject targetRef;
        private HitboxTarget hitboxTarget;

        protected override void StartOnlineOwner()
        {
            base.StartOnlineOwner();
            Setup();
            target.OnValueChanged += UpdateRefs;
        }

        protected override void StartOnlineNotOwner()
        {
            base.StartOnlineNotOwner();
            Setup();
            target.OnValueChanged += UpdateRefs;
        }

        protected override void DisableOnlineOwner()
        {
            base.DisableOnlineOwner();
            target.OnValueChanged -= UpdateRefs;
        }
        protected override void DisableOnlineNotOwner()
        {
            base.DisableOnlineNotOwner();
            target.OnValueChanged -= UpdateRefs;
        }

        private void Setup()
        {
            basePosition = model.localPosition;
            currentPosition = head.TransformPoint(basePosition);
            baseRotation = model.localRotation;
            currentRotation = head.rotation * baseRotation;
        }

        private void UpdateRefs(NetworkObjectReference previousTarget, NetworkObjectReference newTarget)
        {
            if (!target.Value.TryGet(out targetRef))
            {
                targetRef = null;
                hitboxTarget = null;
                return;
            }
            targetRef.TryGetComponent(out hitboxTarget);
        }

        public void SetPosition(HoldPosition holdPosition, NetworkObjectReference target = new())
        {
            this.holdPosition.Value = holdPosition;
            this.target.Value = target;
        }

        private void Update()
        {
            if (holdPosition.Value == HoldPosition.Target && targetRef != null)
            {
                Vector3 targetCenter = hitboxTarget != null ? hitboxTarget.RectOutline.Renderer.bounds.center : targetRef.transform.position;
                Vector3 dir = targetCenter - head.position;
                dir = Quaternion.Euler(rotationOffset) * dir;
                dir.Normalize();
                targetPosition = targetCenter - dir * distanceToTarget;
                targetRotation = Quaternion.LookRotation(dir);
            }
            else
            {
                targetPosition = head.TransformPoint(basePosition);
                targetRotation = head.rotation * baseRotation;
            }
        
            float noiseX = (PerlinNoise.GenerateNoise(Time.time * noiseOffsetSpeed) - 0.5f) * 2 * noiseScale;
            float noiseY = (PerlinNoise.GenerateNoise(Time.time * noiseOffsetSpeed + 1000) - 0.5f) * 2 * noiseScale;
            float noiseZ = (PerlinNoise.GenerateNoise(Time.time * noiseOffsetSpeed + 2000) - 0.5f) * 2 * noiseScale;
            targetPosition += new Vector3(noiseX, noiseY, noiseZ);
        
            UpdateSpring();
        
            model.position = currentPosition;
            model.rotation = currentRotation;
        }

        private void UpdateSpring()
        {
            Vector3 force = Spring.CalculateSpringForce(currentPosition, targetPosition, springVelocity, springConstant, springDamping);
            springVelocity += force * Time.deltaTime;
            currentPosition += springVelocity * Time.deltaTime;
        
            Spring.IntegrateSpringRotation(
                ref currentRotation,
                ref angleVelocity,
                targetRotation,
                angleSpringConstant,
                angleSpringDamping,
                Time.deltaTime
            );
        }
    }
}
