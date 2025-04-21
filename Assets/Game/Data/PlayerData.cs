    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Netcode;
    using UnityEngine;

    [Serializable]
    public struct PlayerGameData : INetworkSerializable
    {
        public PlayerData[] playerDatas;

        public static PlayerData[] BasePlayerDatas()
        {
            PlayerData[] res = new PlayerData[NetcodeManager.MaxPlayers];
            for (int i = 0; i < NetcodeManager.MaxPlayers; i++) res[i] = new PlayerData(null);
            return res;
        }
        public PlayerGameData(PlayerData[] playerDatas)
        {
            this.playerDatas = playerDatas;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            playerDatas ??= BasePlayerDatas();
            if (serializer.IsReader) playerDatas = BasePlayerDatas();
            for (int i = 0; i < NetcodeManager.MaxPlayers; i++) serializer.SerializeValue(ref playerDatas[i]);
        }

        public PlayerGameData AddOrUpdateData(PlayerData data)
        {
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (playerDatas[i].ClientId != data.ClientId) continue;
                playerDatas[i] = data;
                return this;
            }
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (playerDatas[i].ClientId != ulong.MaxValue) continue;
                playerDatas[i] = data;
                return this;
            }
            Debug.LogError("Add PlayerData failed for clientId: " + data.ClientId);
            return this;
        }

        public PlayerGameData RemoveData(PlayerData data)
        {
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (playerDatas[i].ClientId != data.ClientId) continue;
                playerDatas[i] = new PlayerData();
                return this;
            }
            Debug.LogError("Remove PlayerData failed for clientId: " + data.ClientId);
            return this;
        }
        public PlayerGameData RemoveData(ulong clientId)
        {
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (playerDatas[i].ClientId != clientId) continue;
                playerDatas[i] = new PlayerData();
                return this;
            }
            Debug.LogError("Remove PlayerData failed for clientId: " + clientId);
            return this;
        }

        public PlayerData GetDataOrDefault(ulong clientId)
        {
            if (playerDatas == null) return new();
            if (playerDatas.Length == 0) return new();
            if (clientId == ulong.MaxValue) return new();
            
            foreach (PlayerData data in playerDatas)
            {
                if (data.ClientId != clientId) continue;
                return data;
            }
            return new();
        }
        
        public PlayerGameData UpdateData(PlayerData data)
        {
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (playerDatas[i].ClientId != data.ClientId) continue;
                playerDatas[i] = data;
                return this;
            }
            Debug.LogError("Update PlayerData failed for clientId: " + data.ClientId);
            return this;
        }
        
        public PlayerData[] GetDatas()
        {
            List<PlayerData> res = new();
            foreach (PlayerData data in playerDatas)
            {
                if (data.ClientId == ulong.MaxValue) continue;
                res.Add(data);
            }
            return res.ToArray();
        }
        
        public bool HasPlayerData(ulong clientId)
        {
            return playerDatas.Any(data => data.ClientId == clientId);
        }
        
        public PlayerData GetRandomPlayerData()
        {
            var validPlayers = playerDatas.Where(data => data.ClientId != ulong.MaxValue).ToList();
            return validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
        }
    }

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

        public RpcParams ToRpcParams() => RpcParamsExt.Instance.SendToClientIDs(new []{ClientId}, NetworkManager.Singleton);
        
        public override string ToString()
        {
            return $"ClientId: {ClientId}\nOuterData: {OuterData}\nInGameData: {InGameData}";
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
        public PlayerState CurrentPlayerState;
        
        public enum PlayerState
        {
            NotAssigned,
            Disconnected,
            Playing,
            SpectatingGame,
            SpectatingUntilNextRound
        }

        public PlayerOuterData(NetworkClient client = null)
        {
            CurrentPlayerState = PlayerState.NotAssigned;
        }

        public PlayerOuterData(PlayerOuterData copy)
        {
            CurrentPlayerState = copy.CurrentPlayerState;
        }
        
        public PlayerOuterData SetState(PlayerState state)
        {
            CurrentPlayerState = state;
            return this;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CurrentPlayerState);
        }
        
        public override string ToString()
        {
            return $"CurrentPlayerState: {CurrentPlayerState}";
        }

        public bool Equals(PlayerOuterData other)
        {
            return CurrentPlayerState == other.CurrentPlayerState;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerOuterData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)CurrentPlayerState;
        }
    }

    [Serializable]
    public struct PlayerInGameData : INetworkSerializable, IEquatable<PlayerInGameData>
    {
        public ushort health;
        public ushort score;
        public ushort[] upgradesIndexArray;
        [ShowInInspector] [ShowIf("@debugUpgrades")] public string upgradesString => string.Join(", ", GetUpgrades().Select(u => u.UpgradeName));

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
            health = 100;
            return this;
        }
        
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
        public NativeArray<ushort> DefaultUpgradesIndexArrayNative()
        {
            NativeArray<ushort> res = new NativeArray<ushort>(UpgradesManager.MaxUpgrades, Allocator.Temp);
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