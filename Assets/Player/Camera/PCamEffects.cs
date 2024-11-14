using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PCamEffects : NetworkBehaviour
{
    public static List<Effect> Effects = new();

    [SerializeField] private Transform target;
    [SerializeField] private CinemachineVirtualCamera cam;
    private float _baseFov;

    private void Start()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        _baseFov = cam.m_Lens.FieldOfView;

    }

    private void LateUpdate()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        ApplyEffect(GetEffectTotal());
    }
    private Effect GetEffectTotal()
    {
        Effect finalEffect = new Effect();
        
        foreach (var effect in Effects)
        {
            finalEffect.Tilt += effect.Tilt;
            finalEffect.AddedPosition += effect.AddedPosition;
            finalEffect.AddedFov += effect.AddedFov;
        }

        return finalEffect;
    }
    
    private void ApplyEffect(Effect effect)
    {
        cam.m_Lens.FieldOfView = _baseFov + effect.AddedFov;
        target.localPosition = effect.AddedPosition;
        target.localRotation = Quaternion.Euler(effect.Tilt);
    }

    [Serializable]
    public class Effect
    {
        public Vector3 Tilt;
        
        [Space] public Vector3 AddedPosition;
        
        [Space] public float AddedFov;
    }
}
