using Base_Scripts;
using Game.Data.Extensions;
using Networking.Spawning;
using Player.Networking;
using UnityEngine;

namespace Player.Health
{
    public class DamageParticles : PNetworkBehaviour
    {
        [SerializeField] private NetworkPooledParticles pooledParticles;
        [SerializeField] private float particlesPerDamagePoint = .25f;
        

        protected override void StartOnlineOwner()
        {
            PlayerDataHealth.OnOwnerPlayerHealthChanged += OnHealthChanged;
        }
        protected override void DisableAnyOwner()
        {
            PlayerDataHealth.OnOwnerPlayerHealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(ushort previousHealth, ushort newHealth)
        {
            if (newHealth > previousHealth) return;
            float damage = (float)previousHealth - newHealth;
            if (damage > 0)
                pooledParticles.Play(new NetworkPooledParticles.ParticleAdditionalInfo(OwnerClientId, (ushort)(damage * particlesPerDamagePoint)));
        }
    }
}