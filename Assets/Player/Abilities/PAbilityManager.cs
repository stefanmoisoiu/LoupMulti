using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PAbilityManager : PNetworkBehaviour
{
    [SerializeField] private PlayerAbility[] abilities;

    protected override void StartOnlineOwner()
    {
        if(GameManager.Instance != null)
        {
            UpdateAllAbilityStates();
            GameManager.Instance.UpgradesManager.OnUpgradeChosenOwner += UpgradeAddedEnableAbility;
        }
        else
        {
            GameManager.OnCreated += gm =>
            {
                gm.UpgradesManager.OnUpgradeChosenOwner += UpgradeAddedEnableAbility;
                UpdateAllAbilityStates();
            };
        }
    }

    private void UpdateAllAbilityStates()
    {
        ScriptableUpgrade[] ownedUpgrades = GameManager.Instance.GameData.PlayerGameData.GetDataOrDefault(OwnerClientId).InGameData.GetUpgrades();
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