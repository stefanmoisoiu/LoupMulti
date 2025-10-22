using Input;
using Player.Abilities.UI;
using Player.Networking;
using Sirenix.OdinInspector;
using UnityEngine;
using Game.Common;
using Game.Upgrade.Shop;
using AYellowpaper.SerializedCollections; // Ajouté

namespace Player.Abilities
{
    public class AbilityManager : PNetworkBehaviour
    {
        [TitleGroup("Configuration")]
        [SerializeField] private Item drillItem;

        [TitleGroup("UI")]
        [SerializeField] private AbilityManagerUI abilityManagerUI;

        [TitleGroup("Ability Mapping")]
        [SerializeField]
        private SerializedDictionary<Item, Ability> abilityMap;

        public const int AbilitySlotCount = 3;
        
        private Ability[] _abilitySlots;
        public Ability[] AbilitySlotsArray => _abilitySlots;
        
        private Ability _drillSlot;
        public Ability DrillSlot => _drillSlot;

        private Ability GetAbility(Item item)
        {
            if (abilityMap.TryGetValue(item, out Ability prefab)) return prefab;
            throw new System.Exception($"Aucun prefab 'Ability' n'est mappé pour l'item '{item.Info.Name}' dans le AbilityManager.");
        }

        protected override void StartOnlineOwner()
        {
            _abilitySlots = new Ability[AbilitySlotCount];
            EquipDrillAbility(drillItem);
        
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
        
        public void EquipDrillAbility(Item item) => EquipAbility(item, -1);

        public void EquipAbility(Item item, int slot)
        {
            Ability oldAbility = (slot == -1) ? _drillSlot : _abilitySlots[slot];
            if (oldAbility != null)
            {
                oldAbility.DisableAbility();
            }

            if (item == null || item.Type != Item.ItemType.Ability)
            {
                if (slot == -1) _drillSlot = null;
                else _abilitySlots[slot] = null;
                return;
            }

            Ability newAbility = GetAbility(item);
            newAbility.Initialize(item); 

            if (slot == -1) _drillSlot = newAbility;
            else _abilitySlots[slot] = newAbility;

            newAbility.EnableAbility(abilityManagerUI.GetSlotUI(slot));
        }
        
        private void TryUseDrill() => _drillSlot?.TryUseAbility(out bool _);
        
        private void TryUseAbilitySlot(int slot)
        {
            if (slot < 0 || slot >= AbilitySlotCount) return;
            _abilitySlots[slot]?.TryUseAbility(out bool _);
        }
    }
}