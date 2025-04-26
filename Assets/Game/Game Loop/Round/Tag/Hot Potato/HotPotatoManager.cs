using Base_Scripts;
using Game.Data.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace Game.Game_Loop.Round.Tag.Hot_Potato
{
    public class HotPotatoManager : NetworkBehaviour
    {
        [SerializeField] private HotPotatoTarget target;
        public HotPotatoTarget Target => target;
        [SerializeField] private HotPotatoDamage damage;
        public HotPotatoDamage Damage => damage;
    
        public bool HotPotatoActive { get; private set; }

        private void Start()
        {
            if (!IsServer) return;
            PlayerHealth.OnPlayerDiedServer += ChangeTargetOnDeath;
        }

        private void ChangeTargetOnDeath(ulong _)
        {
            if (!HotPotatoActive) return;
            if (PlayerHealth.AlivePlayerCount() <= 1) return;
        
            ulong randomId = PlayerHealth.RandomAlivePlayer().ClientId;
            NetcodeLogger.Instance.LogRpc($"Player Died ! New Target : {randomId}", NetcodeLogger.LogType.GameLoop);
            target.SetTarget(randomId);
        }

        public void Enable() => Enable(PlayerHealth.RandomAlivePlayer().ClientId);
        public void Enable(ulong targetId)
        {
            if (HotPotatoActive) return;
            target.SetTarget(targetId);
            GameTickManager.OnTickServer += OnTick;
            HotPotatoActive = true;
        }
        private void OnTick() => damage.TryApplyDamage(target.TargetPlayer);
        public void Disable()
        {
            if (!HotPotatoActive) return;
            target.ResetTarget();
            GameTickManager.OnTickServer -= OnTick;
            HotPotatoActive = false;
        }
    }
}