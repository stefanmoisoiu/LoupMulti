using Rendering.Fullscreen_Effects;
using Unity.Netcode;
using UnityEngine;

namespace Player.Hitbox
{
    public class HitboxTarget : MonoBehaviour
    {
        public enum HitboxType
        {
            None,
            Player,
            Resource,
        }
        [SerializeField] private HitboxType type = HitboxType.None;
        public HitboxType Type => type;

        [SerializeField] private RectOutline rectOutline;
        public RectOutline RectOutline => rectOutline;

        private NetworkObject obj;

        public bool IsMyPlayerObject()
        {
            if (obj == null && !TryGetComponent(out obj)) return false;
            return obj.IsPlayerObject && obj.IsOwner;
        }
    }
}