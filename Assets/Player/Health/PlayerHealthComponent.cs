using Game.Common;
using Game.Data;
using Game.Data.Extensions;
using Game.Game_Loop;
using Sirenix.OdinInspector;

namespace Player.Health
{
    using System;
    using Unity.Netcode;
    using UnityEngine;

    public class PlayerHealthComponent : NetworkBehaviour, IDamageable, IHealable, IHealth
    {
        public event Action<ushort, ushort> OnHealthChanged;

        private void OnEnable()
        {
            PlayerDataHealth.OnPlayerHealthChanged += CheckHealthChanged;
        }

        private void OnDisable()
        {
            PlayerDataHealth.OnPlayerHealthChanged -= CheckHealthChanged;
        }

        private void CheckHealthChanged(ushort previousHealth, ushort newHealth, ulong clientId)
        {
            if (clientId != OwnerClientId) return;
            
            OnHealthChanged?.Invoke(previousHealth, newHealth);
        }
        
        public void TakeDamage(IDamageable.DamageInfo info)
        {
            TakeDamageServerRpc(GameTickManager.CurrentTick, info);
        }
        [Rpc(SendTo.Server)]
        private void TakeDamageServerRpc(ushort tick, IDamageable.DamageInfo info)
        {
            if (tick > GameTickManager.CurrentTick)
                throw new Exception($"Tick {tick} is greater than current tick {GameTickManager.CurrentTick}");
            
            PlayerData playerData = DataManager.Instance[OwnerClientId];
            
            if (!playerData.inGameData.IsAlive())
            {
                Debug.Log($"Player {OwnerClientId} is already dead. Ignoring damage.");
                return;
            }
            
            ushort previousHealth = playerData.inGameData.health;
            playerData.inGameData = playerData.inGameData.RemoveHealth(info.DamageAmount);
            DataManager.Instance[OwnerClientId] = playerData;
        }

        public void Heal(IHealable.HealInfo info)
        {
            HealServerRpc(GameTickManager.CurrentTick, info);
        }
        [Rpc(SendTo.Server)]
        private void HealServerRpc(ushort tick, IHealable.HealInfo info)
        {
            if (tick > GameTickManager.CurrentTick)
                throw new Exception($"Tick {tick} is greater than current tick {GameTickManager.CurrentTick}");
            
            if (info.HealAmount == 0) return;
            
            PlayerData playerData = DataManager.Instance[OwnerClientId];
            ushort previousHealth = playerData.inGameData.health;
            
            if (!playerData.inGameData.IsAlive())
            {
                Debug.Log($"Player {OwnerClientId} is dead. Ignoring heal.");
                return;
            }
            if (playerData.inGameData.health >= GameSettings.PlayerMaxHealth)
            {
                Debug.Log($"Player {OwnerClientId} is already at max health. Ignoring heal.");
                return;
            }
            
            playerData.inGameData = playerData.inGameData.AddHealth(info.HealAmount);
            DataManager.Instance[OwnerClientId] = playerData;
            Debug.Log($"Healing {info.HealAmount} health from {info.Origin}. {previousHealth}HP -> {playerData.inGameData.health}HP");
        }

        public ushort GetHealth()
        {
            if (DataManager.Instance == null) return GameSettings.PlayerMaxHealth;
            return DataManager.Instance[OwnerClientId].inGameData.health;
        }
        
        public ushort GetMaxHealth()
        {
            return GameSettings.PlayerMaxHealth;
        }
    }
}