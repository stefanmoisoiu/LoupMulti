// using System;
// using System.Linq;
// using Game.Upgrade.Perks;
// using Player.Networking;
// using UnityEngine;
// using Ability = Player.Networking.Ability;
//
// namespace Player.Abilities
// {
//     public class AbilityManager : PNetworkBehaviour
//     {
//         [SerializeField] private PlayerAbility[] abilities;
//
//         protected override void StartOnlineOwner()
//         {
//             PerkManager.OnPerkChosenOwner += PerkAddedEnableAbility;
//         }
//
//         protected override void DisableAnyOwner()
//         {
//             PerkManager.OnPerkChosenOwner -= PerkAddedEnableAbility;
//         }
//
//         private void UpdateAllAbilityStates()
//         {
//             Debug.LogWarning("Get owned perks pas cached!!");
//             if (GameManager.Instance != null) return;
//             if (!DataManager.Instance.TryGetValue(NetworkManager.LocalClientId,
//                     out PlayerData pd)) return;
//             PerkData[] ownedPerks = pd.inGameData.GetPerks();
//             foreach (PerkData perkData in GameManager.Instance.PerkManager.PerkList.perks)
//             {
//                 if (perkData.Type != PerkData.PerkType.Active) continue;
//             
//                 Ability ability = GetAbility(perkData.ActiveAbility);
//                 if (ownedPerks.Contains(perkData)) ability.EnableAbility();
//                 else ability.DisableAbility();
//             }
//         }
//         private void PerkAddedEnableAbility(ushort newPerkIndex)
//         {
//             PerkData newPerkData = GameManager.Instance.PerkManager.PerkList.GetPerk(newPerkIndex);
//             if (newPerkData.Type != PerkData.PerkType.Active) return;
//         
//             Ability ability = GetAbility(newPerkData.ActiveAbility);
//             ability.EnableAbility();
//         }
//
//         public Ability GetAbility(Game.Stats.Ability ability) => abilities.First(a => a.ability == ability).script;
//
//         public void EnableAbility(Game.Stats.Ability ability)
//         {
//             Ability script = GetAbility(ability);
//             if (script.AbilityEnabled) return;
//             script.EnableAbility();
//         }
//         public void DisableAbility(Game.Stats.Ability ability)
//         {
//             Ability script = GetAbility(ability);
//             if (!script.AbilityEnabled) return;
//             script.DisableAbility();
//         }
//         [Serializable]
//         public struct PlayerAbility
//         {
//
//             public Game.Stats.Ability ability;
//             public Ability script;
//         }
//     }
// }