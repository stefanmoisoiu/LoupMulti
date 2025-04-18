using System;
using Unity.Netcode;
using UnityEngine;

public class PModelBallMove : NetworkBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private float ballRadius;
    
    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 deltaPosition = transform.position - previousPosition;
        previousPosition = transform.position;
        
        float radX = -deltaPosition.x / ballRadius;
        float radZ = deltaPosition.z / ballRadius;
        
        Quaternion rotX = Quaternion.AngleAxis(radZ * Mathf.Rad2Deg, Vector3.right);
        Quaternion rotZ = Quaternion.AngleAxis(radX * Mathf.Rad2Deg, Vector3.forward);
        
        ball.rotation = rotX * rotZ * ball.rotation;
    }
}