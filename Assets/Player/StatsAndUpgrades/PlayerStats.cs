using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    private const ushort MaxUpgrades = 20;
    private const ushort NullUpgradeIndex = 999;
    public ushort[] UpgradesIndexArray { get; private set; } = InitializeUpgradesIndexArray();
    private List<ScriptableUpgrade> _upgrades = new(); // LOCAL VARIABLE NOT NETWORKED

    private static ushort[] InitializeUpgradesIndexArray()
    {
        ushort[] upgradesIndexArray = new ushort[MaxUpgrades];
        for (ushort i = 0; i < MaxUpgrades; i++)
            upgradesIndexArray[i] = NullUpgradeIndex;
        
        return upgradesIndexArray;
    }

    // inutile
    
    // public ushort[] GetUpgradeIndexList()
    // {
    //     ushort arraySize = 0;
    //     for (ushort i = 0; i < MaxUpgrades; i++)
    //     {
    //         if (UpgradesIndexArray[i] == NullUpgradeIndex) break;
    //         arraySize++;
    //     }
    //     
    //     ushort[] upgradeList = new ushort[arraySize];
    //     for (ushort i = 0; i < arraySize; i++)
    //         upgradeList[i] = UpgradesIndexArray[i];
    //     
    //     return upgradeList;
    // }
    
    public void SetUpgrades(ushort[] upgradesIndexArray)
    {
        UpgradesIndexArray = upgradesIndexArray;
        
        _upgrades.Clear();
        foreach (ushort upgradeIndex in UpgradesIndexArray)
            _upgrades.Add(GlobalGameData.Instance.upgrades[upgradeIndex]);
    }
    public void AddUpgrade(ushort upgradeIndex)
    {
        for (ushort i = 0; i < MaxUpgrades; i++)
        {
            if (UpgradesIndexArray[i] == NullUpgradeIndex)
            {
                UpgradesIndexArray[i] = upgradeIndex;
                _upgrades.Add(GlobalGameData.Instance.upgrades[upgradeIndex]);
                break;
            }
        }
    }
    public void RemoveUpgrade(ushort upgradeIndex)
    {
        for (ushort i = 0; i < MaxUpgrades; i++)
        {
            if (UpgradesIndexArray[i] == upgradeIndex)
            {
                UpgradesIndexArray[i] = NullUpgradeIndex;
                _upgrades.Remove(GlobalGameData.Instance.upgrades[upgradeIndex]);
                break;
            }
        }
    }
    public void ClearUpgrades()
    {
        UpgradesIndexArray = InitializeUpgradesIndexArray();
        _upgrades.Clear();
    }
}

[Serializable]
public struct StatData
{
    public float Speed;
    public float JumpHeight;

    public StatData(float speed = 1, float jumpHeight = 1)
    {
        Speed = speed;
        JumpHeight = jumpHeight;
    }
    
    public static StatData operator +(StatData a, StatData b)
    {
        return new StatData(a.Speed + b.Speed, a.JumpHeight + b.JumpHeight);
    }
}