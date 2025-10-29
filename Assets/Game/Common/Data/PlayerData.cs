using System;
using System.Collections.Generic;
using System.Linq;
using Base_Scripts;
using Game.Common;
using Game.Common.List;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Game.Common
{
    [Serializable]
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong clientId;
        public OuterData outerData;
        public InGameData inGameData;

        public PlayerData(NetworkClient client)
        {
            clientId = client?.ClientId ?? ulong.MaxValue;
            outerData = new OuterData(client);
            // Le constructeur InGameData s'occupe de la valeur par défaut (2)
            inGameData = new InGameData(
                health: ushort.MaxValue,
                rerolls: ushort.MaxValue,
                ownedItems: new List<OwnedItemData>(),
                resources: new OwnedResourcesData());
        }
        public PlayerData(PlayerData copy)
        {
            clientId = copy.clientId;
            outerData = new OuterData(copy.outerData);
            inGameData = new InGameData(copy.inGameData);
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref outerData);
            serializer.SerializeValue(ref inGameData);
        }
        public RpcParams SendRpcTo() => RpcParamsExt.Instance.SendToClientIDs(new []{clientId});
        public override string ToString()
        {
            return $"ClientId: {clientId}\nOuterData:\n{outerData}\n \nInGameData:\n{inGameData}";
        }
        public bool Equals(PlayerData other)
        {
            return clientId == other.clientId && outerData.Equals(other.outerData) && inGameData.Equals(other.inGameData);
        }
        public override bool Equals(object obj) => obj is PlayerData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(clientId, outerData, inGameData);
    }
}