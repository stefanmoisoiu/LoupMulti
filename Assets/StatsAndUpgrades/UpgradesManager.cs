using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradesManager : NetworkBehaviour
{
    public const ushort MaxUpgrades = 20;
    public const ushort UpgradeChoices = 3;
    
    [SerializeField] private ScriptableUpgrade[] upgrades;
    private readonly Dictionary<ulong,ushort[]> _playerUpgradeChoices = new();
    
    public Action<ushort[]> OnUpgradeChoices;
    public Action OnUpgradeTimeFinished;
    
    public ScriptableUpgrade GetUpgrade(ushort upgradeIndex)
    {
        return upgrades[upgradeIndex];
    }
    
    public ushort[] GetDistinctRandomUpgradesIndex(int amount)
    {
        if (amount > upgrades.Length) amount = upgrades.Length;
        
        ushort[] randomUpgradesIndex = new ushort[amount];
        for (int i = 0; i < randomUpgradesIndex.Length; i++) randomUpgradesIndex[i] = ushort.MaxValue;
        
        for (int i = 0; i < amount; i++)
        {
            ushort upgrade;
            
            do upgrade = (ushort)Random.Range(0, upgrades.Length);
            while (randomUpgradesIndex.Contains(upgrade));
            
            randomUpgradesIndex[i] = upgrade;
        }
        return randomUpgradesIndex;
    }

    public void UpgradeTimeFinished()
    {
        _playerUpgradeChoices.Clear();
    }
    public void ChooseRandomPlayerUpgradesServer(PlayerData playerData)
    {
        ushort[] randomUpgradesIndex;
        if (_playerUpgradeChoices.TryGetValue(playerData.ClientId, out var upgradesIndex))
            randomUpgradesIndex = upgradesIndex;
        else
        {
            randomUpgradesIndex = GetDistinctRandomUpgradesIndex(UpgradeChoices);
            // _playerUpgradeChoices.Add(playerData.ClientId, randomUpgradesIndex);
        }
        // OnUpgradeChoices_ClientRpc(randomUpgradesIndex, playerData.ToRpcParams());
    }
    
    [Rpc(SendTo.SpecifiedInParams)] private void OnUpgradeChoices_ClientRpc(ushort[] upgradesIndex, RpcParams @params) => OnUpgradeChoices?.Invoke(upgradesIndex);
}