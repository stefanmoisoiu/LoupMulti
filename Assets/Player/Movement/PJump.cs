using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PJump : PNetworkBehaviour
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

    [SerializeField] private PGrounded grounded;
    [SerializeField] private PStamina stamina;

    protected override void StartAnyOwner()
    {
        Debug.LogError("Jump enabled");
        InputManager.instance.OnJump += StartPressJump;
        grounded.OnGroundedChanged += CheckCoyote;
    }

    protected override void DisableAnyOwner()
    {
        Debug.LogError("Jump disabled");
        InputManager.instance.OnJump -= StartPressJump;
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
        
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        
        stamina.DecreaseStamina(jumpStaminaCost);
        
        OnJump?.Invoke();
    }

    private bool CanJump() => !JumpCooldown &&
                              (grounded.IsGrounded || _jumpCoyote >= 0) &&
                              stamina.Stamina >= jumpStaminaCost;
}
