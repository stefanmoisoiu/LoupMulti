using System.Collections;
using Player.Camera;
using Player.Movement;
using Player.Movement.Stamina;
using Player.Networking;
using UnityEngine;

namespace Player.Abilities
{
    public class WallBounce : Ability
    {
    
        [SerializeField] private float wallCheckDistance;
        [SerializeField] private float verticalBounceSpeed;
        [SerializeField] private float horizontalBounceSpeed;

        [SerializeField] private float bufferTime;
        private float _bufferTime;

        [SerializeField] private float camRotationLength = 0.25f;
        [SerializeField] private AnimationCurve camRotCurve;
        private Coroutine _camRotCoroutine;

        [SerializeField] private int wallBounceStaminaPartCost = 1;
    

        [SerializeField] private Transform orientation;
        [SerializeField] private Transform wallCheckPosition;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private Grounded grounded;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Camera.PCamera cam;
        [SerializeField] private Stamina stamina;


        public override void EnableAbility()
        {
            base.EnableAbility();
            //InputManager.instance.AddAbilityInputListener(AbilityInput, InputManager.ActionType.Start, PressedWallBounce);
        }

        public override void DisableAbility()
        {
            base.DisableAbility();
            //InputManager.instance.RemoveAbilityInputListener(AbilityInput, InputManager.ActionType.Start, PressedWallBounce);
        }

        protected override void UpdateAnyOwner()
        {
            if (!AbilityEnabled) return;
        
            if (_bufferTime > 0) TryWallBounce();
            _bufferTime -= Time.deltaTime;
        }

        private void PressedWallBounce()
        {
            if (!CanWallBounce() || !WallInFront(out _))
            {
                _bufferTime = bufferTime;
                return;
            }
            TryWallBounce();
        }

        private void TryWallBounce()
        {
            if (!CanWallBounce()) return;
            if (!WallInFront(out RaycastHit hit)) return;
            if (!stamina.HasEnoughStamina(wallBounceStaminaPartCost)) return;
        
            Vector3 force = hit.normal * horizontalBounceSpeed + Vector3.up * verticalBounceSpeed;
            rb.linearVelocity = force;
        
            if (_camRotCoroutine != null) StopCoroutine(_camRotCoroutine);
            _camRotCoroutine = StartCoroutine(RotateCamera(hit.normal));
        
            stamina.DecreaseStamina(wallBounceStaminaPartCost);
        
            _bufferTime = 0;
        }

        private bool CanWallBounce() => !grounded.FullyGrounded();

        private bool WallInFront(out RaycastHit hit)
        {
            return Physics.Raycast(wallCheckPosition.position, orientation.forward, out hit, wallCheckDistance, wallLayer);
        }

        private IEnumerator RotateCamera(Vector3 wallNormal)
        {
            int sign = Vector3.Dot(wallNormal, orientation.right) > 0 ? 1 : -1;
            float angleToRotate = Vector3.Angle(orientation.forward, new Vector3(wallNormal.x,0,wallNormal.z).normalized);
            float currentRot = 0;
            float adv = 0;
            while (adv < 1)
            {
                float curveValue = camRotCurve.Evaluate(adv);
                float angle = curveValue * angleToRotate * sign;
                float delta = angle - currentRot;
                cam.AddRotation(new Vector2(delta, 0));
                currentRot = angle;
                adv += Time.deltaTime / (angleToRotate/180) / camRotationLength;
                yield return null;
            }
        }
    }
}
