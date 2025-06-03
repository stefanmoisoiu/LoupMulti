using System;
using Base_Scripts;
using Game.Common;
using Game.Data;
using Input;
using Player.Networking;
using Player.Stats;
using UnityEngine;

namespace Player.Movement
{
    public class Jump : PNetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
        
        
        [SerializeField] private float jumpForce;

        [SerializeField] private float jumpCooldown = 0.2f;
        private float _jumpCooldown;
        public bool JumpCooldown { get; private set; }

        [SerializeField] private float jumpCoyoteTime = 0.2f;
        private float _jumpCoyote;

        [SerializeField] private float jumpBufferTime = 0.2f;
        private float _jumpBuffer;

        [SerializeField] private float jumpStaminaCost = 0;
    
    
        private float _jumpAdv;

        // [SerializeField] private float jumpStaminaCost = 10f;
    
        public Action OnJump;

        [SerializeField] private Grounded grounded;
        [SerializeField] private Stamina.Stamina stamina;

        protected override void StartAnyOwner()
        {
            InputManager.OnJumpStarted += StartPressJump;
            grounded.OnGroundedChanged += CheckCoyote;
        }

        protected override void DisableAnyOwner()
        {
            InputManager.OnJumpStarted -= StartPressJump;
            grounded.OnGroundedChanged -= CheckCoyote;
        }

        private void CheckCoyote(bool wasGrounded, bool isGrounded)
        {
            if (wasGrounded && !isGrounded && !JumpCooldown)
            {
                _jumpCoyote = jumpCoyoteTime;
            }
        }

        protected override void UpdateAnyOwner()
        {
            if (DataManager.Instance[NetworkManager.LocalClientId].outerData.playingState ==
                OuterData.PlayingState.SpectatingGame) return;
            _jumpCoyote -= Time.deltaTime;
            _jumpBuffer -= Time.deltaTime;
        
            if (_jumpBuffer > 0)
            {
                StartJump();
            }
        
            _jumpCooldown -= Time.deltaTime;
            if (JumpCooldown && _jumpCooldown <= 0) JumpCooldown = false;
        }

        private void StartPressJump()
        {
            if (DataManager.Instance[NetworkManager.LocalClientId].outerData.playingState ==
                OuterData.PlayingState.SpectatingGame) return;
            if (!CanJump()) _jumpBuffer = jumpBufferTime;
            else StartJump();
        }

        private void StartJump()
        {
            if (!CanJump()) return;
        
            _jumpBuffer = 0;
            _jumpCoyote = 0;
        
            JumpCooldown = true;
            _jumpCooldown = jumpCooldown;
        
            rb.useGravity = true;

            float finalJumpForce = PlayerStats.JumpHeight.Apply(jumpForce);
        
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, finalJumpForce, rb.linearVelocity.z);
        
            stamina.DecreaseStamina(jumpStaminaCost);
        
            OnJump?.Invoke();
        }

        private bool CanJump() => !JumpCooldown &&
                                  (grounded.IsGrounded || _jumpCoyote >= 0) &&
                                  stamina.StaminaValue >= jumpStaminaCost;
    }
}
