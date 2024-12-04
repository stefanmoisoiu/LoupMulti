using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Base Upgrade", menuName = "Upgrade/Base Upgrade")]
public class BaseScriptableUpgrade : ScriptableUpgrade
{
    [TitleGroup("Stat Changes")] [FoldoutGroup("Stat Changes/Base Movement")]
    [BoxGroup("Stat Changes/Base Movement/Acceleration")] [Range(0, 3)] public float accelerationFactor = 1;
    [BoxGroup("Stat Changes/Base Movement/Acceleration")] [Range(-5, 5)] public float addedAcceleration = 0;
    [BoxGroup("Stat Changes/Base Movement/Max Speed")][Range(0,3)]public float maxSpeedFactor = 1;
    [BoxGroup("Stat Changes/Base Movement/Max Speed")][Range(-5, 5)]public float addedMaxSpeed = 0;
    
    [FoldoutGroup("Stat Changes/Jump - Gravity")] [Range(0,3)]public float gravityFactor = 1;
    [FoldoutGroup("Stat Changes/Jump - Gravity")] [Range(0,3)]public float jumpHeightFactor = 1;
    [FoldoutGroup("Stat Changes/Jump - Gravity")] [Range(-5, 5)]public float addedJumpHeight = 0;
    
    
    [FoldoutGroup("Stat Changes/Stamina")]
    [BoxGroup("Stat Changes/Stamina/Recovery")][Range(0,3)]public float staminaRecoveryFactor = 1;
    [BoxGroup("Stat Changes/Stamina/Max")] [Range(0, 3)] public int addedStaminaParts = 0;
    [BoxGroup("Stat Changes/Stamina/Max")] [Range(0, 3)] public int staminaPartFactor = 1;
    [BoxGroup("Stat Changes/Stamina/Max")] [Range(0, 3)] public float staminaPerPartFactor = 1;
    [BoxGroup("Stat Changes/Stamina/Max")] [Range(-150, 150)] public float addedStaminaPerPart = 0;
    
    
    private Modifier<float>.ModifierComponent _accelerationModifier = new(1,0);
    private Modifier<float>.ModifierComponent _maxSpeedModifier = new(1,0);
    private Modifier<float>.ModifierComponent _jumpHeightModifier = new(1,0);
    private Modifier<float>.ModifierComponent _staminaRecoveryModifier = new(1,0);
    private Modifier<float>.ModifierComponent _staminaPerPartModifier = new(1,0);
    private Modifier<int>.ModifierComponent _AddedStaminaPartModifier = new(1,0);
    
    public override void Update()
    {
        _accelerationModifier.added = addedAcceleration;
        _accelerationModifier.factor = accelerationFactor;
        
        _maxSpeedModifier.added = addedMaxSpeed;
        _maxSpeedModifier.factor = maxSpeedFactor;
        
        _jumpHeightModifier.added = addedJumpHeight;
        _jumpHeightModifier.factor = jumpHeightFactor;
        
        _staminaRecoveryModifier.factor = staminaRecoveryFactor;
        
        _staminaPerPartModifier.added = addedStaminaPerPart;
        _staminaPerPartModifier.factor = staminaPerPartFactor;
        
        _AddedStaminaPartModifier.added = addedStaminaParts;
        _AddedStaminaPartModifier.factor = staminaPartFactor;
    }


    public override Modifier<float>.ModifierComponent GetAccelerationModifier() => _accelerationModifier;
    public override Modifier<float>.ModifierComponent GetMaxSpeedModifier() => _maxSpeedModifier;
    public override Modifier<float>.ModifierComponent GetJumpHeightModifier() => _jumpHeightModifier;
    public override Modifier<float>.ModifierComponent GetStaminaRecoveryModifier() => _staminaRecoveryModifier;
    public override Modifier<float>.ModifierComponent GetStaminaPerPartModifier() => _staminaPerPartModifier;
    public override Modifier<int>.ModifierComponent GetAddedStaminaParts() => _AddedStaminaPartModifier;
}