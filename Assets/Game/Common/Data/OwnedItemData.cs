using System;
using Unity.Netcode;

namespace Game.Common
{
    [Serializable]
    public struct OwnedItemData : INetworkSerializable, IEquatable<OwnedItemData>
    {
        public ushort ItemRegistryIndex;
        public ushort Level;

        public OwnedItemData(ushort index = ushort.MaxValue, ushort level = 0)
        {
            ItemRegistryIndex = index;
            Level = level;
        }

        public bool IsValid() => ItemRegistryIndex != ushort.MaxValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ItemRegistryIndex);
            serializer.SerializeValue(ref Level);
        }

        public bool Equals(OwnedItemData other) => ItemRegistryIndex == other.ItemRegistryIndex && Level == other.Level;
        public override bool Equals(object obj) => obj is OwnedItemData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ItemRegistryIndex, Level);
    }
}