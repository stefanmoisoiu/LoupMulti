using Input;
using Player.Abilities.UI;
using Player.Networking;
using UnityEngine;

namespace Player.Abilities
{
    public class AbilityManager : PNetworkBehaviour
    {
        [SerializeField] private Ability drill;

        [SerializeField] private Ability[] abilities;
        public Ability[] Abilities => abilities;

        [SerializeField] private AbilityManagerUI abilityManagerUI;
    

        public const int AbilitySlotCount = 3;
        private Ability[] _abilitySlots;
        public Ability[] AbilitySlotsArray => _abilitySlots;
        private Ability _drillSlot;
        public Ability DrillSlot => _drillSlot;

        [SerializeField] private Ability testAbility;
        protected override void StartOnlineOwner()
        {
            _abilitySlots = new Ability[AbilitySlotCount];

            EquipAbility(testAbility, 0);
            EquipDrillAbility(drill);
        
            InputManager.OnDrillUse += TryUseDrillAbility;
            InputManager.OnAbilityUse += TryUseAbility;
        }

        protected override void DisableOnlineOwner()
        {
            InputManager.OnDrillUse -= TryUseDrillAbility;
            InputManager.OnAbilityUse -= TryUseAbility;
        }
        public void EquipDrillAbility(Ability drillAbility) => EquipAbility(drillAbility, -1);
        public void EquipAbility(Ability ability, int slot)
        {
            if (slot == -1) _drillSlot?.DisableAbility();
            else _abilitySlots[slot]?.DisableAbility();
        
            if (slot == -1) _drillSlot = ability;
            else
            {
                if (slot < 0 || slot >= AbilitySlotCount) return;
                _abilitySlots[slot] = ability;
            }

            if (ability != null) ability.EnableAbility(abilityManagerUI.GetSlotUI(slot));
        }
        private void TryUseDrillAbility() => TryUseAbility(-1);
        private void TryUseAbility(int slot)
        {
            Ability abilityToUse = null;
        
            if (slot == -1) abilityToUse = _drillSlot;
            else
            {
                if (slot < 0 || slot >= AbilitySlotCount) return;
                if (_abilitySlots[slot] == null) return;
                abilityToUse = _abilitySlots[slot];
            }
        
            abilityToUse?.TryUseAbility(out bool success);
        }
    }
}
