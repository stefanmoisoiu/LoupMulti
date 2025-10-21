namespace Base_Scripts
{
    using UnityEngine;

    public static class MeshFinder
    {
        public static bool TryGetPointOnCollider(this Collider collider, Vector3 worldDirection, out Vector3 hitPoint)
        {
            if (collider == null)
            {
                Debug.LogError("MeshCollider est nul ! Impossible de lancer le rayon.");
                hitPoint = Vector3.zero;
                return false;
            }

            Vector3 origin = collider.bounds.center - worldDirection * (collider.bounds.extents.magnitude * 1.1f);

            Ray ray = new Ray(origin, worldDirection);

            if (collider.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
            {
                hitPoint = hitInfo.point;
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
        }
    }
}