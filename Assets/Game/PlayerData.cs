    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Netcode;
    using UnityEngine;

    [Serializable] 
    public class PlayerDataList
    {
        public List<PlayerData> playerDatas;

        public void SetupPlayerData()
        {
            playerDatas = new (NetcodeManager.MaxPlayers);
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                AddPlayerData(new PlayerData(client));
        }
        
        public bool ContainsPlayerData(ulong clientId)
        {
            foreach (PlayerData data in playerDatas)
                if (data.ClientId == clientId) return true;
            return false;
        }
        
        public PlayerData GetPlayerData(ulong clientId)
        {
            foreach (PlayerData data in playerDatas)
            {
                if (data.ClientId != clientId) continue;
                return data;
            }
            return new();
        }
        
        public void AddPlayerData(PlayerData data)
        {
            if (ContainsPlayerData(data.ClientId)) return;
            playerDatas.Add(data);
        }
        
        public void UpdatePlayerData(PlayerData data)
        {
            for (int i = 0; i < playerDatas.Count; i++)
            {
                if (playerDatas[i].ClientId != data.ClientId) continue;
                
                playerDatas[i] = data;
            }
        }
        public void RemovePlayerData(ulong clientId)
        {
            for (int i = 0; i < playerDatas.Count; i++)
            {
                if (playerDatas[i].ClientId != clientId) continue;
                
                playerDatas.RemoveAt(i);
            }
        }
        public void RemovePlayerData(PlayerData data)
        {
            playerDatas.Remove(data);
        }
    }

    [Serializable]
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong ClientId;
        
        public PlayerOuterData OuterData;
        public PlayerInGameData InGameData;
        
        public PlayerData(NetworkClient client = null)
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
    }

    [Serializable]
    public struct PlayerInGameData : INetworkSerializable
    {
        public int score;
        public ushort[] upgradesIndexArray;
        
        public void AddScore(int amount) => score += amount;
        public void RemoveScore(int amount) => score -= amount;
        public void ResetScore() => score = 0;

        public override string ToString()
        {
            return $"score: {score}, upgradesIndexArray: {string.Join(',',upgradesIndexArray)}, size: {upgradesIndexArray.Length}";
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