using System.Collections.Generic;
using System.Linq;
using Player.Target;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common.Hitbox
{
    public class TargetDetector : MonoBehaviour
    {
        public enum Shape
        {
            Box,
            Sphere
        }

        [SerializeField] [Tooltip("Optional")] private Transform offsetOrigin;

        [SerializeField] private bool excludeOwnedTargets = false;
        [SerializeField] private Targetable.TargetType[] targetTypes;
        [SerializeField] private Shape shape = Shape.Box;
        [SerializeField] private Vector3 offset = Vector3.zero;
        
        [ShowIf("@shape == Shape.Box")] [SerializeField] private Vector3 boxSize = Vector3.one;
        [ShowIf("@shape == Shape.Sphere")] [SerializeField] private float sphereDiameter = 1f;

        [SerializeField] private bool debug = true;

        Collider[] cachedColliders = new Collider[8];

        private Transform GetOffsetParent() => offsetOrigin != null ? offsetOrigin : transform;

        public Targetable[] CalculateAndGetTargets(out Targetable closestTarget)
        {
            closestTarget = null;
            Vector3 position = GetOffsetParent().TransformPoint(offset);
            Vector3 size = shape == Shape.Box ? boxSize : new Vector3(sphereDiameter, sphereDiameter, sphereDiameter);
            
            for (int i = 0; i < cachedColliders.Length; i++) cachedColliders[i] = null;
            
            int count = GetAllCollidersInArea(position, size, shape);
            if (count == 0) return System.Array.Empty<Targetable>();
            
            return GetAllTargetsAndClosest(cachedColliders, count, position, out closestTarget);
        }

        public Targetable CalculateClosestTarget()
        {
            CalculateAndGetTargets(out Targetable closest);
            return closest;
        }
        
        public Targetable[] CalculateTargets()
        {
            return CalculateAndGetTargets(out _);
        }

        private int GetAllCollidersInArea(Vector3 position, Vector3 size, Shape type)
        {
            switch (type)
            {
                case Shape.Box:
                    return Physics.OverlapBoxNonAlloc(position, size, cachedColliders);
                case Shape.Sphere:
                    return Physics.OverlapSphereNonAlloc(position, size.x / 2f, cachedColliders);
            }
            return 0;
        }
        
        private Targetable[] GetAllTargetsAndClosest(Collider[] results, int count, Vector3 position, out Targetable closestTarget)
        {
            List<Targetable> targets = new List<Targetable>();
            closestTarget = null;
            float closestDistanceSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!results[i].TryGetComponent(out Targetable target)) continue;
                if (!target.enabled) continue;
                if (!target.TargetEnabled) continue;
                if (!targetTypes.Contains(target.Type)) continue;
                if (excludeOwnedTargets && target.IsMyPlayerObject()) continue;
                
                targets.Add(target);
                
                float distanceSqr = (position - target.transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestTarget = target;
                }
            }
            
            return targets.ToArray();
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