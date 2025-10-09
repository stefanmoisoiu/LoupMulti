using Rendering.Fullscreen_Effects;
using UnityEngine;

namespace Player.Hitbox
{
    public class HighlightedHitbox : HitboxTarget
    {
        [SerializeField] private RectOutline outline;
        public RectOutline Outline => outline;
    }
}