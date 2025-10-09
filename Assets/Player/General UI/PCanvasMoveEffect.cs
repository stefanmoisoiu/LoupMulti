using System.Collections.Generic;
using Player.Camera;
using Player.General_UI;
using Player.Networking;
using UnityEngine;

public class PCanvasMoveEffect : PNetworkBehaviour
{
    private static readonly List<MoveEffectInstance> ActiveMoveEffects = new();
    
    [SerializeField] private float movePerDelta = 1;
    [SerializeField] private float maxDelta = 5;
    
    [SerializeField] private float lerpSpeed = 20;
    private Vector2 _currentMove;

    [SerializeField] private PCamera cam;
    
    protected override void UpdateAnyOwner()
    {
        UpdateMoveEffect();
        ApplyMoveEffect();
    }

    private void UpdateMoveEffect()
    {
        float canvasSizeRatio = ((RectTransform)PCanvas.Canvas.transform).sizeDelta.x / 1920f;

        Vector2 delta = new(
            Mathf.Clamp(cam.LookDelta.x, -maxDelta, maxDelta),
            Mathf.Clamp(cam.LookDelta.y, -maxDelta, maxDelta));
        
        Vector2 target = delta * (movePerDelta * canvasSizeRatio);
        
        _currentMove = Vector2.Lerp(_currentMove, target, lerpSpeed * canvasSizeRatio * Time.deltaTime);
    }

    private void ApplyMoveEffect()
    {
        foreach (var effect in ActiveMoveEffects)
        {
            effect.Apply(_currentMove);
        }
    }
    
    public static void AddMoveEffect(RectTransform target, MoveEffectSettings settings)
    {
        ActiveMoveEffects.Add(new (target, settings));
    }
    
    public static void RemoveMoveEffect(RectTransform target)
    {
        for (int i = 0; i < ActiveMoveEffects.Count; i++)
        {
            if (ActiveMoveEffects[i].Target == target)
            {
                ActiveMoveEffects.RemoveAt(i);
                return;
            }
        }
    }
    
    private class MoveEffectInstance
    {
        public MoveEffectSettings Settings;
        public RectTransform Target;
        public Vector2 StartOffset = Vector2.zero;
        
        public MoveEffectInstance(RectTransform target, MoveEffectSettings settings)
        {
            Settings = settings;
            Target = target;
            StartOffset = target.localPosition;
        }
        
        public void Apply(Vector2 offset)
        {
            if (Target == null) return;
            Target.localPosition = StartOffset + offset * Settings.MoveMult;
        }
    }
    
    [System.Serializable]
    public struct MoveEffectSettings
    {
        public float MoveMult;
        
        MoveEffectSettings(float moveMult = 1)
        {
            MoveMult = moveMult;
        }
    }
}
