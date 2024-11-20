using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PMovement : NetworkBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;
    
    [SerializeField] private float maxWalkSpeed = 10f;
    [SerializeField] private float maxRunSpeed = 20f;
    [SerializeField] private float maxGrappleSpeed = 12f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float grappleAcceleration = 30f;
    [SerializeField] private float airAccelMultiplier = 0.5f;

    [SerializeField] private PRun run;
    [SerializeField] private PGrounded grounded;
    [SerializeField] private PGrappling grappling;
    
    
    
    
    private float MaxSpeed => grappling.Grappling ? maxGrappleSpeed : run.Running ? maxRunSpeed : maxWalkSpeed;
    private float Acceleration => grappling.Grappling ? grappleAcceleration : grounded.FullyGrounded() ? acceleration : acceleration * airAccelMultiplier;
    private void FixedUpdate()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        Move();
    }

    private void Move()
    {
        Vector3 dir = orientation.forward * InputManager.instance.MoveInput.y + orientation.right * InputManager.instance.MoveInput.x;
        
        Vector3 force = dir * Acceleration;

        Vector3 vel = rb.linearVelocity;
        vel = grounded.WorldToLocalUp * vel;

        float y = vel.y;
        vel.y = 0;
        
        if ((vel + force * Time.fixedDeltaTime).magnitude > MaxSpeed && Vector3.Dot(vel, force) > 0)
        {
            if (vel.magnitude < MaxSpeed)
            {
                force = ((vel + force * Time.fixedDeltaTime).normalized * MaxSpeed - vel) / Time.fixedDeltaTime;
            }
            else
            {
                Vector3 perpendicularVel = Vector3.Cross(vel.normalized, Vector3.up);
                force = Vector3.Project(force, perpendicularVel);
            }
        }
        
        vel += force * Time.fixedDeltaTime;
        // vel = Vector3.ClampMagnitude(vel, );
        
        vel.y = y;
        
        vel = grounded.LocalUpToWorld * vel;
        
        rb.linearVelocity = vel;
        
        Debug.DrawRay(transform.position, rb.linearVelocity, Color.red);
        Debug.DrawRay(transform.position + rb.linearVelocity,force * Time.fixedDeltaTime, Color.green);
    }
}
