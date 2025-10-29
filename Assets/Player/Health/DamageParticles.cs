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
            PlayerHealthHelper.OnPlayerHealthChangedOwner += OnPlayerHealthChangedHealthChanged;
        }
        protected override void DisableAnyOwner()
        {
            PlayerHealthHelper.OnPlayerHealthChangedOwner -= OnPlayerHealthChangedHealthChanged;
        }

        private void OnPlayerHealthChangedHealthChanged(ushort previousHealth, ushort newHealth)
        {
            if (newHealth > previousHealth) return;
            float damage = (float)previousHealth - newHealth;
            if (damage > 0)
                pooledParticles.Play(new NetworkPooledParticles.ParticleAdditionalInfo(OwnerClientId, (ushort)(damage * particlesPerDamagePoint)));
        }
    }
}