using Base_Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lobby.Spaceship.Button
{
    public class ButtonPressAnim : MonoBehaviour
    {
        [SerializeField] private Transform buttonTop;
    
        [SerializeField] private float springConstant = 0.5f;
        [SerializeField] private float springDamping = 0.1f;
        private float springVelocity = 0f;
        private float springPosition = 0f;
        private bool isPressed = false;

        [SerializeField] private Vector3 pressedScale = new Vector3(1.1f, 0.9f, 1.1f);
        [SerializeField] private float pressedHeight = -0.1f;
    
        [Button]

        public void SetPressed(bool pressed)
        {
            isPressed = pressed;
        }

        private void Update()
        {
            UpdateSpring();
            UpdateModel();
        }

        private void UpdateSpring()
        {
            float force = Spring.CalculateSpringForce(springPosition,isPressed ? 1 : 0,springVelocity,springConstant,springDamping);
            springVelocity += force * Time.fixedDeltaTime;
            springPosition += springVelocity * Time.fixedDeltaTime;
        }
    
        private void UpdateModel()
        {
            Vector3 newScale = Vector3.one + (pressedScale - Vector3.one) * springPosition;
            buttonTop.localScale = newScale;
            Vector3 newPosition = new Vector3(0, pressedHeight, 0) * springPosition;
            buttonTop.localPosition = newPosition;
        }
    }
}
