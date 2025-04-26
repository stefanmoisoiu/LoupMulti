using Unity.Cinemachine;
using UnityEngine;

namespace Player.Camera.Effects
{
    public class SpeedFOV : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private CinemachineCamera cam;
    
    
        [SerializeField] private float maxAddedFOV = 20;
        [SerializeField] private float maxFOVSpeed = 20;
        private float _startFOV;

        private void Start()
        {
            _startFOV = cam.Lens.FieldOfView;
        }

        private void Update()
        {
            cam.Lens.FieldOfView = Mathf.Lerp(_startFOV, _startFOV + maxAddedFOV, rb.linearVelocity.magnitude / maxFOVSpeed);
        }
    }
}
