using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PAbilityManager : PNetworkBehaviour
{
    [SerializeField] private PlayerAbility[] abilities;

    protected override void StartOnlineOwner()
    {
        UpgradesManager.OnUpgradeChosenOwner += UpgradeAddedEnableAbility;
    }

    protected override void DisableAnyOwner()
    {
        UpgradesManager.OnUpgradeChosenOwner -= UpgradeAddedEnableAbility;
    }

    private void UpdateAllAbilityStates()
    {
        Debug.LogWarning("Get owned upgrades pas cached!!");
        if (GameManager.Instance != null) return;
        Debug.LogError(GameManager.Instance.GameData);
        Debug.LogError(PlayerDataManager.Instance);
        if (!PlayerDataManager.Instance.TryGetValue(NetworkManager.LocalClientId,
                out PlayerData pd)) return;
        ScriptableUpgrade[] ownedUpgrades = pd.InGameData.GetUpgrades();
        foreach (ScriptableUpgrade upgrade in GameManager.Instance.UpgradesManager.Upgrades)
        {
            if (upgrade.Type != ScriptableUpgrade.UpgradeType.Active) continue;
            
            PNetworkAbility ability = GetAbility(upgrade.ActiveAbility);
            if (ownedUpgrades.Contains(upgrade)) ability.EnableAbility();
            else ability.DisableAbility();
        }
    }
    private void UpgradeAddedEnableAbility(ushort newUpgradeIndex)
    {
        ScriptableUpgrade newUpgrade = GameManager.Instance.UpgradesManager.GetUpgrade(newUpgradeIndex);
        if (newUpgrade.Type != ScriptableUpgrade.UpgradeType.Active) return;
        
        PNetworkAbility ability = GetAbility(newUpgrade.ActiveAbility);
        ability.EnableAbility();
    }

    public PNetworkAbility GetAbility(Ability ability) => abilities.First(a => a.ability == ability).script;

    public void EnableAbility(Ability ability)
    {
        PNetworkAbility script = GetAbility(ability);
        if (script.AbilityEnabled) return;
        script.EnableAbility();
    }
    public void DisableAbility(Ability ability)
    {
        PNetworkAbility script = GetAbility(ability);
        if (!script.AbilityEnabled) return;
        script.DisableAbility();
    }
    [Serializable]
    public struct PlayerAbility
    {

        public Ability ability;
        public PNetworkAbility script;
    }
}