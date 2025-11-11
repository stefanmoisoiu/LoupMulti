using System;
using System.Diagnostics;
using Base_Scripts;
using Game.Common;
using Game.Data;
using Input;
using Player.Networking;
using Player.Stats;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Player.Movement
{
    // Assurez-vous que PIDController est accessible (même namespace ou global)
    public class Movement : PNetworkBehaviour
    {
        [SerializeField] private Transform orientation;
        [SerializeField] private Rigidbody rb;
        
        private PlayerReferences _playerReferences;
        
        
        [Header("Main Movement")]
        [SerializeField] private float maxWalkSpeed = 10f;
        [SerializeField] private float maxRunSpeed = 20f;
        public float MaxRunSpeed => maxRunSpeed;
        
        [Header("Acceleration")]
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float airAccelMultiplier = 0.5f;
        [SerializeField] private float slideAcceleration = 25;

        [Header("Momentum")]
        [SerializeField] [Range(0,1)] private float momentumMult = .8f;
        [SerializeField] private float groundedMomentumDecayRate = 5;
        [SerializeField] private float airMomentumDecayRate = 2;

        private float _currentMomentumSpeed;

        
        
        [SerializeField] private Run run;
        [SerializeField] private Grounded grounded;

        [Header("PID Control")]
        [SerializeField] private float Kp = 8f;
        [SerializeField] private float Ki = 0.2f;
        [SerializeField] private float Kd = 1.5f;

        private PIDController pidX;
        private PIDController pidZ;

        private bool _isSliding = false;
        public bool IsSliding => _isSliding;

        protected override void StartAnyOwner()
        {
            _playerReferences = GetComponentInParent<PlayerReferences>();
            InitializePID();
        }

        
        private void InitializePID()
        {
            pidX ??= new PIDController(Kp, Ki, Kd);
            pidZ ??= new PIDController(Kp, Ki, Kd);

            pidX.Kp = Kp;
            pidZ.Kp = Kp;
            pidX.Ki = Ki;
            pidZ.Ki = Ki;
            pidX.Kd = Kd;
            pidZ.Kd = Kd;
        }
        public float GetBaseMaxSpeed()
        {
            float maxSpeed = maxWalkSpeed;
            if (run.Running) maxSpeed += maxRunSpeed - maxWalkSpeed;
            return _playerReferences == null ? maxSpeed : _playerReferences.StatManager.GetStat(StatType.MoveSpeed).GetValue(maxSpeed);
        }
        
        public float GetMaxSpeed() => Mathf.Max(GetBaseMaxSpeed(), _currentMomentumSpeed);
        
        private float GetAcceleration()
        {
            float accel = _playerReferences == null ? acceleration : _playerReferences.StatManager.GetStat(StatType.MoveSpeed).GetValue(acceleration);
            if (!grounded.FullyGrounded()) accel *= airAccelMultiplier;
            return accel;
        }

        protected override void FixedUpdateAnyOwner()
        {
            Vector2 input = InputManager.Move;
            HandleMovement(input);
        }

        private void HandleMovement(Vector2 input)
        {
            if (DataManager.Instance != null && DataManager.Instance[NetworkManager.LocalClientId].outerData.playingState ==
                OuterData.PlayingState.SpectatingGame) return;
            
            if (grounded.IsGrounded && !grounded.GroundAngleValid()) Slide(input);
            else Move(input);

            HandleMomentum();
            HandleMomentumDecay(Time.deltaTime);
        }
        
        private void Slide(Vector2 input)
        {
            if (!_isSliding)
            {
                pidX.Reset();
                pidZ.Reset();
            }
            _isSliding = true;
            Vector3 dir = orientation.forward * input.y + orientation.right * input.x;
            Vector3 groundNormalRight = Quaternion.Euler(0,90,0) * new Vector3(grounded.GroundHit.normal.x,0,grounded.GroundHit.normal.z).normalized;
            dir = Vector3.Project(dir, groundNormalRight);
            Vector3 rightVel = Vector3.Project(rb.linearVelocity, groundNormalRight);
            
            MoveDir(dir, rightVel);
            
            Vector3 slideDir = Vector3.Cross(groundNormalRight, grounded.GroundHit.normal).normalized;
            Vector3 force = slideDir * slideAcceleration;
            rb.AddForce(force, ForceMode.Force);
            
            Debug.DrawRay(transform.position, groundNormalRight, Color.green);
            Debug.DrawRay(transform.position, slideDir * 5, Color.blue);
        }
        private void Move(Vector2 input)
        {
            if (_isSliding)
            {
                pidX.Reset();
                pidZ.Reset();
            }
            _isSliding = false;
            Vector3 dir = orientation.forward * input.y + orientation.right * input.x;
            MoveDir(dir, rb.linearVelocity);
        }
        private void MoveDir(Vector3 dir, Vector3 velocity)
        {
            float maxSpeed = GetMaxSpeed();
            
            float maxAccel = GetAcceleration();
            float deltaTime = Time.fixedDeltaTime;

            Vector3 currentLocalVelocity = grounded.WorldToLocalUp * velocity;
            Vector3 desiredLocalVelocity = dir * maxSpeed;

            float accelX = pidX.Calculate(desiredLocalVelocity.x, currentLocalVelocity.x, deltaTime);
            float accelZ = pidZ.Calculate(desiredLocalVelocity.z, currentLocalVelocity.z, deltaTime);

            Vector3 localAcceleration = new(accelX, 0, accelZ);
            localAcceleration = Vector3.ClampMagnitude(localAcceleration, maxAccel);

            Vector3 worldAcceleration = grounded.LocalUpToWorld * localAcceleration;
            rb.AddForce(worldAcceleration, ForceMode.Force);
            Debug.DrawRay(transform.position, worldAcceleration, Color.red);
        }

        private void HandleMomentum()
        {
            Vector3 vel = rb.linearVelocity;
            if (grounded.FullyGrounded()) vel = grounded.WorldToLocalUp * vel;
            vel.y = 0;
            _currentMomentumSpeed = Mathf.Max(vel.magnitude * momentumMult, _currentMomentumSpeed);
        }
        
        private void HandleMomentumDecay(float deltaTime)
        {
            if (grounded.FullyGrounded()) _currentMomentumSpeed -= deltaTime * groundedMomentumDecayRate;
            else _currentMomentumSpeed -= deltaTime * airMomentumDecayRate; 
        }
    }
}