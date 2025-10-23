using UnityEngine;

namespace Player.Abilities
{
    public class ShotgunVFX : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;

        public void SetBullets(Vector3 startPoint, Vector3[] endPoints)
        {
            lineRenderer.positionCount = endPoints.Length * 2 - 1;

            for (int i = 0; i < endPoints.Length; i ++)
            {
                if (i % 2 == 0) lineRenderer.SetPosition(i + 1, endPoints[i]);
                else lineRenderer.SetPosition(i, startPoint);
            }
        }
    }
}