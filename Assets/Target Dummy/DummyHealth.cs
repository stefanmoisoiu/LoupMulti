using System;
using Game.Common;
using Unity.Netcode;
using UnityEngine;

namespace Target_Dummy
{
    public class DummyHealth : NetworkBehaviour, IDamageable, IHealable
    {
        public event Action<ushort> OnDamaged;
        public void TakeDamage(IDamageable.DamageInfo info)
        {
            Debug.Log(info.DamageAmount);
            OnDamaged?.Invoke(info.DamageAmount);
            TakeDamageServerRpc(info);
        }
        [ServerRpc] private void TakeDamageServerRpc(IDamageable.DamageInfo info) => TakeDamageClientRpc(info);
        [Rpc(SendTo.NotOwner)] private void TakeDamageClientRpc(IDamageable.DamageInfo info) => OnDamaged?.Invoke(info.DamageAmount);

        public event Action<ushort> OnHealed;
        public void Heal(IHealable.HealInfo info)
        {
            OnHealed?.Invoke(info.HealAmount);
            HealServerRpc(info);
        }
        [ServerRpc] private void HealServerRpc(IHealable.HealInfo info) => HealClientRpc(info);
        [Rpc(SendTo.NotOwner)] private void HealClientRpc(IHealable.HealInfo info) => OnHealed?.Invoke(info.HealAmount);
    }
}