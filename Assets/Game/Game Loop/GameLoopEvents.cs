using System;
using Game.Game_Loop.Round;
using Unity.Netcode;

namespace Game.Game_Loop
{
    public class GameLoopEvents : NetworkBehaviour
    {
        public NetworkVariable<GameRoundState> roundState = new(GameRoundState.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public static event Action<GameRoundState, float> OnRoundStateChangedAll;
        [Rpc(SendTo.Everyone)]
        private void OnRoundStateChangedClientRpc(GameRoundState state, float serverTime)
            => OnRoundStateChangedAll?.Invoke(state, serverTime);
        public void RoundStateChanged(GameRoundState newRoundState, float serverTime)
        {
            if (!IsServer) throw new InvalidOperationException("RoundChanged can only be called on the server.");
            roundState.Value = newRoundState;
            OnRoundStateChangedClientRpc(newRoundState, serverTime);
        }
    }
}