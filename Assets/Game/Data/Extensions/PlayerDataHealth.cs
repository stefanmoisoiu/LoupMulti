using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Unity.Netcode;
using UnityEngine;

namespace Game.Data.Extensions
{
    public class PlayerDataHealth : NetworkBehaviour
    {
        public static event Action<ushort, ushort, ulong> OnPlayerHealthChanged;
        public static event Action<ushort, ushort> OnOwnerPlayerHealthChanged;
        public static event Action<ulong> OnPlayerDeath;
        public static event Action OnOwnerPlayerDied;
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void OwnerPlayerHealthChangedRpc(ushort previousHealth, ushort newHealth, RpcParams @params)
        {
            OnOwnerPlayerHealthChanged?.Invoke(previousHealth, newHealth);
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void OwnerPlayerDeathRpc(RpcParams @params)
        {
            OnOwnerPlayerDied?.Invoke();
        }
        
        public static PlayerData[] AlivePlayingPlayers()
        {
            List<PlayerData> players = new List<PlayerData>();
            foreach (var data in DataManager.Instance.GetValues())
            {
                if (data.outerData.playingState == OuterData.PlayingState.Playing && data.inGameData.health > 0)
                    players.Add(data);
            }
            return players.ToArray();
        }
        public static int AlivePlayingPlayerCount() => AlivePlayingPlayers().Length;

        private void Start()
        {
            if (!IsServer) return;
            DataManager.OnEntryUpdatedServer += OnEntryUpdated;
        }

        private void OnDisable()
        {
            DataManager.OnEntryUpdatedServer -= OnEntryUpdated;
        }

        private void OnEntryUpdated(PlayerData previousData, PlayerData newData)
        {
            ushort previousHealth = previousData.inGameData.health;
            ushort newHealth = newData.inGameData.health;
            if (previousHealth == newHealth) return;
            
            OnPlayerHealthChanged?.Invoke(previousHealth, newHealth, newData.clientId);
            OwnerPlayerHealthChangedRpc(previousHealth, newHealth, newData.SendRpcTo());

            if (newData.inGameData.health == 0 && previousData.inGameData.health > 0)
            {
                OnPlayerDeath?.Invoke(newData.clientId);
                OwnerPlayerDeathRpc(newData.SendRpcTo());
            }
        }
    }
}