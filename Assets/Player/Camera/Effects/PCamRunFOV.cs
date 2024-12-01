using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCamRunFOV : MonoBehaviour
{
    [SerializeField] private float addedFov;
    [SerializeField] private float fovLerpSpeed;
    
    private PCamEffects.Effect _effect = new();
    private float _targetFov = 0;
    private float _fov = 0;

    [SerializeField] private PRun run;
    
    private void Start()
    {
        PCamEffects.Effects.Add(_effect);
        run.OnRun += () => _targetFov = addedFov;
        run.OnStopRun += () => _targetFov = 0;
    }

    private void Update()
    {
        _fov = Mathf.Lerp(_fov, _targetFov * InputManager.instance.MoveInput.magnitude, fovLerpSpeed * Time.deltaTime);
        _effect.AddedFov = _fov;
    }
}
