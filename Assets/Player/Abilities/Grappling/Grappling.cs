using System.Collections;
using Base_Scripts;
using Player.Movement;
using Player.Movement.Stamina;
using Player.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Player.Abilities.Grappling
{
    public class Grappling : Ability
    {
        [SerializeField] private float springConstant = 1000;
        [SerializeField] private float dampingFactor = 10;

        [SerializeField] private float grappleJumpForce = 5;
    
        [SerializeField] private float minGrappleDist = 2;
        [SerializeField] private float grappleDelay = 0.5f;
        private Coroutine grappleDelayCoroutine;

        [SerializeField] private int staminaPartCost = 2;
    

        // [SerializeField] private float predictionRadius = .75f;
        // [SerializeField] private int predictionResolution = 3;
        [SerializeField] private float maxGrappleDist;
        [SerializeField] private LayerMask grapplingMask;

        private Vector3 _grapplePoint;
        private float _grappleSpringDist;

        [SerializeField] private float grappleGravityMult = 1;
    

        [SerializeField] private Grounded grounded;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform head;
        [SerializeField] private Stamina stamina;
    
        public bool IsGrappling { get; private set; }
    
        private NetworkVariable<bool> grappling = new (writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Vector3> grapplePoint = new (writePerm: NetworkVariableWritePermission.Owner);

        public Vector3 GetUpVector() => IsGrappling ? (_grapplePoint - rb.position).normalized : Vector3.up;
    
        public override void EnableAbility()
        {
            base.EnableAbility();
            Debug.Log("Grappling enabled");
            // InputManager.instance.OnJump += TryJumpGrapple;
            //InputManager.instance.AddAbilityInputListener(AbilityInput, InputManager.ActionType.Start, PressedGrapple);
        }

        public override void DisableAbility()
        {
            base.DisableAbility();
            Debug.Log("Grappling disabled");
            // InputManager.instance.OnJump -= TryJumpGrapple;
            //InputManager.instance.RemoveAbilityInputListener(AbilityInput, InputManager.ActionType.Start, PressedGrapple);
        }

        protected override void UpdateAnyOwner()
        {
            if (!AbilityEnabled) return;
        
            if (IsGrappling && !CanGrapple()) StopGrapple();
            if(IsGrappling) UpdateGrapple();
        }

        private void UpdateGrapple()
        {
            Vector3 dir = (_grapplePoint - rb.position).normalized;
            if (dir.y < 0 && rb.linearVelocity.y > 0)
            {
                StopGrapple();
                return;
            }
        
            float dist = Vector3.Distance(rb.position, _grapplePoint);
        
            float velTowardsPoint = Vector3.Dot(rb.linearVelocity, dir);
        
            float force = Spring.CalculateSpringForce(_grappleSpringDist, dist, velTowardsPoint, springConstant, dampingFactor);
            Vector3 forceToApply = dir * force;
            if (_grapplePoint.y - rb.position.y < 0) forceToApply.y = 0;
            rb.AddForce(forceToApply, ForceMode.Force);

            Vector3 gravityUp = GetUpVector();
            gravityUp.y = Mathf.Abs(gravityUp.y);
            Vector3 gravityApplyDir = Vector3.ProjectOnPlane(new Vector3(dir.x, 0, dir.z),gravityUp);
            rb.AddForce(gravityApplyDir * (Physics.gravity.magnitude * grappleGravityMult), ForceMode.Force);
        
            Debug.DrawLine(rb.position, _grapplePoint, Color.green);
        }

        protected override void UpdateOnlineNotOwner()
        {
            if (grappling.Value) Debug.LogError("Other player grappling position : " + grapplePoint.Value);
        }

        private void PressedGrapple()
        {
            if (IsGrappling)
            {
                StopGrapple();
            }
            else
            {
                TryStartGrapple();
            }
        }
        public bool GrapplingRaycast(out RaycastHit hit)
        {
            if (Physics.Raycast(head.position, head.forward, out hit, maxGrappleDist, grapplingMask)) return true;
            // for (int i = 0; i < predictionResolution; i++)
            // {
            //     if (Physics.SphereCast(
            //             head.position,
            //             predictionRadius * (i + 1) / predictionResolution,
            //             head.forward,
            //             out hit, maxGrappleDist,
            //             grapplingMask)) return true;
            // }

            return false;
        }
        private void TryStartGrapple()
        {
            if (!CanGrapple()) return;
            if (!stamina.HasEnoughStamina(staminaPartCost)) return;
            if (!GrapplingRaycast(out RaycastHit hit)) return;
        
            _grapplePoint = hit.point;
        
            if(IsSpawned)
                grapplePoint.Value = _grapplePoint;
        
            if (grappleDelayCoroutine != null) StopCoroutine(grappleDelayCoroutine);
        
            StartCoroutine(GrappleDelay());
        }
        private IEnumerator GrappleDelay()
        {
            yield return new WaitForSeconds(grappleDelay);
            if (!CanGrapple()) yield break;
            if (!stamina.HasEnoughStamina(staminaPartCost)) yield break;
            stamina.DecreaseStamina(staminaPartCost);
            StartGrapple();
        }
        private void StartGrapple()
        {
            _grappleSpringDist = Mathf.Max(minGrappleDist,Vector3.Distance(rb.position, _grapplePoint));
            IsGrappling = true;
            rb.useGravity = false;
        
            if(IsSpawned)
                grappling.Value = true;
        }
        private void StopGrapple()
        {
            IsGrappling = false;
            rb.useGravity = true;
        
            if(IsSpawned)
                grappling.Value = false;
        }

        private void TryJumpGrapple()
        {
            if (!IsGrappling) return;
            if (rb.linearVelocity.y < 0) rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.linearVelocity += Vector3.up * grappleJumpForce;
            StopGrapple();
        }
    
        private bool CanGrapple() => !grounded.FullyGrounded();
    }
}