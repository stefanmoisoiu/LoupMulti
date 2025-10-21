using Unity.Netcode;
using UnityEngine;

namespace Player.Hitbox
{
    public class HitboxTarget : MonoBehaviour
    {
        [SerializeField] private bool hitboxEnabled = true;
        public bool HitboxEnabled => hitboxEnabled;

        [SerializeField] private Collider collider;
        public Collider Collider => collider;
        
        
        public void SetHitboxEnabled(bool hEnabled) => hitboxEnabled = hEnabled;
        public enum HitboxType
        {
            None,
            Player,
            Resource,
        }
        [SerializeField] private HitboxType type = HitboxType.None;
        public HitboxType Type => type;
        private NetworkObject obj;

        public bool IsMyPlayerObject()
        {
            if (obj == null && !TryGetComponent(out obj)) return false;
            return obj.IsPlayerObject && obj.IsOwner;
        }
    }
}