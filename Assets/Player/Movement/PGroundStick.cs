using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PGroundStick : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float springConstant = 1000;
    [SerializeField] private float dampingFactor = 100;
    [SerializeField] private float offset = 0.1f;

    [SerializeField] private bool debug;

    [SerializeField] private PJump jump;
    [SerializeField] private PGrounded grounded;
    
    private void FixedUpdate()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        StickToGround();
    }
    
    private void StickToGround()
    {
        if(jump.JumpCooldown) return;
        if (!grounded.IsGrounded)
        {
            rb.useGravity = true;
            return;
        }
        rb.useGravity = false;
        float force = Spring.CalculateSpringForce(rb.position.y, grounded.GroundHit.point.y + offset, rb.velocity.y, springConstant, dampingFactor);
        rb.AddForce(Vector3.up * force, ForceMode.Force);
        
        if (debug)
        {
            Debug.DrawLine(rb.position, new Vector3(rb.position.x, grounded.GroundHit.point.y + offset, rb.position.z), Color.red);
        }
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(rb.position.x, grounded.GroundHit.point.y + offset, rb.position.z), 0.1f);
    }
}
