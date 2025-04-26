using System;
using Game.Data;
using Unity.Netcode;

namespace Game.Game_Loop.Round.Tag.Hot_Potato
{
    public class HotPotatoTarget : NetworkBehaviour
    {
        private NetworkVariable<ulong> target = new(ulong.MaxValue);
    
        public ulong TargetClientId => target.Value;
        public PlayerData TargetPlayer => PlayerDataManager.Instance[target.Value];
    
        public static event Action<ulong> OnTargetChanged;
        public static event Action OnTargetReset;

        public void SetTarget(ulong clientId)
        {
            if (!IsServer) throw new System.InvalidOperationException("SetTarget can only be called on the server.");
            target.Value = clientId;
            OnTargetChanged?.Invoke(clientId);
        }
        public void ResetTarget()
        {
            if (!IsServer) throw new System.InvalidOperationException("ResetTarget can only be called on the server.");
            target.Value = ulong.MaxValue;
            OnTargetReset?.Invoke();
        }
    
        private ulong GetRandomAliveClientId()
        {
            ulong[] keys = PlayerDataManager.Instance.GetKeys();
            ushort randomIndex;
            if (keys.Length == 0) return ulong.MaxValue;
            do
            {
                randomIndex = (ushort)UnityEngine.Random.Range(0, keys.Length);
            }
            while (!PlayerDataManager.Instance[randomIndex].InGameData.IsAlive());
            return keys[randomIndex];
        }
    }
}