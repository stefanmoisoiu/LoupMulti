using UnityEngine;

public class PApplyStatsUpgrades : PNetworkBehaviour
{
    [SerializeField] private PMovement movement;
    private PMovement.MoveSpeedModifiers _moveSpeedModifiers = new();
    
    protected override void StartOnlineOwner()
    {
        movement.AddMoveSpeedModifier(_moveSpeedModifiers);
    }
    protected override void UpdateOnlineOwner()
    {
        ScriptableUpgrade[] upgrades = GameManager.instance.gameData.myPlayerData.InGameData.upgrades;
        
        _moveSpeedModifiers = new();
        foreach (ScriptableUpgrade upgrade in upgrades) ApplyUpgrade(upgrade);
    }
    
    private void ApplyUpgrade(ScriptableUpgrade upgrade)
    {

        _moveSpeedModifiers.maxSpeedFactor *= upgrade.GetMaxSpeedFactor();
        _moveSpeedModifiers.accelerationFactor *= upgrade.GetAccelerationFactor();
        _moveSpeedModifiers.addedMaxSpeed += upgrade.GetAddedMaxSpeed();
        _moveSpeedModifiers.addedAcceleration += upgrade.GetAddedAcceleration();
    }
}