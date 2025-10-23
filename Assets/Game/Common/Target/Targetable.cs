using Unity.Netcode;
using UnityEngine;

namespace Player.Target
{
    [RequireComponent(typeof(Collider))]
    public class Targetable : MonoBehaviour
    {
        [SerializeField] private bool targetEnabled = true;
        public bool TargetEnabled => targetEnabled;

        private Collider _collider;
        public Collider Collider => _collider;
        
        private void Awake()
        {
            if (_collider == null) _collider = GetComponent<Collider>();
        }
        
        public void SetHitboxEnabled(bool hEnabled) => targetEnabled = hEnabled;
        public enum TargetType
        {
            None,
            Player,
            Resource,
        }
        [SerializeField] private TargetType type = TargetType.None;
        public TargetType Type => type;
        private NetworkObject obj;

        public bool IsMyPlayerObject()
        {
            if (obj == null && !TryGetComponent(out obj)) return false;
            return obj.IsPlayerObject && obj.IsOwner;
        }
    }
}