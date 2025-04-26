using Base_Scripts;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PModelSpeedTilt : PModelProceduralAnim
    {
        private Vector3 previousPosition;
        [SerializeField] private float tiltMultiplier = 10f;
    
        [SerializeField] private float springConstant = 0.1f;
        [SerializeField] private float dampingFactor = 0.1f;
    
        private Vector2 currentTurn;
        private Vector2 currentTurnVelocity;
    
        private void CalculateTilt()
        {
            Vector3 deltaPosition = transform.position - previousPosition;
            previousPosition = transform.position;
        
            Vector2 targetTurn = new Vector2(deltaPosition.x, deltaPosition.z);
            Vector2 springForce = Spring.CalculateSpringForce(currentTurn, targetTurn, currentTurnVelocity, springConstant, dampingFactor);
            currentTurnVelocity += springForce * Time.deltaTime;
            currentTurn += currentTurnVelocity * Time.deltaTime;
        }

        private void Update()
        {
            CalculateTilt();
        }

        public override Data GetData()
        {
            Quaternion bodyRotation = Quaternion.Euler(-currentTurn.y*tiltMultiplier,0,currentTurn.x*tiltMultiplier);

            return new Data
            {
                bodyRotation = bodyRotation,
                bodyScale = Vector3.one,
                headScale = Vector3.one
            };
        }
    }
}
