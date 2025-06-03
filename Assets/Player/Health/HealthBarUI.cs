using Game.Common;
using Game.Data.Extensions;
using Player.UI;
using Player.UI.CircularBar;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Health
{
    public class HealthBarUI : CircularBar
    {
        private const string HealthTag = "HealthBar";
        
        protected override void StartOnlineOwner()
        {
            Image img = PCanvas.CanvasObjects[HealthTag].GetComponent<Image>();
            target = new(img.material);
            img.material = target;
            PlayerHealth.OnHealthChangedOwner += OnHealthChanged;
        }
        protected override void DisableAnyOwner()
        {
            PlayerHealth.OnHealthChangedOwner -= OnHealthChanged;
        }

        private void OnHealthChanged(ushort previousHealth, ushort newHealth)
        {
            AnimateBar((float)previousHealth / GameSettings.PlayerMaxHealth, (float)newHealth / GameSettings.PlayerMaxHealth);
        }
    }
}