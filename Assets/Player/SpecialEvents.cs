using System;
using Player.Networking;
using UnityEngine;

namespace Player
{
    public class SpecialEvents : PNetworkBehaviour
    {
        private float distanceMoved;
        private Vector3 previousPosition;
        
        public static event Action<float> OnMove;
        
        protected override void StartOnlineOwner()
        {
            previousPosition = transform.position;
        }

        protected override void UpdateOnlineOwner()
        {
            UpdateMoved();
        }

        private void UpdateMoved()
        {
            Vector3 delta = transform.position - previousPosition;
            distanceMoved += delta.magnitude;
            previousPosition = transform.position;
            if (delta.magnitude > 0.2f)
            {
                OnMove?.Invoke(distanceMoved);
            }
        }
    }
}