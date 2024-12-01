using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PCameraJumpTilt : NetworkBehaviour
{
    [SerializeField] private float tiltAmount;
    [SerializeField] private AnimationCurve tiltCurve;
    [SerializeField] private float length;
    
    // [SerializeField] private Transform headJumpTilt;
    
    private float _tilt;
    private PCamEffects.Effect _effect = new();
    
    private Coroutine _jumpTilt;
    
    [SerializeField] private PJump jump;

    private void Start()
    {
        PCamEffects.Effects.Add(_effect);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        jump.OnJump += StartJumpTilt;
    }

    private void OnEnable()
    {
        if (NetcodeManager.InGame) return;
        jump.OnJump += StartJumpTilt;
    }

    private void OnDisable()
    {
        jump.OnJump -= StartJumpTilt;
    }

    private void StartJumpTilt()
    {
        if (_jumpTilt != null) StopCoroutine(_jumpTilt);
        _jumpTilt = StartCoroutine(JumpTilt());
    }
    private IEnumerator JumpTilt()
    {
        float t = 0;
        while (t < length)
        {
            t += Time.deltaTime;
            float p = t / length;
            _tilt = tiltAmount * tiltCurve.Evaluate(p);
            // headJumpTilt.localRotation = Quaternion.Euler(_tilt, 0, 0);
            _effect.Tilt.x = _tilt;
            yield return null;
        }
        _tilt = 0;
        // headJumpTilt.localRotation = Quaternion.Euler(0, 0, 0);
        _effect.Tilt.x = 0;
    }
}
