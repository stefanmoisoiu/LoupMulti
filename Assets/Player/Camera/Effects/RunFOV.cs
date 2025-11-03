using Input;
using Player.Movement;
using Player.Networking;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class RunFOV : PNetworkBehaviour
    {
        [SerializeField] private float addedFov;
        [SerializeField] private float fovLerpSpeed;
    
        private CamEffects.Effect _effect = new();
        private float _targetFov = 0;
        private float _fov = 0;

        [SerializeField] private Run run;

        protected override void StartAnyOwner()
        {
            CamEffects.Effects.Add(_effect);
            run.OnRun += SetRunFOV;
            run.OnStopRun += StopRunFOV;
        }
        private void SetRunFOV() => _targetFov = addedFov;
        private void StopRunFOV() => _targetFov = 0;
        
        protected override void DisableAnyOwner()
        {
            CamEffects.Effects.Remove(_effect);
            run.OnRun -= SetRunFOV;
            run.OnStopRun -= StopRunFOV;
        }

        private void Update()
        {
            _fov = Mathf.Lerp(_fov, _targetFov * InputManager.Move.magnitude, fovLerpSpeed * Time.deltaTime);
            _effect.AddedFov = _fov;
        }
    }
}
