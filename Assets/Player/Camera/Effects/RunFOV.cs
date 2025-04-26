using Input;
using Player.Movement;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class RunFOV : MonoBehaviour
    {
        [SerializeField] private float addedFov;
        [SerializeField] private float fovLerpSpeed;
    
        private CamEffects.Effect _effect = new();
        private float _targetFov = 0;
        private float _fov = 0;

        [SerializeField] private Run run;
    
        private void Start()
        {
            CamEffects.Effects.Add(_effect);
            run.OnRun += () => _targetFov = addedFov;
            run.OnStopRun += () => _targetFov = 0;
        }

        private void Update()
        {
            _fov = Mathf.Lerp(_fov, _targetFov * InputManager.Move.magnitude, fovLerpSpeed * Time.deltaTime);
            _effect.AddedFov = _fov;
        }
    }
}
