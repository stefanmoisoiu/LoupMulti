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
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float airAccelMultiplier = 0.5f;

    [SerializeField] private PRun run;
    [SerializeField] private PGrounded grounded;
    
    
    
    private float MaxSpeed => run.Running ? maxRunSpeed : maxWalkSpeed;
    private void FixedUpdate()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        Move();
    }

    private void Move()
    {
        Vector3 dir = orientation.forward * InputManager.instance.MoveInput.y + orientation.right * InputManager.instance.MoveInput.x;
        
        Vector3 force = dir * acceleration;
        if (!grounded.FullyGrounded()) force *= airAccelMultiplier;

        Vector3 vel = rb.velocity;
        if (grounded.FullyGrounded()) vel = grounded.WorldToGround * vel;

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
        
        if (grounded.FullyGrounded()) vel = grounded.GroundToWorld * vel;
        
        rb.velocity = vel;
        
        Debug.DrawRay(transform.position, rb.velocity, Color.red);
        Debug.DrawRay(transform.position + rb.velocity,force * Time.fixedDeltaTime, Color.green);
    }
}
