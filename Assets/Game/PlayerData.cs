    using System;
    using Unity.Netcode;
    
    [Serializable] 
    public struct PlayerDataList : INetworkSerializable
    {
        public PlayerData[] playerDatas;

        public PlayerDataList SetupPlayerData()
        {
            playerDatas = new PlayerData[NetcodeManager.MaxPlayers];
            GameManager.instance.LogRpc(playerDatas.Length + " player data slots created");
            GameManager.instance.LogRpc("Setting up player data");
            GameManager.instance.LogRpc(NetworkManager.Singleton.ConnectedClientsList.Count + " players connected");
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                PlayerData data = new PlayerData(client);
                AddPlayerData(data);
                GameManager.instance.LogRpc("Player " + data.clientId + " added to player data");
            }

            return this;
        }
        
        public PlayerData GetPlayerData(ulong clientId)
        {
            foreach (PlayerData data in playerDatas)
            {
                if (!data.assignedToPlayer) continue;
                if (data.clientId != clientId) continue;
                return data;
            }
            return new();
        }
        
        public PlayerDataList AddPlayerData(PlayerData data)
        {
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (playerDatas[i].assignedToPlayer) continue;
            
                playerDatas[i] = data;
                return this;
            }
            return this;
        }
        
        public PlayerDataList RemovePlayerData(PlayerData data)
        {
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (playerDatas[i].clientId != data.clientId) continue;
                
                playerDatas[i].assignedToPlayer = false;
            }
            return this;
        }
        
        public PlayerDataList UpdatePlayerData(PlayerData data)
        {
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (!playerDatas[i].assignedToPlayer) continue;
                if (playerDatas[i].clientId != data.clientId) continue;
                
                playerDatas[i] = data;
            }
            return this;
        }
        public PlayerDataList RemovePlayerData(ulong clientId)
        {
            for (int i = 0; i < playerDatas.Length; i++)
            {
                if (playerDatas[i].assignedToPlayer) continue;
                if (playerDatas[i].clientId != clientId) continue;
                
                playerDatas[i].assignedToPlayer = false;
            }
            return this;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // foreach (PlayerData playerData in playerDatas)
            //     playerData.NetworkSerialize(serializer);
            serializer.SerializeValue(ref playerDatas);
        }
    }
    
    [Serializable]
    public struct PlayerData :  INetworkSerializable
    {
        
        public ulong clientId;
        public PlayerState playerState;
        public PlayerInGameData inGameData;

        public bool assignedToPlayer;
        
        public enum PlayerState
        {
            NotAssigned,
            Playing,
            Spectating,
        }

        public PlayerData(NetworkClient client = null)
        {
            if (client != null)
            {
                clientId = client.ClientId;
                assignedToPlayer = true;
            }
            else
            {
                assignedToPlayer = false;
                clientId = ulong.MaxValue;
            }
            
            playerState = PlayerState.NotAssigned;
            inGameData = new PlayerInGameData();
        }

        public void ResetGameData()
        {
            inGameData = new();
        }
        
        public void SetState(PlayerState state) => playerState = state;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref playerState);
            inGameData.NetworkSerialize(serializer);
            serializer.SerializeValue(ref assignedToPlayer);
        }
    }

    [Serializable]
    public struct PlayerInGameData : INetworkSerializable
    {
        public int score;
        
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

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref score);
        }
    }