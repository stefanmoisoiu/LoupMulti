using Rendering.Fullscreen_Effects;
using UnityEngine;

namespace Player.Hitbox
{
    public class HighlightedHitboxTarget : HitboxTarget
    {
        [SerializeField] private RectOutline outline;
        public RectOutline Outline => outline;
    }
}