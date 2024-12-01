using System.Linq;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class UpgradesManager : NetworkBehaviour
{
    public const ushort MaxUpgrades = 20;
    [SerializeField] private ScriptableUpgrade[] upgrades;
    
    public ScriptableUpgrade GetUpgrade(ushort upgradeIndex)
    {
        return upgrades[upgradeIndex];
    }
    
    public ScriptableUpgrade[] GetDistinctRandomUpgrades(int amount)
    {
        ScriptableUpgrade[] randomUpgrades = new ScriptableUpgrade[amount];
        for (int i = 0; i < amount; i++)
        {
            ScriptableUpgrade upgrade;
            
            do upgrade = upgrades[Random.Range(0, upgrades.Length)];
            while (randomUpgrades.Contains(upgrade));
            
            randomUpgrades[i] = upgrade;
        }
        return randomUpgrades;
    }
}