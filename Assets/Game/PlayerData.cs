    using System;
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Netcode;
    
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
            Spectating,
        }

        public PlayerData(NetworkClient client = null)
        {
            ClientId = client?.ClientId ?? ulong.MaxValue;
            CurrentPlayerState = PlayerState.NotAssigned;
            InGameData = new PlayerInGameData();
        }

        public void ResetGameData()
        {
            InGameData = new();
        }
        
        public void SetState(PlayerState state) => CurrentPlayerState = state;
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
    }

    [Serializable]
    public struct PlayerInGameData : INetworkSerializable
    {
        public int score;
        public ushort[] upgradesIndexArray;
        private ushort upgradesLength;
        
        public void AddScore(int amount) => score += amount;
        public void RemoveScore(int amount) => score -= amount;
        public void ResetScore() => score = 0;

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
            upgradesLength = UpgradesManager.MaxUpgrades;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref score);
            
            if (serializer.IsReader)
            {
                serializer.SerializeValue(ref upgradesLength);
                upgradesIndexArray = new ushort[upgradesLength];
                
                for (int i = 0; i < upgradesLength; i++)
                    serializer.SerializeValue(ref upgradesIndexArray[i]);
            }
            else
            {
                upgradesLength = (ushort)upgradesIndexArray.Length;
                serializer.SerializeValue(ref upgradesLength);
                
                for (int i = 0; i < upgradesIndexArray.Length; i++)
                    serializer.SerializeValue(ref upgradesIndexArray[i]);
            }
        }
    }