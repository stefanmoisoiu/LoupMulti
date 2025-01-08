using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradesManager : NetworkBehaviour
{
    public const ushort MaxUpgrades = 20;
    public const ushort UpgradeChoices = 3;
    
    [SerializeField] private ScriptableUpgrade[] upgrades;
    public ScriptableUpgrade[] Upgrades => upgrades;
    private readonly Dictionary<ulong,ushort[]> _playerUpgradeChoices = new();
    private readonly Dictionary<ulong,ushort> _playerUpgradeChosenChoice = new();
    
    public Action<ushort[]> OnUpgradeChoices;
    [Rpc(SendTo.SpecifiedInParams)] private void OnUpgradeChoicesClientRpc(ushort[] upgradesIndex, RpcParams @params) => OnUpgradeChoices?.Invoke(upgradesIndex);
    
    public Action<ushort> OnUpgradeChosenOwner;
    [Rpc(SendTo.SpecifiedInParams)]private void OnUpgradeChosenClientRpc(ushort upgradeIndex, RpcParams @params) => OnUpgradeChosenOwner?.Invoke(upgradeIndex);

    public ScriptableUpgrade GetUpgrade(ushort upgradeIndex)
    {
        if (upgradeIndex >= upgrades.Length) return null;
        return upgrades[upgradeIndex];
    }
    public ushort GetUpgradeIndex(ScriptableUpgrade upgrade)
    {
        return (ushort)Array.IndexOf(upgrades, upgrade);
    }
    
    

    public ushort[] DrawUpgradesIndex(int amount, PlayerData data)
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
    public void UpgradeTimeFinishedServer()
    {
        foreach (var player in NetworkManager.ConnectedClientsList)
            if (!_playerUpgradeChosenChoice.TryGetValue(player.ClientId, out var upgradeIndex))
                _playerUpgradeChosenChoice.Add(player.ClientId, 0);
        
        ApplyUpgradesToPlayersServer();
        ResetChoices();
    }

    public void ChooseUpgradesForPlayingPlayersServer()
    {
        foreach (PlayerData data in GameManager.Instance.gameData.playerGameData.Value.GetDatas())
        {
            if (data.OuterData.CurrentPlayerState != PlayerOuterData.PlayerState.Playing) continue;
            ChooseUpgradesForPlayerServer(data);
        }
    }
    
    public void ChooseUpgradesForPlayerServer(PlayerData data)
    {
        ushort[] randomUpgradesIndex;
        if (_playerUpgradeChoices.TryGetValue(data.ClientId, out var upgradesIndex))
            randomUpgradesIndex = upgradesIndex;
        else
        {
            randomUpgradesIndex = DrawUpgradesIndex(UpgradeChoices, data);
            _playerUpgradeChoices.Add(data.ClientId, randomUpgradesIndex);
        }
        OnUpgradeChoicesClientRpc(randomUpgradesIndex, data.ToRpcParams());
    }
    public void ResetChoices()
    {
        _playerUpgradeChoices.Clear();
        _playerUpgradeChosenChoice.Clear();
    }

    private void ApplyUpgradesToPlayersServer()
    { 
        PlayerData[] playerDatas = GameManager.Instance.gameData.playerGameData.Value.GetDatas();
        
        for (int i = 0; i < playerDatas.Length; i++)
        {
            ulong clientId = playerDatas[i].ClientId;
            int choice = _playerUpgradeChosenChoice[clientId];
            ushort[] availableChoices = _playerUpgradeChoices[clientId];
            ushort upgradeIndex = availableChoices[choice];
            // log
            NetcodeLogger.Instance.LogRpc($"Player {clientId} had these choices: {string.Join(", ", availableChoices)}", NetcodeLogger.ColorType.Orange);
            NetcodeLogger.Instance.LogRpc($"and chose {choice}: {GetUpgrade(upgradeIndex).UpgradeName}", NetcodeLogger.ColorType.Orange);
            
            PlayerInGameData inGameData = playerDatas[i].InGameData.AddUpgrade(upgradeIndex);
            PlayerData finalPlayerData = new PlayerData(playerDatas[i]) { InGameData = inGameData };
            
            GameManager.Instance.gameData.playerGameData.Value = GameManager.Instance.gameData.playerGameData.Value.UpdateData(finalPlayerData);
            OnUpgradeChosenClientRpc(upgradeIndex, playerDatas[i].ToRpcParams());
        }
    }
    // client
    public void ChooseUpgradeClient(ushort upgradeIndex) => UpgradeChosenServerRpc(upgradeIndex, RpcParamsExt.Instance.SenderClientID(NetworkManager.LocalClientId));

    [Rpc(SendTo.Server)]
    public void UpgradeChosenServerRpc(ushort upgradeIndex, RpcParams @params)
    {
        _playerUpgradeChosenChoice[@params.Receive.SenderClientId] = upgradeIndex;
    }
}