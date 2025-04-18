using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PCamEffects : PNetworkBehaviour
{
    public static List<Effect> Effects = new();

    [SerializeField] private Transform target;
    [SerializeField] private CinemachineCamera cam;
    private float _baseFov;

    protected override void StartAnyOwner()
    {
        Effects = new();
    }
    private void Start()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        _baseFov = cam.Lens.FieldOfView;
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
        cam.Lens.FieldOfView = _baseFov + effect.AddedFov;
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
