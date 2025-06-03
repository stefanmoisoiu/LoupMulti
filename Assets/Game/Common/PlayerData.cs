

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
        public ulong ClientId;
        
        public OuterData outerData;
        public InGameData inGameData;
        
        public PlayerData(NetworkClient client)
        {
            ClientId = client?.ClientId ?? ulong.MaxValue;
            outerData = new OuterData(client);
            inGameData = new InGameData(
                perksIndexArray:InGameData.DefaultPerksIndexArray(),
                shopItemsIndexArray: InGameData.DefaultShopItemsIndexArray());
        }
        public PlayerData(PlayerData copy)
        {
            ClientId = copy.ClientId;
            outerData = new OuterData(copy.outerData);
            inGameData = new InGameData(copy.inGameData.health, copy.inGameData.perksIndexArray, copy.inGameData.shopItemsIndexArray, copy.inGameData.resources);
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref outerData);
            serializer.SerializeValue(ref inGameData);
        }
        public RpcParams SendRpcTo() => RpcParamsExt.Instance.SendToClientIDs(new []{ClientId});
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
        public ushort[] perksIndexArray;
        public ushort[] shopItemsIndexArray;
        public PlayerResourceData resources;
        [ShowInInspector] public string perksString => string.Join(", ", GetPerks().Select(u => u.PerkName));

        public override string ToString()
        {
            PerkData[] perks = GetPerks();
            return $"Health: {health}\nResources: {resources}\nPerks: {string.Join(", ", perks.Select(u => u))}";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref resources);
            serializer.SerializeValue(ref health);
            
            perksIndexArray ??= new ushort[GameSettings.Instance.MaxPerks];
            
            for (int i = 0; i < perksIndexArray.Length; i++)
                serializer.SerializeValue(ref perksIndexArray[i]);
            
            
            shopItemsIndexArray ??= new ushort[GameSettings.Instance.MaxShopItems];
            
            for (int i = 0; i < shopItemsIndexArray.Length; i++)
                serializer.SerializeValue(ref shopItemsIndexArray[i]);
        }
        
        public InGameData(ushort health = GameSettings.PlayerMaxHealth, ushort[] perksIndexArray = null, ushort[] shopItemsIndexArray = null, PlayerResourceData resources = new())
        {
            this.health = health;
            this.perksIndexArray = perksIndexArray;
            this.shopItemsIndexArray = shopItemsIndexArray;
            this.resources = resources;
            
            if(perksIndexArray == null) Debug.LogError("initialise perksIndexArray ca marche pas sinon");
            if (shopItemsIndexArray == null) Debug.LogError("initialise shopItemsIndexArray ca marche pas sinon");
        }

        public InGameData(InGameData copy)
        {
            health = copy.health;
            resources = copy.resources;
            perksIndexArray = copy.perksIndexArray;
            shopItemsIndexArray = copy.shopItemsIndexArray;
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
        
        public InGameData AddPerk(ushort perkIndex)
        {
            perksIndexArray ??= DefaultPerksIndexArray();
            for (int i = 0; i < perksIndexArray.Length; i++)
            {
                if (perksIndexArray[i] != ushort.MaxValue) continue;
                
                perksIndexArray[i] = perkIndex;
                return this;
            }
            return this;
        }
        public PerkData[] GetPerks()
        {
            if (perksIndexArray == null || perksIndexArray.Length == 0) return new PerkData[] {};

            List<PerkData> perks = new();

            for (int i = 0; i < GameSettings.Instance.MaxPerks; i++)
            {
                if (perksIndexArray[i] == ushort.MaxValue) continue;
                perks.Add(PerkList.Instance.GetPerk(perksIndexArray[i]));
            }
            return perks.ToArray();
        }
        public static ushort[] DefaultPerksIndexArray()
        { 
            ushort[] res = new ushort[GameSettings.Instance.MaxPerks];
            for (int i = 0; i < GameSettings.Instance.MaxPerks; i++) res[i] = ushort.MaxValue;
            return res;
        } 
        
        public InGameData AddShopItem(ushort shopItemIndex)
        {
            shopItemsIndexArray ??= new ushort[GameSettings.Instance.MaxShopItems];
            for (int i = 0; i < shopItemsIndexArray.Length; i++)
            {
                if (shopItemsIndexArray[i] != ushort.MaxValue) continue;
                
                shopItemsIndexArray[i] = shopItemIndex;
                return this;
            }
            return this;
        }
        
        public ShopItemData[] GetShopItems()
        {
            if (shopItemsIndexArray == null || shopItemsIndexArray.Length == 0) return new ShopItemData[] {};
            
            List<ShopItemData> shopItems = new();

            for (int i = 0; i < GameSettings.Instance.MaxShopItems; i++)
            {
                if (shopItemsIndexArray[i] == ushort.MaxValue) continue;
                shopItems.Add(ShopItemList.Instance.GetShopItem(shopItemsIndexArray[i]));
            }
            return shopItems.ToArray();
        }
        
        public static ushort[] DefaultShopItemsIndexArray()
        { 
            ushort[] res = new ushort[GameSettings.Instance.MaxShopItems];
            for (int i = 0; i < GameSettings.Instance.MaxShopItems; i++) res[i] = ushort.MaxValue;
            return res;
        }
        
        public bool Equals(InGameData other)
        {
            return health == other.health &&
                   resources.Equals(other.resources) &&
                   Equals(perksIndexArray, other.perksIndexArray) &&
                   Equals(shopItemsIndexArray, other.shopItemsIndexArray);
        }
        public override bool Equals(object obj)
        {
            return obj is InGameData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(health, resources, perksIndexArray);
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