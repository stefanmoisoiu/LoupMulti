using System;
using System.Linq;
using Game.Data;
using Game.Manager;
using Game.Stats;
using Game.Upgrades;
using Player.Networking;
using UnityEngine;
using Ability = Player.Networking.Ability;

namespace Player.Abilities
{
    public class AbilityManager : PNetworkBehaviour
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
            if (!DataManager.Instance.TryGetValue(NetworkManager.LocalClientId,
                    out PlayerData pd)) return;
            ScriptableUpgrade[] ownedUpgrades = pd.inGameData.GetUpgrades();
            foreach (ScriptableUpgrade upgrade in GameManager.Instance.UpgradesManager.Upgrades)
            {
                if (upgrade.Type != ScriptableUpgrade.UpgradeType.Active) continue;
            
                Ability ability = GetAbility(upgrade.ActiveAbility);
                if (ownedUpgrades.Contains(upgrade)) ability.EnableAbility();
                else ability.DisableAbility();
            }
        }
        private void UpgradeAddedEnableAbility(ushort newUpgradeIndex)
        {
            ScriptableUpgrade newUpgrade = GameManager.Instance.UpgradesManager.GetUpgrade(newUpgradeIndex);
            if (newUpgrade.Type != ScriptableUpgrade.UpgradeType.Active) return;
        
            Ability ability = GetAbility(newUpgrade.ActiveAbility);
            ability.EnableAbility();
        }

        public Ability GetAbility(Game.Stats.Ability ability) => abilities.First(a => a.ability == ability).script;

        public void EnableAbility(Game.Stats.Ability ability)
        {
            Ability script = GetAbility(ability);
            if (script.AbilityEnabled) return;
            script.EnableAbility();
        }
        public void DisableAbility(Game.Stats.Ability ability)
        {
            Ability script = GetAbility(ability);
            if (!script.AbilityEnabled) return;
            script.DisableAbility();
        }
        [Serializable]
        public struct PlayerAbility
        {

            public Game.Stats.Ability ability;
            public Ability script;
        }
    }
}