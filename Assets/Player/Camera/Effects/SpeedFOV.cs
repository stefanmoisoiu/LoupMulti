using Player.Networking;
using Unity.Cinemachine;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class SpeedFOV : PNetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
    
        [SerializeField] private float maxAddedFOV = 20;
        [SerializeField] private float maxFOVSpeed = 20;

        [SerializeField] private float fovLerpSpeed = 10;
        private float _addedFOV;
        
        private CamEffects.Effect _effect = new();

        protected override void StartAnyOwner()
        {
            CamEffects.Effects.Add(_effect);
        }

        protected override void DisableAnyOwner()
        {
            CamEffects.Effects.Remove(_effect);
        }

        protected override void UpdateAnyOwner()
        {
            _addedFOV = Mathf.Lerp(_addedFOV, Mathf.Clamp01(rb.linearVelocity.magnitude / maxFOVSpeed) * maxAddedFOV, Time.deltaTime * fovLerpSpeed);
            _effect.AddedFov = _addedFOV;
        }
    }
}
