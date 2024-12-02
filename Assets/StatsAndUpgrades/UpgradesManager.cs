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
    private readonly Dictionary<ulong,ushort> _playerUpgradeChosenChoice = new();
    
    public Action<ushort[]> OnUpgradeChoices;
    
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

    // server
    public void UpgradeTimeFinished()
    {
        foreach (var player in NetworkManager.ConnectedClientsList)
            if (!_playerUpgradeChosenChoice.TryGetValue(player.ClientId, out var upgradeIndex))
                _playerUpgradeChosenChoice.Add(player.ClientId, 0);
        
        LogUpgradeChoices();
    }
    public void ChooseRandomPlayerUpgradesServer(PlayerData playerData)
    {
        ushort[] randomUpgradesIndex;
        if (_playerUpgradeChoices.TryGetValue(playerData.ClientId, out var upgradesIndex))
            randomUpgradesIndex = upgradesIndex;
        else
        {
            randomUpgradesIndex = GetDistinctRandomUpgradesIndex(UpgradeChoices);
            _playerUpgradeChoices.Add(playerData.ClientId, randomUpgradesIndex);
        }
        OnUpgradeChoices_ClientRpc(randomUpgradesIndex, playerData.ToRpcParams());
    }
    public void ResetChoices()
    {
        _playerUpgradeChoices.Clear();
        _playerUpgradeChosenChoice.Clear();
    }
    private void LogUpgradeChoices()
    {
        foreach (var clientID in _playerUpgradeChosenChoice)
            GameManager.Instance.LogRpc($"Player {clientID.Key} chose upgrade {clientID.Value}", GameManager.LogType.UpgradeInfo);
    }
    // client
    public void ChooseUpgrade(ushort upgradeIndex) => UpgradeChosenServerRpc(upgradeIndex, RpcParamsExt.Instance.SenderClientID(NetworkManager.LocalClientId));
    [Rpc(SendTo.Server)]
    public void UpgradeChosenServerRpc(ushort upgradeIndex, RpcParams @params)
    {
        _playerUpgradeChosenChoice[@params.Receive.SenderClientId] = upgradeIndex;
    }
    [Rpc(SendTo.SpecifiedInParams)] private void OnUpgradeChoices_ClientRpc(ushort[] upgradesIndex, RpcParams @params) => OnUpgradeChoices?.Invoke(upgradesIndex);
}