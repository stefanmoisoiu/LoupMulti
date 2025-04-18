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
        var owned = new HashSet<ushort>(data.InGameData.upgradesIndexArray);
        List<ushort> availableUpgrades = Enumerable.Range(0, upgrades.Length)
            .Select(i => (ushort)i)
            .Where(i => !owned.Contains(i))
            .ToList();

        return GetDistinctRandomUpgradesIndex(availableUpgrades.ToArray(), amount);
    }
    
    private ushort[] GetDistinctRandomUpgradesIndex(ushort[] availableUpgrades, int amount)
    {
        amount = Mathf.Min(amount, availableUpgrades.Length);
        return availableUpgrades.OrderBy(_ => Random.value).Take(amount).ToArray();
    }

    // server
    public void UpgradeTimeFinishedServer()
    {
        // If player did not choose, we give them the first upgrade
        foreach (PlayerData data in GameManager.Instance.gameData.playerGameData.Value.GetDatas())
            if (!_playerUpgradeChosenChoice.TryGetValue(data.ClientId, out var upgradeIndex))
                _playerUpgradeChosenChoice.Add(data.ClientId, 0);
        
        ApplyUpgradesToPlayersServer();
        ResetChoices();
    }

    public void ChooseUpgradesForPlayingPlayersServer()
    {
        foreach (PlayerData data in GameManager.Instance.gameData.playerGameData.Value.GetDatas())
        {
            PlayerOuterData.PlayerState state = data.OuterData.CurrentPlayerState;
            if (state is PlayerOuterData.PlayerState.Playing or PlayerOuterData.PlayerState.Disconnected)
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
        foreach (var playerData in GameManager.Instance.gameData.playerGameData.Value.GetDatas())
        {
            ulong clientId = playerData.ClientId;

            if (!_playerUpgradeChosenChoice.TryGetValue(clientId, out var choice))
                choice = 0; // défaut si absent

            if (!_playerUpgradeChoices.TryGetValue(clientId, out var availableChoices) || availableChoices.Length <= choice)
            {
                Debug.LogError($"Invalid upgrade choice for player {clientId}");
                continue; // Sécurité
            }

            ushort upgradeIndex = availableChoices[choice];

            NetcodeLogger.Instance.LogRpc($"Player {clientId} had these choices: {string.Join(", ", availableChoices)}", NetcodeLogger.ColorType.Orange);
            NetcodeLogger.Instance.LogRpc($"and chose {choice}: {GetUpgrade(upgradeIndex).UpgradeName}", NetcodeLogger.ColorType.Orange);
            
            PlayerInGameData inGameData = playerData.InGameData.AddUpgrade(upgradeIndex);
            PlayerData finalPlayerData = new PlayerData(playerData) { InGameData = inGameData };
            
            GameManager.Instance.gameData.playerGameData.Value = GameManager.Instance.gameData.playerGameData.Value.UpdateData(finalPlayerData);
            OnUpgradeChosenClientRpc(upgradeIndex, playerData.ToRpcParams());
        }
    }

    
    // client
    public void ChooseUpgradeClient(ushort upgradeIndex) => ClientChoseUpgradeServerRpc(upgradeIndex, RpcParamsExt.Instance.SenderClientID(NetworkManager.LocalClientId));

    [Rpc(SendTo.Server)]
    public void ClientChoseUpgradeServerRpc(ushort upgradeIndex, RpcParams @params)
    {
        _playerUpgradeChosenChoice[@params.Receive.SenderClientId] = upgradeIndex;
    }
}