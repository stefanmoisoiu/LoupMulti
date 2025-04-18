using System;
using Unity.Netcode;
using UnityEngine;

public class PGrounded : PNetworkBehaviour
{
    [SerializeField] private float rayLength = 0.5f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool debug = true;


    public bool IsGrounded { get; private set; } = true;
    public NetworkVariable<bool> IsGroundedNet = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public RaycastHit GroundHit { get; private set; }
    public Quaternion WorldToLocalUp { get; private set; }
    public Quaternion LocalUpToWorld { get; private set; }

    public Action<bool,bool> OnGroundedChanged;

    [SerializeField] private PJump jump;
    [SerializeField] private PGrappling grappling;
    
    
    public bool FullyGrounded() =>  IsGrounded && !jump.JumpCooldown;

    protected override void UpdateAnyOwner()
    {
        CheckGrounded();
    }

    private Vector3 LocalUp()
    {
        if (IsGrounded) return GroundHit.normal;
        if (grappling.Grappling) return grappling.GetUpVector();
        return Vector3.up;
    }
    private void CheckGrounded()
    {
        bool WasGrounded = IsGrounded;
        IsGrounded = Physics.Raycast(
            transform.position, Vector3.down, out var hit, rayLength, groundMask);
        IsGroundedNet.Value = IsGrounded;
        GroundHit = hit;

        Vector3 localUp = LocalUp();
        WorldToLocalUp = Quaternion.FromToRotation(localUp, Vector3.up);
        LocalUpToWorld = Quaternion.FromToRotation(Vector3.up, localUp);
        
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