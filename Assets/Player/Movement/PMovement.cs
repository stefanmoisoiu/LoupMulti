using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PMovement : PNetworkBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;
    
    [SerializeField] private float maxWalkSpeed = 10f;
    [SerializeField] private float maxRunSpeed = 20f;
    public float MaxRunSpeed => maxRunSpeed;
    [SerializeField] private float maxGrappleSpeed = 12f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float grappleAcceleration = 30f;
    [SerializeField] private float airAccelMultiplier = 0.5f;

    [SerializeField] private PRun run;
    [SerializeField] private PGrounded grounded;
    [SerializeField] private PGrappling grappling;
    
    public Modifier<float> MaxSpeedModifier = new();
    public Modifier<float> AccelerationModifier = new();
    
    
    public float GetMaxSpeed()
    {
        if (grappling.Grappling) return maxGrappleSpeed;
        float maxSpeed = MaxSpeedModifier.Apply(maxWalkSpeed);
        if (run.Running) maxSpeed += maxRunSpeed - maxWalkSpeed;
        return maxSpeed;
    }

    private float GetAcceleration()
    {
        if (grappling.Grappling) return grappleAcceleration;
        float a = AccelerationModifier.Apply(acceleration);
        if (!grounded.FullyGrounded()) a *= airAccelMultiplier;
        return a;
    }

    protected override void FixedUpdateAnyOwner()
    {
        Move();
    }

    private void Move()
    {
        Vector3 dir = orientation.forward * InputManager.instance.MoveInput.y + orientation.right * InputManager.instance.MoveInput.x;
        
        float accel = GetAcceleration();
        Vector3 force = dir * accel;

        Vector3 vel = rb.linearVelocity;
        vel = grounded.WorldToLocalUp * vel;

        float y = vel.y;
        vel.y = 0;
        
        float maxSpeed = GetMaxSpeed();

        Vector3 nextVel = vel + force * Time.fixedDeltaTime;
        Vector3 clampedVel = Vector3.ClampMagnitude(nextVel, maxSpeed);
        
        if (nextVel.magnitude > maxSpeed && Vector3.Dot(vel, force) > 0)
        {
            if (vel.magnitude < maxSpeed)
            {
                Vector3 newDir = (clampedVel - vel).normalized;
                force = newDir * accel;
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
