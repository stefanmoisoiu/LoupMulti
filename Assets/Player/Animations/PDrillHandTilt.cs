using System;
using System.Collections;
using Base_Scripts;
using Networking;
using Networking.Connection;
using Player.Camera;
using Player.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PDrillHandTilt : PNetworkBehaviour
    {
        [SerializeField] private Transform drillHand;
        [SerializeField] private float rotationSpeed = 360;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private bool direction;

        private float _currentRotationSpeedMult = 0;
        private float _currentRotation = 0;
        

        private bool _rotating = false;
        public bool Rotating => _rotating;

        private Coroutine _anim;

        private void Update()
        {
            _currentRotation += rotationSpeed * Time.deltaTime * _currentRotationSpeedMult;
            float rot = (direction ? 1 : -1) * _currentRotation;
            drillHand.localRotation = Quaternion.Euler(0, 0, rot);
        }

        public void SetRotatingOwner(bool rotating)
        {
            if (rotating == _rotating) return;
            SetRotating(true);
            SetRotatingServerRpc(rotating);
        }
        [ServerRpc]
        private void SetRotatingServerRpc(bool rotating) => SetRotatingClientRpc(rotating);
        [ClientRpc]
        private void SetRotatingClientRpc(bool rotating) => SetRotating(rotating);

        private void SetRotating(bool rotating)
        {
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(Animate(rotating));
            _rotating = rotating;
        }

        public IEnumerator Animate(bool startRotating)
        {
            float start = startRotating ? 0 : 1;
            float end = startRotating ? 1 : 0;
            
            float t = 0;
            
            _currentRotationSpeedMult = start;
            
            while (t < 1)
            {
                t += Time.deltaTime;
                _currentRotationSpeedMult = Mathf.Lerp(start, end, curve.Evaluate(t));
            }
            
            _currentRotationSpeedMult = end;

            yield break;
        }
    }
}
