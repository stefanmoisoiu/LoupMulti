using System;
using Game.Common;
using Player.Abilities.UI;
using Player.Networking;
using Player.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Abilities
{
    public class Ability : PNetworkBehaviour
    {
        public Item Item { get; private set; }
        public PlayerReferences PlayerReferences { get; private set; }

        private float _cooldown;
        public float Cooldown => _cooldown;

        private float _maxCooldown;
        
        private bool _abilityEnabled;
        public bool AbilityEnabled => _abilityEnabled;
        private bool _canUseAbility;

        private AbilitySlotUI _slotUI;

        [Title("Events")]
        public Action OnAbilityEnabledOwner;
        public Action OnAbilityDisabledOwner;
        public Action OnAbilityUsedOwner;
        public Action OnAbilityAvailableOwner;
        public Action<bool> OnCanUseAbilityChangedOwner;
        
        public void Initialize(Item item, PlayerReferences playerReferences)
        {
            Item = item;
            PlayerReferences = playerReferences;
            
            _maxCooldown = Item.AbilityData.BaseCooldown;
        }
        
        protected override void UpdateOnlineOwner()
        {
            UpdateCooldown();
        }

        private void UpdateCooldown()
        {
            if (_cooldown <= 0) return;
            
            _cooldown -= Time.deltaTime;

            _slotUI?.UpdateCooldownDisplay(_cooldown, _maxCooldown);
            
            if (_cooldown <= 0)
            {
                _cooldown = 0;
                if (_abilityEnabled)
                {
                    OnAbilityAvailableOwner?.Invoke();
                    UpdateCanUseAbilityFlag();
                }
            }
        }
        
        private void UpdateCanUseAbilityFlag()
        {
            bool canUse = CanUseAbility();
            if (canUse != _canUseAbility)
            {
                _canUseAbility = canUse;
                OnCanUseAbilityChangedOwner?.Invoke(canUse);
            }
        }
                
        public void EnableAbility(AbilitySlotUI ui)
        {
            _abilityEnabled = true;
            
            _slotUI = ui;
            _slotUI.SetIcon(Item.Info.Icon);
            OnCanUseAbilityChangedOwner += _slotUI.CanUseAbilityChanged;
            
            OnAbilityEnabledOwner?.Invoke();

            if (_cooldown <= 0)
            {
                OnAbilityAvailableOwner?.Invoke();
            }
            
            UpdateCanUseAbilityFlag();
        }
        
        public void DisableAbility()
        {
            _abilityEnabled = false;
            
            if (_slotUI != null)
            {
                _slotUI.SetIcon(null);
                OnCanUseAbilityChangedOwner -= _slotUI.CanUseAbilityChanged;
                _slotUI.UpdateCooldownDisplay(0, _maxCooldown);
                _slotUI = null;
            }
            
            OnAbilityDisabledOwner?.Invoke();
            UpdateCanUseAbilityFlag();
        }
        
        public virtual void TryUseAbility(out bool success)
        {
            success = false;
            if (!CanUseAbility()) return;
            
            success = true;
            ApplyCooldown();
            OnAbilityUsedOwner?.Invoke();
        }
                
        public virtual bool CanUseAbility() => _abilityEnabled && _cooldown <= 0;

        public void ApplyCooldown(float newCooldown = float.MinValue, float maxCooldown = float.MinValue)
        {
            _maxCooldown = maxCooldown == float.MinValue ? Item.AbilityData.BaseCooldown : Mathf.Max(0, maxCooldown);
            _cooldown = newCooldown == float.MinValue ? _maxCooldown : Mathf.Clamp(newCooldown, 0, _maxCooldown);
            _slotUI?.UpdateCooldownDisplay(_cooldown, _maxCooldown);
            UpdateCanUseAbilityFlag();
        }
                
        public void ReduceCooldown(float amount)
        {
            if (_cooldown <= 0) return;
            
            _cooldown -= amount;
            if (_cooldown < 0) _cooldown = 0;
            
            _slotUI?.UpdateCooldownDisplay(_cooldown, _maxCooldown);
            
            if (_cooldown <= 0)
            {
                UpdateCanUseAbilityFlag();
            }
        }
    }
}