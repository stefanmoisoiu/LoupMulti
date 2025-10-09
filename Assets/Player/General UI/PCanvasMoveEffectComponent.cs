using System;
using UnityEngine;

namespace Player.General_UI
{
    public class PCanvasMoveEffectComponent : MonoBehaviour
    {
        [SerializeField] private PCanvasMoveEffect.MoveEffectSettings settings;

        private void OnEnable()
        {
            PCanvasMoveEffect.AddMoveEffect((RectTransform)transform, settings);
        }
        
        private void OnDisable()
        {
            PCanvasMoveEffect.RemoveMoveEffect((RectTransform)transform);
        }
    }
}