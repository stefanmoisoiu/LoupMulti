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
        [SerializeField] private ushort particlesPerDamagePoint = 2;
        

        protected override void StartOnlineOwner()
        {
            PlayerHealth.OnHealthChangedOwner += OnHealthChanged;
        }
        protected override void DisableAnyOwner()
        {
            PlayerHealth.OnHealthChangedOwner -= OnHealthChanged;
        }

        private void OnHealthChanged(ushort previousHealth, ushort newHealth)
        {
            if (newHealth > previousHealth) return;
            ushort damage = (ushort)(previousHealth - newHealth);
            if (damage > 0)
                pooledParticles.Play(new NetworkPooledParticles.ParticleAdditionalInfo(OwnerClientId, (ushort)(damage * particlesPerDamagePoint)));
        }
    }
}