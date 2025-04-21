using System;
using Unity.Netcode;
using UnityEngine;

public static class CachedOwnerClientData
{
    public static ScriptableUpgrade[] upgrades { get; private set; }
    public static ushort score { get; private set; }
    public static ushort health { get; private set; }
    
    public static PlayerData ownerData { get; private set; }
    
    public static Action<PlayerData,PlayerData> onOwnerDataChanged;

    public static void UpdateCachedDataOwner(PlayerData playerData)
    {
        Debug.LogError("UpdateCachedDataOwner 1 CLIENTID = " + playerData.ClientId);
        if (ownerData.Equals(playerData)) return;
        
        Debug.Log("UpdateCachedDataOwner 2");
        
        score = playerData.InGameData.score;
        health = playerData.InGameData.health;
        upgrades = playerData.InGameData.GetUpgrades();
        
        onOwnerDataChanged?.Invoke(ownerData, playerData);
        ownerData = playerData;
    }
}