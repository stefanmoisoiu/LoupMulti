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
    
    public ushort[] GetDistinctRandomUpgradesIndex(int amount)
    {
        ushort[] randomUpgradesIndex = new ushort[amount];
        for (int i = 0; i < amount; i++)
        {
            ushort upgrade;
            
            do upgrade = (ushort)Random.Range(0, upgrades.Length);
            while (randomUpgradesIndex.Contains(upgrade));
            
            randomUpgradesIndex[i] = upgrade;
        }
        return randomUpgradesIndex;
    }

    public void ChoosePlayerUpgradesServer(PlayerData playerData)
    {
        
    }
}