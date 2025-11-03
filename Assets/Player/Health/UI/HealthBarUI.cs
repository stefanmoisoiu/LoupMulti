using Game.Common;
using Game.Common.CircularBar;
using Game.Data;
using Game.Data.Extensions;
using Player.General_UI;
using Player.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Health
{
    public class HealthBarUI : PNetworkBehaviour
    {
        private const string HealthTag = "HealthBar";
        private CircularBar _healthBar;
        protected override void StartOnlineOwner()
        {
            _healthBar = PCanvas.CanvasObjects[HealthTag].GetComponent<CircularBar>();
            PlayerHealthHelper.OnPlayerHealthChangedOwner += OnPlayerHealthChangedHealthChanged;
        }
        protected override void DisableAnyOwner()
        {
            PlayerHealthHelper.OnPlayerHealthChangedOwner -= OnPlayerHealthChangedHealthChanged;
        }

        private void OnPlayerHealthChangedHealthChanged(ushort previousHealth, ushort newHealth)
        {
            ushort maxHealth = GameSettings.Instance.PlayerMaxHealth;
            if (DataManager.Instance != null &&
                DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData playerData))
                maxHealth = playerData.inGameData.maxHealth;

            float newAdv = (float)newHealth / maxHealth;
            _healthBar.SetAdv(newAdv);
        }
    }
}