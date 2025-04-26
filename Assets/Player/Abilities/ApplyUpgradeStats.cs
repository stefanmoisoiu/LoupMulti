using Game.Data;
using Game.Manager;
using Game.Upgrades;
using Player.Movement;
using Player.Networking;
using UnityEngine;

namespace Player.Upgrades
{
    public class ApplyUpgradeStats : PNetworkBehaviour
    {
        [SerializeField] private Movement.Movement movement;
        [SerializeField] private Jump jump;
        [SerializeField] private Stamina stamina;
    
        private ScriptableUpgrade[] cachedUpgrades;
    
        protected override void StartOnlineOwner()
        {
            UpgradesManager.OnUpgradeChosenOwner += UpgradeAdded;
            PlayerDataManager.OnEntryUpdatedOwner += UpdateCachedUpgrades;
        }
        private void UpdateCachedUpgrades(PlayerData data) => cachedUpgrades = data.InGameData.GetUpgrades();

        protected override void DisableOnlineOwner()
        {
            if (GameManager.Instance == null) return;
            UpgradesManager.OnUpgradeChosenOwner -= UpgradeAdded;
            PlayerDataManager.OnEntryUpdatedOwner -= UpdateCachedUpgrades;
        }

        protected override void UpdateOnlineOwner()
        {
            if (cachedUpgrades == null) return;
            if (cachedUpgrades.Length == 0) return;
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
}