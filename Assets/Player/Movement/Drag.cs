using Player.Networking;
using UnityEngine;

namespace Player.Movement
{
    public class Drag : PNetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] [Range(0,1)] private float drag;
        [SerializeField] [Range(0,1)] private float airDrag = 0.05f;

        [SerializeField] private Grounded grounded;


        protected override void FixedUpdateAnyOwner()
        {
            ApplyDrag();
        }

        private void ApplyDrag()
        {
            Vector3 vel = rb.linearVelocity;
        
            if (grounded.FullyGrounded()) vel = grounded.WorldToLocalUp * vel;
            float d = 1 - (grounded.FullyGrounded() ? drag : airDrag);
            vel = new(vel.x * d, vel.y, vel.z * d);
        
            if (grounded.FullyGrounded()) vel = grounded.LocalUpToWorld * vel;
        
            rb.linearVelocity = vel;
        }
    }
}
