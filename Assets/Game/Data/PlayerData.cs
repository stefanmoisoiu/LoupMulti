using System;
using System.Collections.Generic;
using System.Linq;
using Base_Scripts;
using Game.Game_Loop.Round.Collect.Resource;
using Game.Manager;
using Game.Upgrades;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong ClientId;
        
        public OuterData outerData;
        public InGameData inGameData;
        
        public PlayerData(NetworkClient client)
        {
            ClientId = client?.ClientId ?? ulong.MaxValue;
            outerData = new OuterData(client);
            inGameData = new InGameData(upgradesIndexArray:InGameData.DefaultUpgradesIndexArray());
        }
        public PlayerData(PlayerData copy)
        {
            ClientId = copy.ClientId;
            outerData = new OuterData(copy.outerData);
            inGameData = new InGameData(copy.inGameData.health, copy.inGameData.upgradesIndexArray, copy.inGameData.resources);
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref outerData);
            serializer.SerializeValue(ref inGameData);
        }
        public RpcParams SendRpcTo() => RpcParamsExt.Instance.SendToClientIDs(new []{ClientId}, NetworkManager.Singleton);
        public override string ToString()
        {
            return $"ClientId: {ClientId}\n" +
                   $"OuterData:\n{outerData}\n \n" +
                   $"InGameData:\n{inGameData}";
        }

        public bool Equals(PlayerData other)
        {
            return ClientId == other.ClientId && outerData.Equals(other.outerData) && inGameData.Equals(other.inGameData);
        }
        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, outerData, inGameData);
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
            SpectatingUntilNextRound
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
        public const ushort MaxHealth = 100;
        public ushort health;
        public ushort[] upgradesIndexArray; 
        public ResourceData resources;
        [ShowInInspector] public string upgradesString => string.Join(", ", GetUpgrades().Select(u => u.UpgradeName));

        public override string ToString()
        {
            ScriptableUpgrade[] upgrades = GetUpgrades();
            return $"Health: {health}\nResources: {resources}\nUpgrades: {string.Join(", ", upgrades.Select(u => u))}";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref resources);
            serializer.SerializeValue(ref health);
            
            upgradesIndexArray ??= new ushort[UpgradesManager.MaxUpgrades];
            for (int i = 0; i < upgradesIndexArray.Length; i++)
                serializer.SerializeValue(ref upgradesIndexArray[i]);
        }
        
        public InGameData(ushort health = MaxHealth, ushort[] upgradesIndexArray = null, ResourceData resources = new())
        {
            this.health = health;
            this.upgradesIndexArray = upgradesIndexArray;
            this.resources = resources;
            
            if(upgradesIndexArray == null) Debug.LogError("initialise upgradesIndexArray ca marche pas sinon");
        }

        public InGameData(InGameData copy)
        {
            health = copy.health;
            resources = new ResourceData(copy.resources);
            upgradesIndexArray = copy.upgradesIndexArray?.ToArray();
        }
        
        public InGameData AddHealth(ushort amount)
        {
            if (health + amount > MaxHealth)
            {
                health = MaxHealth;
                return this;
            }
            health += amount;
            return this;
        }
        public InGameData RemoveHealth(ushort amount)
        {
            if (health - amount < 0)
            {
                health = 0;
                return this;
            }
            health -= amount;
            return this;
        }
        public InGameData ResetHealth()
        {
            health = MaxHealth;
            return this;
        }
        public bool IsAlive() => health > 0;
        
        public InGameData AddUpgrade(ushort upgradeIndex)
        {
            upgradesIndexArray ??= DefaultUpgradesIndexArray();
            for (int i = 0; i < upgradesIndexArray.Length; i++)
            {
                if (upgradesIndexArray[i] != ushort.MaxValue) continue;
                
                upgradesIndexArray[i] = upgradeIndex;
                return this;
            }
            return this;
        }
        public ScriptableUpgrade[] GetUpgrades()
        {
            if (upgradesIndexArray == null || upgradesIndexArray.Length == 0) return new ScriptableUpgrade[] {};
            
            List<ScriptableUpgrade> upgrades = new();

            for (int i = 0; i < UpgradesManager.MaxUpgrades; i++)
            {
                if (upgradesIndexArray[i] == ushort.MaxValue) continue;
                upgrades.Add(GameManager.Instance.UpgradesManager.GetUpgrade(upgradesIndexArray[i]));
            }
            return upgrades.ToArray();
        }
        public static ushort[] DefaultUpgradesIndexArray()
        { 
            ushort[] res = new ushort[UpgradesManager.MaxUpgrades];
            for (int i = 0; i < UpgradesManager.MaxUpgrades; i++) res[i] = ushort.MaxValue;
            return res;
        }
        
        public bool Equals(InGameData other)
        {
            return health == other.health &&
                   resources.Equals(other.resources) &&
                   Equals(upgradesIndexArray, other.upgradesIndexArray);
        }
        public override bool Equals(object obj)
        {
            return obj is InGameData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(health, resources, upgradesIndexArray);
        }
    }
    
    public struct ResourceData : INetworkSerializable, IEquatable<ResourceData>
    {
        public ushort commonAmount;
        public ushort rareAmount;
        
        public ResourceData(ushort commonAmount = 0, ushort rareAmount = 0)
        {
            this.commonAmount = commonAmount;
            this.rareAmount = rareAmount;
        }
        public ResourceData(ResourceData copy)
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
        public ResourceData AddResource(ResourceType type, ushort amount = 1)
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
        public ResourceData RemoveResource(ResourceType type, ushort amount = 1)
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

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref commonAmount);
            serializer.SerializeValue(ref rareAmount);
        }

        public bool Equals(ResourceData other)
        {
            return commonAmount == other.commonAmount && rareAmount == other.rareAmount;
        }

        public override string ToString()
        {
            return $"Common: {commonAmount}\nRare: {rareAmount}";
        }
    }
}