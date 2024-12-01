using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ScriptableUpgrade : ScriptableObject
{
    [TitleGroup("Info")][SerializeField] private string upgradeName;
    [TitleGroup("Info")][SerializeField][TextArea] private string upgradeDescription;
    public string UpgradeName => upgradeName;
    public string UpgradeDescription => upgradeDescription;

    public abstract float GetAccelerationFactor();
    public abstract float GetMaxSpeedFactor();
    public abstract float GetAddedMaxSpeed();
    public abstract float GetAddedAcceleration();
    
    
    public abstract float GetJumpHeightFactor();
    public abstract float GetGravityFactor();
    
    
    public abstract float GetStaminaRecoveryFactor();
    public abstract int GetAddedStaminaParts();
}