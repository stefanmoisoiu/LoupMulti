using Rendering.Fullscreen_Effects;
using UnityEngine;

namespace Player.Target
{
    public class HighlightedTargetable : Targetable
    {
        [SerializeField] private RectOutline outline;
        public RectOutline Outline => outline;
    }
}