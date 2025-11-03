using Base_Scripts;
using Game.Common;
using Game.Data;
using Player.Camera.Effects;
using Player.Networking;
using UnityEngine;

namespace Player.Health
{
    public class DamagedShake : PNetworkBehaviour
    {
        [SerializeField] private PlayerHealthComponent healthComponent;
        
        [SerializeField] private CamShake camShake;
        [SerializeField] private Shake.ShakeSettings shakeSettings;

        protected override void StartAnyOwner()
        {
            healthComponent.OnHealthChanged += OnHealthChanged;
        }

        protected override void DisableAnyOwner()
        {
            healthComponent.OnHealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(ushort previousHealth, ushort newHealth)
        {
            if (newHealth >= previousHealth) return;
            float healthLost = previousHealth - newHealth;

            ushort maxHealth = GameSettings.Instance.PlayerMaxHealth;
            if (DataManager.Instance != null &&
                DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData playerData))
                maxHealth = playerData.inGameData.maxHealth;
            
            float adv = healthLost / maxHealth;
            Shake.ShakeSettings settings = new Shake.ShakeSettings(
                shakeSettings.Duration, 
                shakeSettings.Amplitude,
                shakeSettings.Curve);
            camShake.AddShake(settings);
        }
    }
}