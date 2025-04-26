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
            if (!targetPlayer.InGameData.IsAlive()) return;
            if (targetPlayer.ClientId == ulong.MaxValue) return;
        
            ushort damage = CalculateDamage();
            PlayerDataManager.Instance[targetPlayer.ClientId] = new(targetPlayer) {InGameData = targetPlayer.InGameData.RemoveHealth(damage)};
        }
    }
}