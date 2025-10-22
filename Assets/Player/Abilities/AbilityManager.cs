using Game.Common;
using Game.Upgrade.Shop;
using Input;
using Player.Abilities.UI;
using Player.Networking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Abilities
{
    public class AbilityManager : PNetworkBehaviour
    {
        [TitleGroup("Configuration")]
        [SerializeField] private Ability drill;
        [TitleGroup("Configuration")]
        [SerializeField] private Ability[] abilities;
        public Ability[] Abilities => abilities;

        [TitleGroup("UI")]
        [SerializeField] private AbilityManagerUI abilityManagerUI;

        public const int AbilitySlotCount = 3;
        
        private Ability[] _abilitySlots;
        public Ability[] AbilitySlotsArray => _abilitySlots;
        
        private Ability _drillSlot;
        public Ability DrillSlot => _drillSlot;
        
        protected override void StartOnlineOwner()
        {
            _abilitySlots = new Ability[AbilitySlotCount];
            EquipDrillAbility(drill);
        
            InputManager.OnDrillUse += TryUseDrill;
            InputManager.OnAbilityUse += TryUseAbilitySlot;

            ShopManager.OnShopItemBoughtOwner += OnShopItemBought;
        }

        protected override void DisableOnlineOwner()
        {
            InputManager.OnDrillUse -= TryUseDrill;
            InputManager.OnAbilityUse -= TryUseAbilitySlot;
            
            ShopManager.OnShopItemBoughtOwner -= OnShopItemBought;
        }
        
        private void OnShopItemBought(ushort itemInd)
        {
            Item item = ItemRegistry.Instance.GetItem(itemInd);
            if (item.Type != Item.ItemType.Ability) return;
            Debug.LogError("Equipping ability in slot 0. This should be changed later. :)");
            EquipAbility(item, 0);
        }

        private Ability GetAbility(Item item)
        {
            foreach (Ability ability in abilities)
            {
                if (ability.AbilityData == item.AbilityData)
                    return ability;
            }
            throw new System.Exception($"Ability {item.Info.Name} not found.");
        }
        
        public void EquipDrillAbility(Ability drillAbility) => EquipAbility(drillAbility, -1);

        public void EquipAbility(Item item, int slot)
        {
            
            EquipAbility(GetAbility(item), slot);
        }
        public void EquipAbility(Ability ability, int slot)
        {
            if (slot == -1) EquipDrillInternal(ability);
            else EquipSlotInternal(ability, slot);
        }

        private void EquipDrillInternal(Ability ability)
        {
            _drillSlot?.DisableAbility();
            _drillSlot = ability;
            _drillSlot?.EnableAbility(abilityManagerUI.GetSlotUI(-1));
        }

        private void EquipSlotInternal(Ability ability, int slot)
        {
            if (slot < 0 || slot >= AbilitySlotCount) return;
            
            _abilitySlots[slot]?.DisableAbility();
            _abilitySlots[slot] = ability;
            _abilitySlots[slot]?.EnableAbility(abilityManagerUI.GetSlotUI(slot));
        }
        
        private void TryUseDrill()
        {
            _drillSlot?.TryUseAbility(out bool success);
        }

        private void TryUseAbilitySlot(int slot)
        {
            if (slot < 0 || slot >= AbilitySlotCount) return;
            
            _abilitySlots[slot]?.TryUseAbility(out bool success);
        }
    }
}