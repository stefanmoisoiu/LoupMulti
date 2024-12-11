using UnityEngine;

public class PApplyStatsUpgrades : PNetworkBehaviour
{
    [SerializeField] private PMovement movement;
    [SerializeField] private PJump jump;
    [SerializeField] private PStamina stamina;
    
    
    protected override void StartOnlineOwner()
    {
        GameManager.OnCreated += GameManagerEnabled;
    }

    private void GameManagerEnabled(GameManager manager)
    {
        manager.upgradesManager.OnUpgradeChosenOwner += UpgradeAdded;
    }

    protected override void DisableOnlineOwner()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.upgradesManager.OnUpgradeChosenOwner -= UpgradeAdded;
    }

    protected override void UpdateOnlineOwner()
    {
        Debug.Log("a changer");
        ScriptableUpgrade[] upgrades = GameManager.Instance.gameData.playerGameData.Value.GetDataOrDefault(OwnerClientId).InGameData.GetUpgrades();
        
        if (upgrades == null) return;
        if (upgrades.Length == 0) return;
        
        foreach (ScriptableUpgrade upgrade in upgrades) UpdateUpgradeStats(upgrade);
    }
    
    private void UpdateUpgradeStats(ScriptableUpgrade upgrade)
    {
        upgrade.Update();
    }

    private void UpgradeAdded(ushort upgradeIndex)
    {
        ScriptableUpgrade upgrade = GameManager.Instance.upgradesManager.GetUpgrade(upgradeIndex);
        
        upgrade.Update();
        
        movement.AccelerationModifier.AddModifier(upgrade.GetAccelerationModifier());
        movement.MaxSpeedModifier.AddModifier(upgrade.GetMaxSpeedModifier());
        jump.JumpHeightModifier.AddModifier(upgrade.GetJumpHeightModifier());
        stamina.StaminaRecoverRateModifier.AddModifier(upgrade.GetStaminaRecoveryModifier());
        stamina.StaminaPerPartModifier.AddModifier(upgrade.GetStaminaPerPartModifier());
        stamina.AddedStaminaPartsModifier.AddModifier(upgrade.GetAddedStaminaParts());
    }
}