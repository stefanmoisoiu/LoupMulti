using System.Collections;
using Networking;
using Player.Movement;
using Unity.Netcode;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class JumpTilt : NetworkBehaviour
    {
        [SerializeField] private float tiltAmount;
        [SerializeField] private AnimationCurve tiltCurve;
        [SerializeField] private float length;
    
        // [SerializeField] private Transform headJumpTilt;
    
        private float _tilt;
        private CamEffects.Effect _effect = new();
    
        private Coroutine _jumpTilt;
    
        [SerializeField] private Jump jump;

        private void Start()
        {
            CamEffects.Effects.Add(_effect);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
            jump.OnJump += StartJumpTilt;
        }

        private void OnEnable()
        {
            if (NetcodeManager.InGame) return;
            jump.OnJump += StartJumpTilt;
        }

        private void OnDisable()
        {
            jump.OnJump -= StartJumpTilt;
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
                // headJumpTilt.localRotation = Quaternion.Euler(_tilt, 0, 0);
                _effect.Tilt.x = _tilt;
                yield return null;
            }
            _tilt = 0;
            // headJumpTilt.localRotation = Quaternion.Euler(0, 0, 0);
            _effect.Tilt.x = 0;
        }
    }
}
