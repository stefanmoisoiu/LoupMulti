using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PCameraSpeedFOV : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CinemachineVirtualCamera cam;
    
    
    [SerializeField] private float maxAddedFOV = 20;
    [SerializeField] private float maxFOVSpeed = 20;
    private float _startFOV;

    private void Start()
    {
        _startFOV = cam.m_Lens.FieldOfView;
    }

    private void Update()
    {
        cam.m_Lens.FieldOfView = Mathf.Lerp(_startFOV, _startFOV + maxAddedFOV, rb.velocity.magnitude / maxFOVSpeed);
    }
}
