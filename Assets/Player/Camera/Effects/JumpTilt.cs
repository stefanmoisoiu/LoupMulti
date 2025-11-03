using System.Collections;
using Networking;
using Networking.Connection;
using Player.Movement;
using Player.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class JumpTilt : PNetworkBehaviour
    {
        [SerializeField] private float tiltAmount;
        [SerializeField] private AnimationCurve tiltCurve;
        [SerializeField] private float length;
    
        // [SerializeField] private Transform headJumpTilt;
    
        private float _tilt;
        private CamEffects.Effect _effect = new();
    
        private Coroutine _jumpTilt;
    
        [SerializeField] private Jump jump;

        protected override void StartAnyOwner()
        {
            jump.OnJump += StartJumpTilt;
            CamEffects.Effects.Add(_effect);
        }

        protected override void DisableAnyOwner()
        {
            jump.OnJump -= StartJumpTilt;
            CamEffects.Effects.Remove(_effect);
        }
        private void StartJumpTilt()
        {
            if (_jumpTilt != null) StopCoroutine(_jumpTilt);
            _jumpTilt = StartCoroutine(Apply());
        }
        private IEnumerator Apply()
        {
            float t = 0;
            while (t < length)
            {
                t += Time.deltaTime;
                float p = t / length;
                _tilt = tiltAmount * tiltCurve.Evaluate(p);
                _effect.Tilt.x = _tilt;
                yield return null;
            }
            _tilt = 0;
            _effect.Tilt.x = 0;
        }
    }
}
