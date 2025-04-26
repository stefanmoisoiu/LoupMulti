using System;
using System.Collections.Generic;
using System.Linq;
using Base_Scripts;
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
        
        public PlayerOuterData OuterData;
        public PlayerInGameData InGameData;
        
        public PlayerData(NetworkClient client)
        {
            ClientId = client?.ClientId ?? ulong.MaxValue;
            OuterData = new PlayerOuterData(client);
            InGameData = new PlayerInGameData(UpgradesIndexArray:PlayerInGameData.DefaultUpgradesIndexArray());
        }
        public PlayerData(PlayerData copy)
        {
            ClientId = copy.ClientId;
            OuterData = new PlayerOuterData(copy.OuterData);
            InGameData = new PlayerInGameData(copy.InGameData.score, copy.InGameData.health, copy.InGameData.upgradesIndexArray);
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref OuterData);
            serializer.SerializeValue(ref InGameData);
        }
        public RpcParams SendRpcTo() => RpcParamsExt.Instance.SendToClientIDs(new []{ClientId}, NetworkManager.Singleton);
        public override string ToString()
        {
            return $"ClientId: {ClientId}\n" +
                   $"OuterData:\n{OuterData}\n \n" +
                   $"InGameData:\n{InGameData}";
        }

        public bool Equals(PlayerData other)
        {
            return ClientId == other.ClientId && OuterData.Equals(other.OuterData) && InGameData.Equals(other.InGameData);
        }
        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, OuterData, InGameData);
        }
    }
    
    [Serializable]
    public struct PlayerOuterData : INetworkSerializable, IEquatable<PlayerOuterData>
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

        public PlayerOuterData(NetworkClient client = null)
        {
            playingState = PlayingState.NotAssigned;
        }
        public PlayerOuterData(PlayerOuterData copy)
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
        
        public PlayerOuterData SetState(PlayingState newState)
        {
            playingState = newState;
            return this;
        }

        public bool Equals(PlayerOuterData other)
        {
            return playingState == other.playingState;
        }
        public override bool Equals(object obj)
        {
            return obj is PlayerOuterData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return (int)playingState;
        }
    }

    [Serializable]
    public struct PlayerInGameData : INetworkSerializable, IEquatable<PlayerInGameData>
    {
        public const ushort MaxHealth = 100;
        public ushort health;
        public ushort score;
        public ushort[] upgradesIndexArray; 
        [ShowInInspector] public string upgradesString => string.Join(", ", GetUpgrades().Select(u => u.UpgradeName));

        public override string ToString()
        {
            ScriptableUpgrade[] upgrades = GetUpgrades();
            return $"Health: {health}\nScore: {score}\nUpgrades: {string.Join(", ", upgrades.Select(u => u))}";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref score);
            serializer.SerializeValue(ref health);
            
            upgradesIndexArray ??= new ushort[UpgradesManager.MaxUpgrades];
            for (int i = 0; i < upgradesIndexArray.Length; i++)
                serializer.SerializeValue(ref upgradesIndexArray[i]);
        }
        
        public PlayerInGameData(ushort Score = 0, ushort Health = 100, ushort[] UpgradesIndexArray = null)
        {
            score = Score;
            health = 100;
            
            upgradesIndexArray = UpgradesIndexArray;
            
            if(UpgradesIndexArray == null) Debug.LogError("initialise upgradesIndexArray ca marche pas sinon");
        }
        
        public PlayerInGameData AddScore(ushort amount)
        {
            score += amount;
            return this;
        }
        public PlayerInGameData RemoveScore(ushort amount)
        {
            if (score - amount < 0)
            {
                score = 0;
                return this;
            }
            score -= amount;
            return this;
        }
        public PlayerInGameData ResetScore()
        {
            score = 0;
            return this;
        }
        
        public PlayerInGameData AddHealth(ushort amount)
        {
            if (health + amount > MaxHealth)
            {
                health = MaxHealth;
                return this;
            }
            health += amount;
            return this;
        }
        public PlayerInGameData RemoveHealth(ushort amount)
        {
            if (health - amount < 0)
            {
                health = 0;
                return this;
            }
            health -= amount;
            return this;
        }
        public PlayerInGameData ResetHealth()
        {
            health = MaxHealth;
            return this;
        }
        public bool IsAlive() => health > 0;
        
        public PlayerInGameData AddUpgrade(ushort upgradeIndex)
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
        
        public bool Equals(PlayerInGameData other)
        {
            return health == other.health && score == other.score &&
                   Equals(upgradesIndexArray, other.upgradesIndexArray);
        }
        public override bool Equals(object obj)
        {
            return obj is PlayerInGameData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(health, score, upgradesIndexArray);
        }
    }
}