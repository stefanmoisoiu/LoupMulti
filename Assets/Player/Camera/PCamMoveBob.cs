using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCamMoveBob : MonoBehaviour
{
    [SerializeField] private AnimationCurve bobCurve;
    [SerializeField] private float amplitude;

    [SerializeField] private float walkBobSpeed;
    [SerializeField] private float runBobSpeed;

    [SerializeField] private float lerpBackSpeed;

    [SerializeField] private AnimationCurve hRunBobCurve;
    [SerializeField] private float hRunBobAmplitude;
    [SerializeField] private float hRunBobSpeed;
    
    private float _advancement;
    private float _hAdvancement;
    
    private PCamEffects.Effect _effect = new();

    [SerializeField] private PRun run;
    [SerializeField] private PGrounded grounded;

    private void Start()
    {
        PCamEffects.Effects.Add(_effect);
    }

    private void Update()
    {
        
        _effect.AddedPosition.y = Mathf.Lerp(_effect.AddedPosition.y, GetBob(), lerpBackSpeed * Time.deltaTime);
        _effect.Tilt.z = Mathf.Lerp(_effect.Tilt.z, GetHBob(), lerpBackSpeed * Time.deltaTime);
    }

    private float GetBob()
    {
        if (CanBob())
        {
            _advancement += Time.deltaTime * (run.Running ? runBobSpeed : walkBobSpeed) * InputManager.instance.MoveInput.magnitude;
            if (_advancement > 1) _advancement--;
            
            return bobCurve.Evaluate(_advancement) * amplitude;
        }
        else
        {
            _advancement = 0;
            return 0;
        }
    }

    private float GetHBob()
    {
        if (CanBob() && run.Running)
        {
            _hAdvancement += Time.deltaTime * hRunBobSpeed * InputManager.instance.MoveInput.magnitude;
            if (_hAdvancement > 1) _hAdvancement--;
            
            return hRunBobCurve.Evaluate(_hAdvancement) * hRunBobAmplitude;
        }
        else
        {
            _hAdvancement = 0;
            return 0;
        }
    }

    private bool CanBob() =>
        grounded.FullyGrounded() && InputManager.instance.MoveInput.magnitude > 0;
}
