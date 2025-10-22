using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Abilities
{
    public class ShotgunAbility : Ability
    {
        [SerializeField] private int startBulletCount = 3;
        [SerializeField] private int addedBulletsPerRing = 3;
        [SerializeField] private int ringCount = 3;
        [SerializeField] private float shootMaxAngle = 45;
        [SerializeField] private bool debug;
        
        public override void TryUseAbility(out bool success)
        {
            base.TryUseAbility(out success);
            if (!success) return;
            
            Vector3[] directions = GetBulletDirections();
        }

        private void OnDrawGizmos()
        {
            if (!debug) return;
            Vector3[] directions = GetBulletDirections();
            
            foreach (Vector3 dir in directions)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, dir * 5f);
            }
        }
        
        
        private Vector3[] GetBulletDirections()
        {
            List<Vector3> directions = new List<Vector3>();
            for (int ring = 0; ring < ringCount; ring++)
            {
                int bulletCount = ring == 0 ? 1 : startBulletCount + addedBulletsPerRing * (ring - 1);
                float outwardAngle = ring == 0 ? 0 : (float)ring / (ringCount - 1) * shootMaxAngle;
                
                for (int i = 0; i < bulletCount; i++)
                {
                    float degrees = 360f * i / bulletCount;
                    Quaternion rotation = Quaternion.Euler(0, 0, degrees) * Quaternion.Euler(0, outwardAngle, 0);
                    Vector3 direction = rotation * transform.forward;
                    directions.Add(direction);
                }
            }
            
            return directions.ToArray();
        }
    }
}
