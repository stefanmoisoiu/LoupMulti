using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PDrag : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] [Range(0,1)] private float drag;
    [SerializeField] [Range(0,1)] private float airDrag = 0.05f;

    [SerializeField] private PGrounded grounded;
    
    
    private void FixedUpdate()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        ApplyDrag();
    }
    
    private void ApplyDrag()
    {
        Vector3 vel = rb.velocity;
        
        if (grounded.FullyGrounded()) vel = grounded.WorldToGround * vel;
        float d = 1 - (grounded.FullyGrounded() ? drag : airDrag);
        vel = new(vel.x * d, vel.y, vel.z * d);
        
        if (grounded.FullyGrounded()) vel = grounded.GroundToWorld * vel;
        
        rb.velocity = vel;
    }
}
