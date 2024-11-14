using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCameraHeadTurnTilt : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    
    [SerializeField] private float tiltLerpBackSpeed = 1;
    [SerializeField] private float minSpeedTiltFactor = 0.1f;
    [SerializeField] private float maxSpeedTiltFactor = 0.3f;
    [SerializeField] private float maxSpeedTiltSpeed = 20;
    
    private float _tilt = 0;
    private PCamEffects.Effect _effect = new();

    [SerializeField] private PCamera cam;
    
    private void Start()
    {
        PCamEffects.Effects.Add(_effect);
    }

    private void Update()
    {
        Tilt();
    }

    private void Tilt()
    {
        _tilt -= cam.LookDelta.x * Mathf.Lerp(minSpeedTiltFactor, maxSpeedTiltFactor, rb.velocity.magnitude / maxSpeedTiltSpeed);
        _tilt = Mathf.Lerp(_tilt, 0, tiltLerpBackSpeed * Time.deltaTime);
        
        // headTurn.localRotation = Quaternion.Euler(0, 0, _tilt);
        _effect.Tilt.z = _tilt;
    }
}
