using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ScriptableUpgrade : ScriptableObject
{
    [TitleGroup("Base")]
    [FoldoutGroup("Base/Info")][SerializeField] private Sprite icon;
    [FoldoutGroup("Base/Info")][SerializeField] private string upgradeName;
    [FoldoutGroup("Base/Info")][SerializeField][TextArea] private string upgradeDescription;
    [FoldoutGroup("Base/Info")][SerializeField] private UpgradeType upgradeType;

    [BoxGroup("Base/Info/Active Ability")] [ShowIf(("@upgradeType == UpgradeType.Ability"))] [SerializeField] private string abilityName;
    [TitleGroup("Evolution")][SerializeField] private Evolution[] evolutions;

    
    public Sprite Icon => icon;
    public string UpgradeName => upgradeName;
    public string UpgradeDescription => upgradeDescription;
    public UpgradeType Type => upgradeType;

    public Evolution[] AvailableEvolutions(ScriptableUpgrade[] ownedUpgrades)
    {
        return evolutions.Where(e => e.EvolutionPossible(ownedUpgrades)).ToArray();
    }

    public enum UpgradeType
    {
        Ability,
        Passive
    }

    public abstract void Update();

    public abstract Modifier<float>.ModifierComponent GetAccelerationModifier();
    public abstract Modifier<float>.ModifierComponent GetMaxSpeedModifier();
    public abstract Modifier<float>.ModifierComponent GetJumpHeightModifier();
    public abstract Modifier<float>.ModifierComponent GetStaminaRecoveryModifier();
    public abstract Modifier<float>.ModifierComponent GetStaminaPerPartModifier();
    public abstract Modifier<int>.ModifierComponent GetAddedStaminaParts();

    [Serializable]
    public struct Evolution
    {
        public ScriptableUpgrade evolution;
        public ScriptableUpgrade[] requiredUpgrades;
        public bool EvolutionPossible(ScriptableUpgrade[] ownedUpgrades)
        {
            if (requiredUpgrades == null) return true;
            return requiredUpgrades.All(ownedUpgrades.Contains);
        }
    }
}