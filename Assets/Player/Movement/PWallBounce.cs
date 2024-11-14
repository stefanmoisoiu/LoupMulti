using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PWallBounce : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float verticalBounceSpeed;
    [SerializeField] private float horizontalBounceSpeed;

    [SerializeField] private float _type;
    
    private void Update()
    {
        if (InputManager.instance.Jumping) TryWallBounce();
    }

    private void TryWallBounce()
    {
        
    }
}
