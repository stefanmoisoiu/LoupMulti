using Player.UI;
using Player.UI.CircularBar;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Movement.Stamina
{
    public class StaminaBarUI : CircularBar
    {
        private const string StaminaTag = "StaminaBar";
        
        [SerializeField] private Stamina stamina;

        protected override void StartAnyOwner()
        {
            Image img = PCanvas.CanvasObjects[StaminaTag].GetComponent<Image>();
            target = new(img.material);
            img.material = target;
        }

        protected override void UpdateAnyOwner()
        {
            SetBarAdv(stamina.StaminaValue / stamina.MaxStamina);
        }
    }
}