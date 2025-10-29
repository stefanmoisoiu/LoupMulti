using System;
using Unity.Netcode;

namespace Game.Common
{
    [Serializable]
    public struct OuterData : INetworkSerializable, IEquatable<OuterData>
    {
        public PlayingState playingState;
        public enum PlayingState { NotAssigned, Disconnected, Playing, SpectatingGame }

        public OuterData(NetworkClient client = null) { playingState = PlayingState.NotAssigned; }
        public OuterData(OuterData copy) { playingState = copy.playingState; }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter { serializer.SerializeValue(ref playingState); }
        public override string ToString() => $"Playing State: {playingState}";
        public OuterData SetState(PlayingState newState) { playingState = newState; return this; }
        public bool Equals(OuterData other) => playingState == other.playingState;
        public override bool Equals(object obj) => obj is OuterData other && Equals(other);
        public override int GetHashCode() => (int)playingState;
    }
}