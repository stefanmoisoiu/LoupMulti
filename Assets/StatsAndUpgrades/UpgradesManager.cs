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

    public List<ScriptableUpgrade> cachedClientUpgrades;
    
    public Action<ushort[]> OnUpgradeChoices;
    
    public Action<ushort> OnUpgradeChosenOwner;
    [Rpc(SendTo.SpecifiedInParams)]private void OnUpgradeChosenClientRpc(ushort upgradeIndex, RpcParams @params) => OnUpgradeChosenOwner?.Invoke(upgradeIndex);

    private void Start()
    {
        SetupCachedClientUpgrades();
    }
    
    private void SetupCachedClientUpgrades()
    {
        PlayerData myPlayerData = GameManager.Instance.gameData.myPlayerData;
        cachedClientUpgrades = myPlayerData.InGameData.GetUpgrades().ToList();
        
        OnUpgradeChosenOwner += UpdateCachedClientUpgrades;
    }

    private void UpdateCachedClientUpgrades(ushort upgradeIndex)
    {
        cachedClientUpgrades.Add(GetUpgrade(upgradeIndex));
    }

    public ScriptableUpgrade GetUpgrade(ushort upgradeIndex)
    {
        if (upgradeIndex >= upgrades.Length) return null;
        return upgrades[upgradeIndex];
    }
    public ushort GetUpgradeIndex(ScriptableUpgrade upgrade)
    {
        return (ushort)Array.IndexOf(upgrades, upgrade);
    }

    public ushort[] GetUpgradesIndex(int amount, PlayerData data)
    {
        ushort[] ownedUpgrades = data.InGameData.upgradesIndexArray;
        ScriptableUpgrade[] ownedUpgradesScriptable = data.InGameData.GetUpgrades();
        List<ushort> availableUpgrades = new();
        
        for (int i = 0; i < upgrades.Length; i++)
        {
            // ScriptableUpgrade upgrade = upgrades[i];
            if (ownedUpgrades != null && ownedUpgrades.Contains((ushort)i))
            {
                
            }
            else
            {
                availableUpgrades.Add((ushort)i);
            }
        }

        return GetDistinctRandomUpgradesIndex(availableUpgrades.ToArray(), amount);
    }
    
    public ushort[] GetDistinctRandomUpgradesIndex(ushort[] availableUpgrades, int amount)
    {
        if (amount > availableUpgrades.Length) amount = availableUpgrades.Length;
        
        ushort[] randomUpgradesIndex = new ushort[amount];
        for (int i = 0; i < randomUpgradesIndex.Length; i++) randomUpgradesIndex[i] = ushort.MaxValue;
        
        for (int i = 0; i < amount; i++)
        {
            ushort upgrade;
            
            do upgrade = (ushort)Random.Range(0, availableUpgrades.Length);
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
        
        ApplyUpgradesToPlayers();
    }
    public void ChooseRandomPlayerUpgradesServer(PlayerData data)
    {
        ushort[] randomUpgradesIndex;
        if (_playerUpgradeChoices.TryGetValue(data.ClientId, out var upgradesIndex))
            randomUpgradesIndex = upgradesIndex;
        else
        {
            randomUpgradesIndex = GetUpgradesIndex(UpgradeChoices, data);
            _playerUpgradeChoices.Add(data.ClientId, randomUpgradesIndex);
        }
        OnUpgradeChoices_ClientRpc(randomUpgradesIndex, data.ToRpcParams());
    }
    public void ResetChoices()
    {
        _playerUpgradeChoices.Clear();
        _playerUpgradeChosenChoice.Clear();
    }

    private void ApplyUpgradesToPlayers()
    { 
        List<PlayerData> playerDatas = GameManager.Instance.gameData.ServerSidePlayerDataList.playerDatas;
        
        for (int i = 0; i < playerDatas.Count; i++)
        {
            ulong clientId = playerDatas[i].ClientId;
            int choice = _playerUpgradeChosenChoice[clientId];
            ushort[] availableChoices = _playerUpgradeChoices[clientId];
            ushort upgradeIndex = availableChoices[choice];
            // log
            GameManager.Instance.LogRpc($"Player {clientId} had these choices: {string.Join(", ", availableChoices)}", GameManager.LogType.UpgradeInfo);
            GameManager.Instance.LogRpc($"and chose {choice}: {GetUpgrade(upgradeIndex).UpgradeName}", GameManager.LogType.UpgradeInfo);
            
            PlayerInGameData inGameData = playerDatas[i].InGameData.AddUpgrade(upgradeIndex);
            PlayerData finalPlayerData = new PlayerData(playerDatas[i]) { InGameData = inGameData };
            
            GameManager.Instance.gameData.ServerSidePlayerDataList.UpdatePlayerData(finalPlayerData);
            
            OnUpgradeChosenClientRpc(upgradeIndex, playerDatas[i].ToRpcParams());
        }
        GameManager.Instance.gameData.UpdateEntireClientPlayerData_ClientRpc(playerDatas.ToArray(), RpcParamsExt.Instance.SendToAllClients(NetworkManager.Singleton));
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