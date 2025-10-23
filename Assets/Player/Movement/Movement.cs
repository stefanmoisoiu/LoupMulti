using System;
using Base_Scripts;
using Game.Common;
using Game.Data;
using Input;
using Player.Networking;
using Player.Stats;
using UnityEngine;

namespace Player.Movement
{
    // Assurez-vous que PIDController est accessible (même namespace ou global)
    public class Movement : PNetworkBehaviour
    {
        [SerializeField] private Transform orientation;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private FloatStat maxSpeedStat;
        [SerializeField] private FloatStat accelerationStat;
        
        private PlayerReferences _playerReferences;
        private StatManager StatManager => _playerReferences.StatManager;
        
        
        
        [SerializeField] private float maxWalkSpeed = 10f;
        [SerializeField] private float maxRunSpeed = 20f;
        public float MaxRunSpeed => maxRunSpeed;
        // L'accélération définit maintenant la force MAXIMALE que le PID est autorisé à appliquer.
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float airAccelMultiplier = 0.5f;

        [SerializeField] private float slideForce = 5;
        
        
        [SerializeField] private Run run;
        [SerializeField] private Grounded grounded;

        // --- NOUVEAU : Configuration PID ---
        [Header("PID Control")]
        // Ces valeurs sont des points de départ suggérés.
        [SerializeField] private float Kp = 8f;
        [SerializeField] private float Ki = 0.2f;
        [SerializeField] private float Kd = 1.5f;

        private PIDController pidX;
        private PIDController pidZ;
        protected override void StartAnyOwner()
        {
            _playerReferences = GetComponentInParent<PlayerReferences>();
            InitializePID();
        }

        private void InitializePID()
        {
            pidX = new PIDController(Kp, Ki, Kd);
            pidZ = new PIDController(Kp, Ki, Kd);
        }
        public float GetMaxSpeed()
        {
            float maxSpeed = StatManager.GetFloatStat(maxSpeedStat).Apply(maxWalkSpeed);
            if (run.Running) maxSpeed += maxRunSpeed - maxWalkSpeed;
            return maxSpeed;
        }

        // Retourne l'accélération MAXIMALE autorisée
        private float GetAcceleration()
        {
            float a = StatManager.GetFloatStat(accelerationStat).Apply(acceleration);
            if (!grounded.FullyGrounded()) a *= airAccelMultiplier;
            return a;
        }

        protected override void FixedUpdateAnyOwner()
        {
            if (DataManager.Instance != null && DataManager.Instance[NetworkManager.LocalClientId].outerData.playingState ==
                OuterData.PlayingState.SpectatingGame) return;
            
            if (grounded.IsGrounded && !grounded.GroundAngleValid()) Slide();
            else Move();
        }

        private void Move()
        {
            Vector3 dir = orientation.forward * InputManager.Move.y + orientation.right * InputManager.Move.x;
            MoveDir(dir);
        }

        // RÉÉCRITURE COMPLÈTE pour utiliser le PID et AddForce
        private void MoveDir(Vector3 dir)
        {
            float maxSpeed = GetMaxSpeed();
            float maxAccel = GetAcceleration();
            float deltaTime = Time.fixedDeltaTime;

            // 1. Transformation en Espace Local (Local Ground Space)
            // C'est crucial pour gérer correctement les pentes et comparer les vitesses.
            
            // Vitesse actuelle en local
            Vector3 currentLocalVelocity = grounded.WorldToLocalUp * rb.linearVelocity;

            // 2. Vitesse Locale Désirée (Setpoint)
            Vector3 desiredLocalVelocity = dir * maxSpeed;

            // 3. Calcul de l'Accélération Requise via PID (Axes X et Z uniquement)
            // Le PID calcule l'accélération nécessaire pour combler l'écart.
            // On ne contrôle pas l'axe Y (vertical), qui est géré par la lévitation et la gravité.
            float accelX = pidX.Calculate(desiredLocalVelocity.x, currentLocalVelocity.x, deltaTime);
            float accelZ = pidZ.Calculate(desiredLocalVelocity.z, currentLocalVelocity.z, deltaTime);

            Vector3 localAcceleration = new Vector3(accelX, 0, accelZ);

            // 4. Limitation de l'Accélération
            // Empêche le PID d'appliquer une force supérieure aux limites physiques du personnage.
            localAcceleration = Vector3.ClampMagnitude(localAcceleration, maxAccel);

            // 5. Application de la Force
            // Retour en espace mondial (World Space)
            Vector3 worldAcceleration = grounded.LocalUpToWorld * localAcceleration;

            // Utilisation de ForceMode.Acceleration.
            rb.AddForce(worldAcceleration, ForceMode.Force);

            Debug.DrawRay(transform.position, worldAcceleration, Color.red);
        }

        // Slide reste similaire mais bénéficie du nouveau MoveDir basé sur PID
        private void Slide()
        {
            Vector3 dir = orientation.forward * InputManager.Move.y + orientation.right * InputManager.Move.x;
            Vector3 groundNormalRight = Quaternion.Euler(0,90,0) * new Vector3(grounded.GroundHit.normal.x,0,grounded.GroundHit.normal.z).normalized;
            dir = Vector3.Project(dir, groundNormalRight);
            
            // Utilise le PID pour contrôler le mouvement latéral pendant la glissade
            MoveDir(dir);
            
            // Ajoute la force de glissade vers le bas de la pente
            Vector3 slideDir = Vector3.Cross(groundNormalRight, grounded.GroundHit.normal).normalized;
            Vector3 force = slideDir * slideForce;
            rb.AddForce(force, ForceMode.Acceleration);
            
            Debug.DrawRay(transform.position, groundNormalRight, Color.red);
            Debug.DrawRay(transform.position, slideDir * 5, Color.blue);
        }
    }
}