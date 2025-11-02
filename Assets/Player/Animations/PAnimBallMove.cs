using Unity.Netcode;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PAnimBallMove : PAnimBehaviour
    {
        [SerializeField] private PAnimManager animManager;
        private AnimComponent _animComponent = new() { Target = PAnimManager.Target.Ball };
        
        [SerializeField] private float ballRadius;
    
        private Vector3 _previousPosition;

        private void Start()
        {
            _previousPosition = transform.position;
        }

        private void Update()
        {
            Vector3 deltaPosition = transform.position - _previousPosition;
            _previousPosition = transform.position;
        
            float radX = -deltaPosition.x / ballRadius;
            float radZ = -deltaPosition.z / ballRadius;
        
            Quaternion rotX = Quaternion.AngleAxis(radZ * Mathf.Rad2Deg, Vector3.right);
            Quaternion rotZ = Quaternion.AngleAxis(radX * Mathf.Rad2Deg, Vector3.up);
            
            _animComponent.Rotation = rotZ * rotX * _animComponent.Rotation;
        }
        public override AnimComponent[] GetAnimComponents() => new[] {_animComponent};
    }
}