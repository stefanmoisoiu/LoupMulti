using UnityEngine;

namespace Base_Scripts
{
    public static class Spring
    {
        public static Vector3 CalculateSpringForce(Vector3 currentPos, Vector3 targetPos, Vector3 currentVel, float springConstant, float dampingFactor)
        {
            Vector3 springForce = (targetPos - currentPos) * springConstant;
            Vector3 dampingForce = -currentVel * dampingFactor;

            return springForce + dampingForce;
        }
        public static Vector2 CalculateSpringForce(Vector2 currentPos, Vector2 targetPos, Vector2 currentVel, float springConstant, float dampingFactor)
        {
            Vector2 springForce = (targetPos - currentPos) * springConstant;
            Vector2 dampingForce = -currentVel * dampingFactor;

            return springForce + dampingForce;
        }
        public static float CalculateSpringForce(float currentPos, float targetPos, float currentVel, float springConstant, float dampingFactor)
        {
            float springForce = (targetPos - currentPos) * springConstant;
            float dampingForce = -currentVel * dampingFactor;

            return springForce + dampingForce;
        }
    }
}
