using Base_Scripts;
using Game.Common;
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
        [SerializeField] private float maxAddedDuration = 0.5f;
        [SerializeField] private float maxAddedAmplitude = 0.5f;

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
            float adv = healthLost / GameSettings.Instance.PlayerMaxHealth;
            Shake.ShakeSettings settings = new Shake.ShakeSettings(
                shakeSettings.Duration + adv * maxAddedDuration, 
                shakeSettings.Amplitude + adv * maxAddedAmplitude,
                shakeSettings.Curve);
            camShake.AddShake(settings);
        }
    }
}