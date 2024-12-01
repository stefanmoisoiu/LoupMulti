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
    
    public List<MoveSpeedModifiers> moveSpeedModifiers { get; private set; } = new();
    
    public void AddMoveSpeedModifier(MoveSpeedModifiers modifier)
    {
        moveSpeedModifiers.Add(modifier);
    }
    private float GetMaxSpeedFactor()
    {
        float maxSpeedFactor = 1;
        foreach (var modifier in moveSpeedModifiers)
        {
            maxSpeedFactor *= modifier.maxSpeedFactor;
        }
        
        

        return maxSpeedFactor;
    }
    private float GetAccelerationFactor()
    {
        float accelerationFactor = 1;
        foreach (var modifier in moveSpeedModifiers)
        {
            accelerationFactor *= modifier.accelerationFactor;
        }

        return accelerationFactor;
    }
    private float GetAddedMaxSpeed()
    {
        float addedMaxSpeed = 0;
        foreach (var modifier in moveSpeedModifiers)
        {
            addedMaxSpeed += modifier.addedMaxSpeed;
        }

        return addedMaxSpeed;
    }
    private float GetAddedAcceleration()
    {
        float addedAcceleration = 0;
        foreach (var modifier in moveSpeedModifiers)
        {
            addedAcceleration += modifier.addedAcceleration;
        }

        return addedAcceleration;
    }
    
    private float GetMaxSpeed()
    {
        if (grappling.Grappling) return maxGrappleSpeed;
        float maxSpeed = maxWalkSpeed;
        maxSpeed *= GetMaxSpeedFactor();
        maxSpeed += GetAddedMaxSpeed();
        if (run.Running) maxSpeed += maxRunSpeed - maxWalkSpeed;
        return maxSpeed;
    }

    private float GetAcceleration()
    {
        if (grappling.Grappling) return grappleAcceleration;
        float a = acceleration;
        a *= GetAccelerationFactor();
        a += GetAddedAcceleration();
        if (!grounded.FullyGrounded()) a *= airAccelMultiplier;
        return a;
    }

    private void FixedUpdate()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        Move();
    }

    private void Move()
    {
        Vector3 dir = orientation.forward * InputManager.instance.MoveInput.y + orientation.right * InputManager.instance.MoveInput.x;
        
        Vector3 force = dir * GetAcceleration();

        Vector3 vel = rb.linearVelocity;
        vel = grounded.WorldToLocalUp * vel;

        float y = vel.y;
        vel.y = 0;
        
        float maxSpeed = GetMaxSpeed();
        if ((vel + force * Time.fixedDeltaTime).magnitude > maxSpeed && Vector3.Dot(vel, force) > 0)
        {
            if (vel.magnitude < maxSpeed)
            {
                force = ((vel + force * Time.fixedDeltaTime).normalized * maxSpeed - vel) / Time.fixedDeltaTime;
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

    public class MoveSpeedModifiers
    {
        public float maxSpeedFactor;
        public float accelerationFactor;

        public float addedMaxSpeed;
        public float addedAcceleration;
        
        public MoveSpeedModifiers(float maxSpeedFactor = 1, float accelerationFactor = 1, float addedMaxSpeed = 0, float addedAcceleration = 0)
        {
            this.maxSpeedFactor = maxSpeedFactor;
            this.accelerationFactor = accelerationFactor;
            
            this.addedMaxSpeed = addedMaxSpeed;
            this.addedAcceleration = addedAcceleration;
        }
    }
}
