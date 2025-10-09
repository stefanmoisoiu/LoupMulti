using Game.Common.CircularBar;
using Player.General_UI;
using Player.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Movement.Stamina
{
    public class StaminaBarUI : PNetworkBehaviour
    {
        private const string StaminaTag = "StaminaBar";
        
        [SerializeField] private Stamina stamina;
        private CircularBar _staminaBar;

        protected override void StartAnyOwner()
        {
            _staminaBar = PCanvas.CanvasObjects[StaminaTag].GetComponent<CircularBar>();
        }

        protected override void UpdateAnyOwner()
        {
            _staminaBar.SetAdv(stamina.StaminaValue / stamina.MaxStamina);
        }
    }
}