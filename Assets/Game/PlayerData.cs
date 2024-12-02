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
            {
                PlayerData data = new PlayerData(client);
                AddPlayerData(data);
            }
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
        public PlayerState CurrentPlayerState;
        public PlayerInGameData InGameData;
        
        public enum PlayerState
        {
            NotAssigned,
            Playing,
            SpectatingGame,
            SpectatingUntilNextRound
        }

        public PlayerData(NetworkClient client = null)
        {
            ClientId = client?.ClientId ?? ulong.MaxValue;
            CurrentPlayerState = PlayerState.NotAssigned;
            InGameData = new PlayerInGameData();
        }

        public PlayerData ResetGameData()
        {
            InGameData = new();
            return this;
        }
        
        public PlayerData SetState(PlayerState state)
        {
            CurrentPlayerState = state;
            return this;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref CurrentPlayerState);
            InGameData.NetworkSerialize(serializer);
        }

        public bool Equals(PlayerData other)
        {
            return ClientId == other.ClientId;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, (int)CurrentPlayerState, InGameData);
        }
        
        public RpcParams ToRpcParams() => RpcParamsExt.Instance.SendToClientIDs(new []{ClientId}, NetworkManager.Singleton);
    }

    [Serializable]
    public struct PlayerInGameData : INetworkSerializable
    {
        public int score;
        public ushort[] upgradesIndexArray;
        public ScriptableUpgrade[] upgrades { get; private set; }
        
        public void AddScore(int amount) => score += amount;
        public void RemoveScore(int amount) => score -= amount;
        public void ResetScore() => score = 0;
        
        private string UpgradesText()
        {
            string text = "";
            foreach (ScriptableUpgrade upgrade in upgrades)
            {
                if (upgrade == null) continue;
                text += upgrade.UpgradeName + "\n";
            }
            return text;
        }
        [ShowInInspector] private string _upgradesText => UpgradesText();

        public bool Equals(PlayerInGameData other)
        {
            return score == other.score;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerInGameData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return score;
        }
        
        public PlayerInGameData(int score = 0)
        {
            this.score = score;
            upgradesIndexArray = new ushort[UpgradesManager.MaxUpgrades];
            upgrades = new ScriptableUpgrade[UpgradesManager.MaxUpgrades];
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref score);
            
            upgradesIndexArray ??= new ushort[UpgradesManager.MaxUpgrades];
            serializer.SerializeValue(ref upgradesIndexArray);
        }
        
        public void AddUpgrade(ushort upgradeIndex)
        {
            for (int i = 0; i < upgradesIndexArray.Length; i++)
            {
                if (upgradesIndexArray[i] != 0) continue;
                
                upgradesIndexArray[i] = (ushort)(upgradeIndex + 1);
                UpdateUpgradesArray();
                return;
            }
        }
        private void UpdateUpgradesArray()
        {
            upgrades ??= new ScriptableUpgrade[UpgradesManager.MaxUpgrades];
            
            for (int i = 0; i < upgradesIndexArray.Length; i++)
            {
                if (upgradesIndexArray[i] == 0)
                {
                    upgrades[i] = null;
                    continue;
                }
                ScriptableUpgrade upgrade = GameManager.Instance.upgradesManager.GetUpgrade((ushort)(upgradesIndexArray[i] - 1));
                upgrades[i] = upgrade;
            }
        }
    }