using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradesManager : NetworkBehaviour
{
    public const int MaxUpgrades = 20;
    public const ushort UpgradeChoices = 3;
    
    [SerializeField] private ScriptableUpgrade[] upgrades;
    public ScriptableUpgrade[] Upgrades => upgrades;
    public readonly Dictionary<ulong,ushort[]> PlayerAvailableUpgrades = new();
    public readonly Dictionary<ulong,ushort> PlayerUpgradeChoice = new();
    
    public static event Action<ushort[]> OnUpgradeChoicesAvailable;
    [Rpc(SendTo.SpecifiedInParams)] private void OnUpgradeChoicesAvailableClientRpc(ushort[] upgradesIndex, RpcParams @params) => OnUpgradeChoicesAvailable?.Invoke(upgradesIndex);
    
    public static event Action<ushort> OnUpgradeChosenOwner;
    [Rpc(SendTo.SpecifiedInParams)]private void OnUpgradeChosenClientRpc(ushort upgradeIndex, RpcParams @params) => OnUpgradeChosenOwner?.Invoke(upgradeIndex);

    public ScriptableUpgrade GetUpgrade(ushort upgradeIndex)
    {
        if (upgradeIndex >= upgrades.Length) return null;
        return upgrades[upgradeIndex];
    }
    public ushort GetUpgrade(ScriptableUpgrade upgrade)
    {
        return (ushort)Array.IndexOf(upgrades, upgrade);
    }
    
    // server
    public void ApplyUpgrades()
    {
        // If player did not choose, we give them the first upgrade
        GameData gameData = GameManager.Instance.GameData;
        foreach (ulong clientId in gameData.PlayerDataManager.GetKeys())
        {
            if (!PlayerUpgradeChoice.TryGetValue(clientId, out var upgradeChoiceIndex)) // premier choix = 0, etc
                PlayerUpgradeChoice.Add(clientId, 0);

            ushort[] availableChoices = PlayerAvailableUpgrades[clientId];
            ushort chosenUpgrade = availableChoices[upgradeChoiceIndex];

            NetcodeLogger.Instance.LogRpc($"Player {clientId} had these choices: {string.Join(", ", availableChoices)}", NetcodeLogger.LogType.Upgrades);
            NetcodeLogger.Instance.LogRpc($"and chose {GetUpgrade(upgradeChoiceIndex).UpgradeName}", NetcodeLogger.LogType.Upgrades);

            PlayerData data = gameData.PlayerDataManager[clientId];
            gameData.PlayerDataManager[clientId] = new(data) {InGameData = data.InGameData.AddUpgrade(chosenUpgrade)};
            
            OnUpgradeChosenClientRpc(upgradeChoiceIndex, data.SendRpcTo());
        }
        
        ResetChoices();
    }

    public void ChooseUpgradesForPlayersServer()
    {
        foreach (PlayerData data in GameManager.Instance.GameData.PlayerDataManager.GetValues())
        {
            PlayerOuterData.PlayingState state = data.OuterData.playingState;
            if (state != PlayerOuterData.PlayingState.Playing) continue;
            PlayerAvailableUpgrades[data.ClientId] = DrawUpgrades(UpgradeChoices, data);
            OnUpgradeChoicesAvailableClientRpc(PlayerAvailableUpgrades[data.ClientId], data.SendRpcTo());
        }
    }
    public ushort[] DrawUpgrades(int amount, PlayerData data)
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
    
    public void ResetChoices()
    {
        PlayerAvailableUpgrades.Clear();
        PlayerUpgradeChoice.Clear();
    }
    
    
    // client
    public void ChooseUpgradeClient(ushort upgradeIndex) => ClientChoseUpgradeServerRpc(upgradeIndex, RpcParamsExt.Instance.SenderClientID(NetworkManager.LocalClientId));

    [Rpc(SendTo.Server)]
    public void ClientChoseUpgradeServerRpc(ushort upgradeIndex, RpcParams @params)
    {
        PlayerUpgradeChoice[@params.Receive.SenderClientId] = upgradeIndex;
    }
}