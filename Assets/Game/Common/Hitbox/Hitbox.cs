using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Hitbox
{
    public class Hitbox : MonoBehaviour
    {
        public enum Shape
        {
            Box,
            Sphere
        }

        [SerializeField] [Tooltip("Optional")] private Transform offsetOrigin;

        [SerializeField] private bool excludeOwnedHitboxes = false;
        [SerializeField] private HitboxTarget.HitboxType[] hitboxTargets;
        [SerializeField] private Shape shape = Shape.Box;
        [SerializeField] private Vector3 offset = Vector3.zero;
        
        [ShowIf("@shape == Shape.Box")] [SerializeField] private Vector3 boxSize = Vector3.one;
        [ShowIf("@shape == Shape.Sphere")] [SerializeField] private float sphereDiameter = 1f;

        [SerializeField] private bool debug = true;

        Collider[] cachedColliders = new Collider[8];

        private Transform GetOffsetParent() => offsetOrigin != null ? offsetOrigin : transform;

        public HitboxTarget CalculateClosestHitbox()
        {
            Vector3 position = GetOffsetParent().TransformPoint(offset);
            return GetClosestTarget(position, CalculateHitboxes());
        }
        public HitboxTarget[] CalculateHitboxes()
        {
            Vector3 position = GetOffsetParent().TransformPoint(offset);
            Vector3 size = shape == Shape.Box ? boxSize : new Vector3(sphereDiameter, sphereDiameter, sphereDiameter);
            Collider[] results = GetAllCollidersInArea(position, size, shape);
            List<HitboxTarget> targets = GetAllTargets(results);
            return targets?.ToArray();
        }
        
        private HitboxTarget GetClosestTarget(Vector3 position, HitboxTarget[] targets)
        {
            if (targets == null || targets.Length == 0) return null;
            
            HitboxTarget closestTarget = targets[0];
            float closestDistance = Vector3.Distance(position, closestTarget.transform.position);
            for (int i = 1; i < targets.Length; i++)
            {
                float distance = Vector3.Distance(position, targets[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = targets[i];
                }
            }
            return closestTarget;
        }
        private List<HitboxTarget> GetAllTargets(Collider[] results)
        {
            if (results == null) return null;
            
            List<HitboxTarget> targets = new List<HitboxTarget>();
            foreach (var col in results)
            {
                if (col == null) continue;
                if (!col.TryGetComponent(out HitboxTarget target)) continue;
                if (!target.enabled) continue;
                if (!target.HitboxEnabled) continue;
                if (!hitboxTargets.Contains(target.Type)) continue;
                if (excludeOwnedHitboxes && target.IsMyPlayerObject()) continue;
                targets.Add(target);
            }
            
            return targets.Count == 0 ? null : targets;
        }
        private Collider[] GetAllCollidersInArea(Vector3 position, Vector3 size, Shape type)
        {
            for (int i = 0; i < cachedColliders.Length; i++)  cachedColliders[i] = null; // Clear the array
            int colliderCount = 0;
            switch (type)
            {
                case Shape.Box:
                    colliderCount = Physics.OverlapBoxNonAlloc(position, size, cachedColliders);
                    break;
                case Shape.Sphere:
                    colliderCount = Physics.OverlapSphereNonAlloc(position, size.x / 2f, cachedColliders);
                    break;
            }
            
            return colliderCount == 0 ? null : cachedColliders;
        }
        
        private void OnDrawGizmos()
        {
            if (!debug) return;
            
            Gizmos.color = Color.red;
            Vector3 position = GetOffsetParent().TransformPoint(offset);
            if (shape == Shape.Box)
            {
                Gizmos.DrawWireCube(position, boxSize);
            }
            else if (shape == Shape.Sphere)
            {
                Gizmos.DrawWireSphere(position, sphereDiameter / 2f);
            }
        }
    }
}