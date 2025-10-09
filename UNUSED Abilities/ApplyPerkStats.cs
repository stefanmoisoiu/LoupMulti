// using Game.Data;
// using Game.Manager;
// using Game.Upgrade.Perks;
// using Player.Movement;
// using Player.Networking;
// using UnityEngine;
//
// namespace Player.Abilities
// {
//     public class ApplyPerkStats : PNetworkBehaviour
//     {
//         [SerializeField] private Movement.Movement movement;
//         [SerializeField] private Jump jump;
//         [SerializeField] private Stamina stamina;
//     
//         private PerkData[] cachedPerks;
//     
//         protected override void StartOnlineOwner()
//         {
//             PerkManager.OnPerkChosenOwner += PerkAdded;
//             DataManager.OnEntryUpdatedOwner += UpdateCachedPerks;
//         }
//         private void UpdateCachedPerks(PlayerData data) => cachedPerks = data.inGameData.GetPerks();
//
//         protected override void DisableOnlineOwner()
//         {
//             if (GameManager.Instance == null) return;
//             PerkManager.OnPerkChosenOwner -= PerkAdded;
//             DataManager.OnEntryUpdatedOwner -= UpdateCachedPerks;
//         }
//
//         protected override void UpdateOnlineOwner()
//         {
//             if (cachedPerks == null) return;
//             if (cachedPerks.Length == 0) return;
//         }
//
//         private void PerkAdded(ushort perkIndex)
//         {
//             PerkData perkData = GameManager.Instance.PerkManager.PerkList.GetPerk(perkIndex);
//         
//             perkData.Update();
//         
//             movement.AccelerationModifier.AddModifier(perkData.GetAccelerationModifier());
//             movement.MaxSpeedModifier.AddModifier(perkData.GetMaxSpeedModifier());
//             jump.JumpHeightModifier.AddModifier(perkData.GetJumpHeightModifier());
//             stamina.StaminaRecoverRateModifier.AddModifier(perkData.GetStaminaRecoveryModifier());
//             stamina.StaminaPerPartModifier.AddModifier(perkData.GetStaminaPerPartModifier());
//             stamina.AddedStaminaPartsModifier.AddModifier(perkData.GetAddedStaminaParts());
//         }
//     }
// }