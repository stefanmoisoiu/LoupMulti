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
}