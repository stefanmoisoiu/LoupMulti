using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Base Upgrade", menuName = "Upgrade/Base Upgrade")]
public class BaseScriptableUpgrade : ScriptableUpgrade
{
    [TitleGroup("Stat Changes")] [FoldoutGroup("Stat Changes/Base Movement")]
    [BoxGroup("Stat Changes/Base Movement/Acceleration")] [Range(0, 3)] public float accelerationFactor = 1;
    [BoxGroup("Stat Changes/Base Movement/Acceleration")] [Range(0, 3)] public float addedAcceleration = 0;
    [BoxGroup("Stat Changes/Base Movement/Max Speed")][Range(0,3)]public float maxSpeedFactor = 1;
    [BoxGroup("Stat Changes/Base Movement/Max Speed")][Range(0,3)]public float addedMaxSpeed = 0;
    
    [FoldoutGroup("Stat Changes/Jump - Gravity")] [Range(0,3)]public float gravityFactor = 1;
    [FoldoutGroup("Stat Changes/Jump - Gravity")] [Range(0,3)]public float jumpHeightFactor = 1;
    
    [FoldoutGroup("Stat Changes/Stamina")][Range(0,3)]public float staminaRecoveryFactor = 1;
    [FoldoutGroup("Stat Changes/Stamina")] [Range(0, 3)] public int addedStaminaParts = 0;
    
    public override float GetAccelerationFactor() => accelerationFactor;
    public override float GetMaxSpeedFactor() => maxSpeedFactor;
    public override float GetAddedMaxSpeed() => addedMaxSpeed;
    public override float GetAddedAcceleration() => addedAcceleration;
    
    
    public override float GetJumpHeightFactor() => jumpHeightFactor;
    public override float GetGravityFactor() => gravityFactor;
    public override float GetStaminaRecoveryFactor() => staminaRecoveryFactor;
    public override int GetAddedStaminaParts() => addedStaminaParts;
}