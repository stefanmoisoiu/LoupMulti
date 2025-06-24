using UnityEngine;

namespace Base_Scripts
{
    public static class Spring
    {
        public static Vector3 CalculateSpringForce(Vector3 currentPos, Vector3 targetPos, Vector3 currentVel,
            float springConstant, float dampingFactor)
        {
            Vector3 springForce = (targetPos - currentPos) * springConstant;
            Vector3 dampingForce = -currentVel * dampingFactor;

            return springForce + dampingForce;
        }

        public static Vector2 CalculateSpringForce(Vector2 currentPos, Vector2 targetPos, Vector2 currentVel,
            float springConstant, float dampingFactor)
        {
            Vector2 springForce = (targetPos - currentPos) * springConstant;
            Vector2 dampingForce = -currentVel * dampingFactor;

            return springForce + dampingForce;
        }

        public static float CalculateSpringForce(float currentPos, float targetPos, float currentVel,
            float springConstant, float dampingFactor)
        {
            float springForce = (targetPos - currentPos) * springConstant;
            float dampingForce = -currentVel * dampingFactor;

            return springForce + dampingForce;
        }

        public static Vector3 CalculateSpringTorque(
            Quaternion currentRot,
            Quaternion targetRot,
            Vector3 currentAngularVelocity,
            float springConstant,
            float dampingFactor
        )
        {
            Quaternion qDelta = targetRot * Quaternion.Inverse(currentRot);
            qDelta.ToAngleAxis(out float angleDeg, out Vector3 axis);
            if (angleDeg > 180f) angleDeg -= 360f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector3 springTorque = axis.normalized * (angleRad * springConstant);
            Vector3 dampingTorque = -currentAngularVelocity * dampingFactor;
            return springTorque + dampingTorque;
        }

        /// <summary>
        /// Intègre la rotation par ressort + amortissement sur un transform (pas de Rigidbody).
        /// - currentRot      : rotation courante (sera mise à jour)
        /// - currentAngVel   : vitesse angulaire en rad/s (sera mise à jour)
        /// - targetRot       : rotation cible
        /// - springConstant  : k
        /// - dampingFactor   : d
        /// - deltaTime       : Time.deltaTime
        /// </summary>
        public static void IntegrateSpringRotation(
            ref Quaternion currentRot,
            ref Vector3 currentAngVel,
            Quaternion targetRot,
            float springConstant,
            float dampingFactor,
            float deltaTime
        )
        {
            // 1) Calcul du "couple" ressort+amortissement
            Vector3 torque = CalculateSpringTorque(
                currentRot,
                targetRot,
                currentAngVel,
                springConstant,
                dampingFactor
            );

            // 2) Mise à jour de la vitesse angulaire (on suppose inertie unitaire)
            currentAngVel += torque * deltaTime;

            // 3) Intégration de la rotation via axe-angle
            float angleRad = currentAngVel.magnitude * deltaTime;
            if (angleRad > Mathf.Epsilon)
            {
                float angleDeg = angleRad * Mathf.Rad2Deg;
                Vector3 axis = currentAngVel.normalized;
                Quaternion deltaRot = Quaternion.AngleAxis(angleDeg, axis);
                currentRot = deltaRot * currentRot;
            }
        }
    }
}