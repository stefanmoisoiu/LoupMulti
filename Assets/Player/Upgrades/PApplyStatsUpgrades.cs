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
        manager.UpgradesManager.OnUpgradeChosenOwner += UpgradeAdded;
    }

    protected override void DisableOnlineOwner()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.UpgradesManager.OnUpgradeChosenOwner -= UpgradeAdded;
    }

    protected override void UpdateOnlineOwner()
    {
        ScriptableUpgrade[] ownedUpgrades = CachedOwnerClientData.upgrades;
        
        if (ownedUpgrades == null) return;
        if (ownedUpgrades.Length == 0) return;
        
        foreach (ScriptableUpgrade upgrade in ownedUpgrades) upgrade.Update();
    }

    private void UpgradeAdded(ushort upgradeIndex)
    {
        ScriptableUpgrade upgrade = GameManager.Instance.UpgradesManager.GetUpgrade(upgradeIndex);
        
        upgrade.Update();
        
        movement.AccelerationModifier.AddModifier(upgrade.GetAccelerationModifier());
        movement.MaxSpeedModifier.AddModifier(upgrade.GetMaxSpeedModifier());
        jump.JumpHeightModifier.AddModifier(upgrade.GetJumpHeightModifier());
        stamina.StaminaRecoverRateModifier.AddModifier(upgrade.GetStaminaRecoveryModifier());
        stamina.StaminaPerPartModifier.AddModifier(upgrade.GetStaminaPerPartModifier());
        stamina.AddedStaminaPartsModifier.AddModifier(upgrade.GetAddedStaminaParts());
    }
}