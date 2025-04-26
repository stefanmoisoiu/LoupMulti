using Input;
using Player.Movement;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class MoveBob : MonoBehaviour
    {
        [SerializeField] private AnimationCurve bobCurve;
        [SerializeField] private float amplitude;
        [SerializeField] [Range(0,1)] private float frequency = 0.25f;
    
        [SerializeField] private Movement.Movement movement;

        [SerializeField] private float lerpBackSpeed;

        [SerializeField] private AnimationCurve hRunBobCurve;
        [SerializeField] private float hRunBobAmplitude;
        [SerializeField] [Range(0,1)] private float hRunBobFrequency = 0.25f;
    
        private float _advancement;
        private float _hAdvancement;
    
        private CamEffects.Effect _effect = new();

        [SerializeField] private Run run;
        [SerializeField] private Grounded grounded;

        private void Start()
        {
            CamEffects.Effects.Add(_effect);
        }

        private void Update()
        {
        
            _effect.AddedPosition.y = Mathf.Lerp(_effect.AddedPosition.y, GetBob(), lerpBackSpeed * Time.deltaTime);
            _effect.Tilt.z = Mathf.Lerp(_effect.Tilt.z, GetHBob(), lerpBackSpeed * Time.deltaTime);
        }

        private float GetBob()
        {
            if (CanBob())
            {
                _advancement += Time.deltaTime * movement.GetMaxSpeed() * frequency * InputManager.Move.magnitude;
                if (_advancement > 1) _advancement--;
            
                return bobCurve.Evaluate(_advancement) * amplitude;
            }
            else
            {
                _advancement = 0;
                return 0;
            }
        }

        private float GetHBob()
        {
            if (CanBob() && movement.GetMaxSpeed() > movement.MaxRunSpeed - .5f)
            {
                _hAdvancement += Time.deltaTime * hRunBobFrequency * movement.GetMaxSpeed() * InputManager.Move.magnitude;
                if (_hAdvancement > 1) _hAdvancement--;
            
                return hRunBobCurve.Evaluate(_hAdvancement) * hRunBobAmplitude;
            }
            else
            {
                _hAdvancement = 0;
                return 0;
            }
        }

        private bool CanBob() =>
            grounded.FullyGrounded() && InputManager.Move.magnitude > 0;
    }
}
