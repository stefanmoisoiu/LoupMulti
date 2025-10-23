

using System;
using System.Collections.Generic;
using System.Linq;
using Base_Scripts;
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
            inGameData = new InGameData(
                health: GameSettings.PlayerMaxHealth,
                items: InGameData.DefaultItemsArray(),
                resources: new PlayerResourceData());
        }
        public PlayerData(PlayerData copy)
        {
            clientId = copy.clientId; 
            
            outerData = new OuterData(copy.outerData);
            inGameData = new InGameData(copy.inGameData.health, copy.inGameData.items, copy.inGameData.resources);
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
            return $"ClientId: {clientId}\n" +
                   $"OuterData:\n{outerData}\n \n" +
                   $"InGameData:\n{inGameData}";
        }

        public bool Equals(PlayerData other)
        {
            return clientId == other.clientId && outerData.Equals(other.outerData) && inGameData.Equals(other.inGameData);
        }
        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(clientId, outerData, inGameData);
        }
    }
    
    [Serializable]
    public struct OuterData : INetworkSerializable, IEquatable<OuterData>
    {
        public PlayingState playingState;
        
        public enum PlayingState
        {
            NotAssigned,
            Disconnected,
            Playing,
            SpectatingGame,
        }

        public OuterData(NetworkClient client = null)
        {
            playingState = PlayingState.NotAssigned;
        }
        public OuterData(OuterData copy)
        {
            playingState = copy.playingState;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playingState);
        }
        
        public override string ToString()
        {
            return $"Playing State: {playingState}";
        }
        
        public OuterData SetState(PlayingState newState)
        {
            playingState = newState;
            return this;
        }

        public bool Equals(OuterData other)
        {
            return playingState == other.playingState;
        }
        public override bool Equals(object obj)
        {
            return obj is OuterData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return (int)playingState;
        }
    }

    [Serializable]
    public struct InGameData : INetworkSerializable, IEquatable<InGameData>
    {
        public ushort health;
        
        
        public ushort[] items;
        public PlayerResourceData resources;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref resources);
            serializer.SerializeValue(ref health);
            
            items ??= new ushort[GameSettings.Instance.MaxItems];
            
            for (int i = 0; i < items.Length; i++)
                serializer.SerializeValue(ref items[i]);
        }
        
        public InGameData(ushort health = GameSettings.PlayerMaxHealth, ushort[] items = null, PlayerResourceData resources = new())
        {
            this.health = health;
            if (health > GameSettings.PlayerMaxHealth) this.health = GameSettings.PlayerMaxHealth;

            this.items = items;
            this.resources = resources;
            
            if (items == null) throw new ArgumentNullException(nameof(items), "Items array cannot be null.");
        }
        
        public InGameData SetResources(PlayerResourceData newResources)
        {
            resources = newResources;
            return this;
        }

        public InGameData(InGameData copy)
        {
            health = copy.health;
            items = copy.items;
            resources = copy.resources;
        }
        
        public InGameData AddHealth(ushort amount)
        {
            if (health + amount > GameSettings.PlayerMaxHealth)
            {
                health = GameSettings.PlayerMaxHealth;
                return this;
            }
            health += amount;
            return this;
        }
        public InGameData RemoveHealth(ushort amount)
        {
            if (amount > health)
            {
                health = 0;
                return this;
            }
            health -= amount;
            return this;
        }
        public InGameData ResetHealth()
        {
            health = GameSettings.PlayerMaxHealth;
            return this;
        }
        public bool IsAlive() => health > 0;
        
        public InGameData AddItem(ushort itemIndex)
        {
            items ??= new ushort[GameSettings.Instance.MaxItems];
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != ushort.MaxValue) continue;
                
                items[i] = itemIndex;
                return this;
            }
            return this;
        }
        
        public Item[] GetItems()
        {
            items ??= new ushort[GameSettings.Instance.MaxItems];
            List<Item> res = new List<Item>(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == ushort.MaxValue) continue;
                Item item = ItemRegistry.Instance.GetItem(items[i]);
                res.Add(item);
            }
            return res.ToArray();
        }
        public static ushort[] DefaultItemsArray()
        { 
            ushort[] res = new ushort[GameSettings.Instance.MaxItems];
            for (int i = 0; i < GameSettings.Instance.MaxItems; i++) res[i] = ushort.MaxValue;
            return res;
        }
        
        public bool Equals(InGameData other)
        {
            return health == other.health &&
                   resources.Equals(other.resources) &&
                   Equals(items, other.items);
        }
        public override bool Equals(object obj)
        {
            return obj is InGameData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(health, resources, items);
        }
    }
    
    public struct PlayerResourceData : INetworkSerializable, IEquatable<PlayerResourceData>
    {
        public ushort commonAmount;
        public ushort rareAmount;
        
        public PlayerResourceData(ushort commonAmount = 0, ushort rareAmount = 0)
        {
            this.commonAmount = commonAmount;
            this.rareAmount = rareAmount;
        }
        public PlayerResourceData(PlayerResourceData copy)
        {
            commonAmount = copy.commonAmount;
            rareAmount = copy.rareAmount;
        }

        
        public bool HasEnough(ResourceType type, ushort amount)
        {
            switch (type)
            {
                case ResourceType.Common:
                    return commonAmount >= amount;
                case ResourceType.Rare:
                    return rareAmount >= amount;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public PlayerResourceData AddResource(ResourceType type, ushort amount = 1)
        {
            switch (type)
            {
                case ResourceType.Common:
                    commonAmount += amount;
                    break;
                case ResourceType.Rare:
                    rareAmount += amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return this;
        }
        public PlayerResourceData RemoveResource(ResourceType type, ushort amount = 1)
        {
            switch (type)
            {
                case ResourceType.Common:
                    commonAmount -= amount;
                    break;
                case ResourceType.Rare:
                    rareAmount -= amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return this;
        }
        
        public ushort GetResourceAmount(ResourceType type)
        {
            return type switch
            {
                ResourceType.Common => commonAmount,
                ResourceType.Rare => rareAmount,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref commonAmount);
            serializer.SerializeValue(ref rareAmount);
        }

        public bool Equals(PlayerResourceData other)
        {
            return commonAmount == other.commonAmount && rareAmount == other.rareAmount;
        }

        public override string ToString()
        {
            return $"Common: {commonAmount}\nRare: {rareAmount}";
        }
    }
}