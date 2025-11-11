using Player.Movement;
using Player.Networking;
using UnityEngine;

namespace Player.General_UI
{
    public class PDebugInfo : PNetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Grounded grounded;
        [SerializeField] private Movement.Movement movement;
        
#if UNITY_EDITOR
        private GUIStyle _style;
        private Vector3 _worldVel;

        protected override void StartAnyOwner()
        {
            _style = new GUIStyle();
            _style.fontSize = 10;
            _style.fontStyle = FontStyle.Bold;
            _style.normal.textColor = Color.white;
        }

        protected override void FixedUpdateAnyOwner()
        {
            _worldVel = rb.linearVelocity;
        }

        private void OnGUI()
        {
            if (IsOnline && !IsOwner) return;

            Vector2 textPos = new(5, 5);
            float padding = 2;
            Vector2 textSize = new (100, 10);
            void NextLine() => textPos.y += textSize.y + padding;
            
            void DrawText(string text, Color color = default)
            {
                if (color == default) color = Color.white;
                
                Rect rect = new (textPos, textSize);
                Rect shadowRect = new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height);
                GUI.color = Color.black;
                GUI.Label(shadowRect, text, _style);
        
                GUI.color = color;
                GUI.Label(rect, text, _style);
            }
            
            // Velocity
            if (grounded.FullyGrounded())
            {
                Vector3 localVel = grounded.WorldToLocalUp * _worldVel;
                string localVelText = $"Local Vel: {localVel:F1} ({localVel.magnitude:F1}m/s)";
                DrawText(localVelText);
            }
            else
            {
                string worldVelText = $"World Vel: {_worldVel:F1} ({_worldVel.magnitude:F1}m/s)";
                DrawText(worldVelText);
            }
            
            NextLine();
            
            // Movement Info
            if (grounded.FullyGrounded())
            {
                Vector3 localVel = grounded.WorldToLocalUp * _worldVel;
                localVel.y = 0;
                string localMoveInfo = $"Movement Speed: {localVel.magnitude:F1} / {movement.GetMaxSpeed():F1}";
                DrawText(localMoveInfo);
            }
            else
            {
                Vector3 worldVel = _worldVel;
                worldVel.y = 0;
                string worldMoveInfo = $"Movement Speed: {worldVel.magnitude:F1} / {movement.GetMaxSpeed():F1}";
                DrawText(worldMoveInfo);
            }

            NextLine();
            
            // Grounded
            string groundedText = $"Fully Grounded: {grounded.FullyGrounded()}, Grounded: {grounded.IsGrounded}";
            DrawText(groundedText);
        }
#endif
    }
}
