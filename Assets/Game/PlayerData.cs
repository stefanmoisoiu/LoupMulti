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
                if (playerDatas[i].ClientId != ulong.MaxValue && playerDatas[i].ClientId != data.ClientId) continue;
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
            InGameData = new PlayerInGameData(0,PlayerInGameData.DefaultUpgradesIndexArray());
        }
        
        public PlayerData(PlayerData copy)
        {
            ClientId = copy.ClientId;
            OuterData = new PlayerOuterData(copy.OuterData);
            InGameData = new PlayerInGameData(copy.InGameData.score, copy.InGameData.upgradesIndexArray);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref OuterData);
            serializer.SerializeValue(ref InGameData);
        }
        public bool Equals(PlayerData other)
        {
            return ClientId == other.ClientId;
        }
        public RpcParams ToRpcParams() => RpcParamsExt.Instance.SendToClientIDs(new []{ClientId}, NetworkManager.Singleton);
        
        public override string ToString()
        {
            return $"ClientId: {ClientId}\nOuterData: {OuterData}\nInGameData: {InGameData}";
        }
    }


    [Serializable]
    public struct PlayerOuterData : INetworkSerializable
    {
        public PlayerState CurrentPlayerState;
        
        public enum PlayerState
        {
            NotAssigned,
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
    }

    [Serializable]
    public struct PlayerInGameData : INetworkSerializable
    {
        public int score;
        public ushort[] upgradesIndexArray;
        
        public bool debugUpgrades;
        [ShowInInspector] [ShowIf("@debugUpgrades")] public string upgradesString => string.Join(", ", GetUpgrades().Select(u => u.UpgradeName));
        
        public void AddScore(int amount) => score += amount;
        public void RemoveScore(int amount) => score -= amount;
        public void ResetScore() => score = 0;

        public override string ToString()
        {
            ScriptableUpgrade[] upgrades = GetUpgrades();
            return $"Score: {score}\nUpgrades: {string.Join(", ", upgrades.Select(u => u))}";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref score);
            
            upgradesIndexArray ??= new ushort[UpgradesManager.MaxUpgrades];
            for (int i = 0; i < upgradesIndexArray.Length; i++)
                serializer.SerializeValue(ref upgradesIndexArray[i]);
        }
        
        public PlayerInGameData(int Score = 0, ushort[] UpgradesIndexArray = null)
        {
            score = Score;
            
            upgradesIndexArray = UpgradesIndexArray;
            debugUpgrades = false;
            
            if(UpgradesIndexArray == null) Debug.LogError("initialise upgradesIndexArray ca marche pas sinon");
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
                upgrades.Add(GameManager.Instance.upgradesManager.GetUpgrade(upgradesIndexArray[i]));
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
    }