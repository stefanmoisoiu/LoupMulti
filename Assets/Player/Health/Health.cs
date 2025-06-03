using Game.Common;
using Game.Data;
using Game.Game_Loop;
using Player.Networking;
using Player.Stats;
using UnityEngine;

namespace Player.Health
{
    public class Health : PNetworkBehaviour
    {
        private ClientScheduledAction healAction;
        
        public void Heal(ushort amount)
        {
            DataManager.Instance.PlayerHealth.AddHealthServerRpc(amount, OwnerClientId);
        }
        public void Damage(ushort amount)
        {
            DataManager.Instance.PlayerHealth.RemoveHealthServerRpc(amount, OwnerClientId);
        }
        
        protected override void StartOnlineOwner()
        {
            healAction?.Cancel();
            healAction = new(TryApplyHealPerSecond, GameTickManager.TICKRATE, true, true);
            
        }

        protected override void DisableAnyOwner()
        {
            healAction?.Cancel();
        }

        private void TryApplyHealPerSecond()
        {
            if (GameTickManager.CurrentTick % GameTickManager.TICKRATE != 0) return;
            int healAmount = (int)PlayerStats.HealthPerSecond.Apply(0);
            switch (healAmount)
            {
                case > 0:
                    Heal((ushort)healAmount);
                    break;
                case < 0:
                    Damage((ushort)(-healAmount));
                    break;
            }
        }
    }
}