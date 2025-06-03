using Game.Common;
using Game.Data;
using Unity.Netcode;
using UnityEngine;

namespace Game.Game_Loop.Round.Tag.Hot_Potato
{
    public class HotPotatoDamage : NetworkBehaviour
    {
        [SerializeField] private int healthLossPerSec = 5;
        [SerializeField] private int healthLossTickDelay = 10;
    
        private ushort CalculateDamage() =>
            (ushort)(healthLossPerSec * healthLossTickDelay / GameTickManager.TICKRATE);

        public void TryApplyDamage(PlayerData targetPlayer)
        {
            if (!IsServer) return;
            if (GameTickManager.CurrentTick % healthLossTickDelay != 0) return;
            if (!targetPlayer.inGameData.IsAlive()) return;
            if (targetPlayer.ClientId == ulong.MaxValue) return;
        
            ushort damage = CalculateDamage();
            DataManager.Instance[targetPlayer.ClientId] = new(targetPlayer) {inGameData = targetPlayer.inGameData.RemoveHealth(damage)};
        }
    }
}