using System;
using Game.Game_Loop.Round;
using Unity.Netcode;

namespace Game.Game_Loop
{
    public class GameLoopEvents : NetworkBehaviour
    {
        public static event Action<GameRoundState, float> OnRoundStateChangedAll;
        [Rpc(SendTo.Everyone)]
        private void OnRoundStateChangedClientRpc(GameRoundState state, float serverTime)
            => OnRoundStateChangedAll?.Invoke(state, serverTime);
        public void RoundStateChanged(GameRoundState newRoundState, float serverTime)
        {
            if (!IsServer) throw new InvalidOperationException("RoundChanged can only be called on the server.");
            OnRoundStateChangedClientRpc(newRoundState, serverTime);
        }
    
    
        public static event Action OnGameEnded;
        public void GameEnded()
        {
            if (!IsServer) throw new InvalidOperationException("OnGameEnded can only be called on the server.");
            OnGameEndedClientRpc();
        }
        [Rpc(SendTo.Everyone)]
        private void OnGameEndedClientRpc() => OnGameEnded?.Invoke();
    }
}