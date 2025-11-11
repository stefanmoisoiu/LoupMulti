using Game.Common;
using Game.Data;
using Game.Data.Extensions;
using Game.Game_Loop;
using Player.Stats;
using Sirenix.OdinInspector;

namespace Player.Health
{
    using System;
    using Unity.Netcode;
    using UnityEngine;

    public class PlayerHealthComponent : NetworkBehaviour, IDamageable, IHealable, IHealth
    {
        private PlayerReferences _playerReferences;
        public event Action<ushort, ushort> OnHealthChanged;

        private void OnEnable()
        {
            _playerReferences ??= GetComponentInParent<PlayerReferences>();
            PlayerHealthHelper.OnPlayerHealthChangedServer += CheckHealthChangedServer;
        }

        private void OnDisable()
        {
            PlayerHealthHelper.OnPlayerHealthChangedServer -= CheckHealthChangedServer;
        }

        private void CheckHealthChangedServer(ushort previousHealth, ushort newHealth, ulong clientId)
        {
            if (clientId != OwnerClientId) return;
            OnHealthChanged?.Invoke(previousHealth, newHealth);
        }

        public event Action<ushort> OnDamaged;

        public void TakeDamage(IDamageable.DamageInfo info)
        {
            TakeDamageServerRpc(GameTickManager.CurrentTick, info);
        }
        [Rpc(SendTo.Server)]
        private void TakeDamageServerRpc(ushort tick, IDamageable.DamageInfo info)
        {
            if (tick > GameTickManager.CurrentTick)
                throw new Exception($"Tick {tick} is greater than current tick {GameTickManager.CurrentTick}");

            info.DamageAmount = (ushort)Mathf.CeilToInt(info.DamageAmount * 100f /
                                                        _playerReferences.StatManager.GetStat(StatType.Armor)
                                                            .GetValue(100));
            
            PlayerData playerData = DataManager.Instance[OwnerClientId];
            
            if (!playerData.inGameData.IsAlive())
            {
                Debug.Log($"Player {OwnerClientId} is already dead. Ignoring damage.");
                return;
            }
            
            playerData.inGameData = playerData.inGameData.Damage(info.DamageAmount);
            DataManager.Instance[OwnerClientId] = playerData;
            
            TakeDamageClientRpc(info.DamageAmount);
        }
        [ClientRpc] private void TakeDamageClientRpc(ushort damageAmount) => OnDamaged?.Invoke(damageAmount);

        public event Action<ushort> OnHealed;

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
            if (playerData.inGameData.health >= playerData.inGameData.maxHealth)
            {
                Debug.Log($"Player {OwnerClientId} is already at max health. Ignoring heal.");
                return;
            }
            
            playerData.inGameData = playerData.inGameData.Heal(info.HealAmount);
            DataManager.Instance[OwnerClientId] = playerData;
            
            OnHealedClientRpc(info.HealAmount);
        }
        [ClientRpc] private void OnHealedClientRpc(ushort healAmount) => OnHealed?.Invoke(healAmount);

        public ushort GetHealth()
        {
            if (DataManager.Instance == null || !DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData playerData)) return GameSettings.Instance.PlayerMaxHealth;
            return playerData.inGameData.health;
        }
        
        public ushort GetMaxHealth()
        {
            if (DataManager.Instance == null || !DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData playerData)) return GameSettings.Instance.PlayerMaxHealth;
            return playerData.inGameData.maxHealth;
        }
    }
}