using System;
using Unity.Netcode;
using UnityEngine;

public class PGrounded : NetworkBehaviour
{
    [SerializeField] private float rayLength = 0.5f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool debug = true;
    
    
    public bool IsGrounded { get; private set; }
    public RaycastHit GroundHit { get; private set; }
    public Quaternion WorldToGround { get; private set; }
    public Quaternion GroundToWorld { get; private set; }

    public Action<bool,bool> OnGroundedChanged;

    [SerializeField] private PJump jump;
    
    public bool FullyGrounded() =>  IsGrounded && !jump.JumpCooldown;
    private void Update()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        CheckGrounded();
    }
    private void CheckGrounded()
    {
        bool WasGrounded = IsGrounded;
        IsGrounded = Physics.Raycast(
            transform.position, Vector3.down, out var hit, rayLength, groundMask);
        GroundHit = hit;
        WorldToGround = Quaternion.FromToRotation(IsGrounded ? hit.normal : Vector3.up, Vector3.up);
        GroundToWorld = Quaternion.FromToRotation(Vector3.up, IsGrounded ? hit.normal : Vector3.up);
        
        if (WasGrounded != IsGrounded)
        {
            OnGroundedChanged?.Invoke(WasGrounded, IsGrounded);
        }
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;
        bool debugGrounded = Physics.Raycast(
            transform.position, Vector3.down, out var hit, rayLength, groundMask);
        Gizmos.color = debugGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayLength);
    }
}