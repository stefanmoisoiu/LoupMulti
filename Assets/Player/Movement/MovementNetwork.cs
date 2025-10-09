using Player.Networking;
using UnityEngine;

namespace Player.Movement
{
    public class MovementNetwork : PNetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
        
        protected override void StartOnlineNotOwner()
        {
            rb.useGravity = false;
        }
    }
}