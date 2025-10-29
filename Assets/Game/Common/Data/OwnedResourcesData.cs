using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Common
{
    public struct OwnedResourcesData : INetworkSerializable, IEquatable<OwnedResourcesData>
    {
        public ushort commonAmount;
        public ushort rareAmount;

        public OwnedResourcesData(ushort commonAmount = 0, ushort rareAmount = 0) { this.commonAmount = commonAmount; this.rareAmount = rareAmount; }
        public OwnedResourcesData(OwnedResourcesData copy) { commonAmount = copy.commonAmount; rareAmount = copy.rareAmount; }
        public bool HasEnough(ResourceType type, ushort amount) => type == ResourceType.Common ? commonAmount >= amount : rareAmount >= amount;
        public OwnedResourcesData AddResource(ResourceType type, ushort amount = 1)
        {
            if (type == ResourceType.Common) commonAmount += amount;
            else rareAmount += amount;
            return this;
        }
        public OwnedResourcesData RemoveResource(ResourceType type, ushort amount = 1)
        {
            if (type == ResourceType.Common) commonAmount = (ushort)Mathf.Max(0, commonAmount - amount);
            else rareAmount = (ushort)Mathf.Max(0, rareAmount - amount);
            return this;
        }
        public ushort GetResourceAmount(ResourceType type) => type == ResourceType.Common ? commonAmount : rareAmount;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter { serializer.SerializeValue(ref commonAmount); serializer.SerializeValue(ref rareAmount); }
        public bool Equals(OwnedResourcesData other) => commonAmount == other.commonAmount && rareAmount == other.rareAmount;
        public override string ToString() => $"Common: {commonAmount}\nRare: {rareAmount}";
        public override int GetHashCode() => HashCode.Combine(commonAmount, rareAmount);
    }
}