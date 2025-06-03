using Base_Scripts;
using Game.Common;
using Game.Data;
using Input;
using Player.Abilities.Grappling;
using Player.Networking;
using Player.Stats;
using UnityEngine;

namespace Player.Movement
{
    public class Movement : PNetworkBehaviour
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

        [SerializeField] private Run run;
        [SerializeField] private Grounded grounded;
        [SerializeField] private Grappling grappling;
    
    
        public float GetMaxSpeed()
        {
            if (grappling.IsGrappling) return maxGrappleSpeed;
            float maxSpeed = PlayerStats.MaxSpeed.Apply(maxWalkSpeed);
            if (run.Running) maxSpeed += maxRunSpeed - maxWalkSpeed;
            return maxSpeed;
        }

        private float GetAcceleration()
        {
            if (grappling.IsGrappling) return grappleAcceleration;
            float a = PlayerStats.Acceleration.Apply(acceleration);
            if (!grounded.FullyGrounded()) a *= airAccelMultiplier;
            return a;
        }

        protected override void FixedUpdateAnyOwner()
        {
            if (DataManager.Instance[NetworkManager.LocalClientId].outerData.playingState ==
                OuterData.PlayingState.SpectatingGame) return;
            Move();
        }

        private void Move()
        {
            Vector3 dir = orientation.forward * InputManager.Move.y + orientation.right * InputManager.Move.x;
        
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
}
