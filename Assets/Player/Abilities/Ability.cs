using System;
using Game.Common;
using Player.Abilities.UI;
using Player.Networking;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Abilities
{
        public class Ability : PNetworkBehaviour
        {
                [SerializeField] [InlineEditor] private AbilityData abilityData;
                public AbilityData AbilityData => abilityData;
                
                private float _cooldown = 0;
                public float Cooldown => _cooldown;
                
                private AbilitySlotUI _slotUI;

                private bool _abilityEnabled = false;
                public bool AbilityEnabled => _abilityEnabled;
                
                public Action OnAbilityEnabledOwner;
                public Action OnAbilityDisabledOwner;
                public Action OnAbilityUsedOwner;
                public Action OnAbilityAvailableOwner;
                
                public Action<bool> OnCanUseAbilityChangedOwner;
                private bool _canUseAbility = false;

                protected virtual void ReduceCooldown()
                {
                        float previousCooldown = _cooldown;
                        _cooldown -= Time.deltaTime;
                        if (_cooldown < 0) _cooldown = 0;
                        
                        if (previousCooldown > 0 && _cooldown <= 0 && _abilityEnabled)
                                OnAbilityAvailableOwner?.Invoke();
                }
                
                private void UpdateCanUseAbility()
                {
                        bool canUse = CanUseAbility();
                        if (canUse != _canUseAbility)
                        {
                                _canUseAbility = canUse;
                                OnCanUseAbilityChangedOwner?.Invoke(canUse);
                        }
                }
                
                protected override void UpdateOnlineOwner()
                {
                        ReduceCooldown();
                        UpdateCanUseAbility();
                }
                
                public void EnableAbility(AbilitySlotUI ui)
                {
                        _abilityEnabled = true;
                        
                        _slotUI = ui;
                        ui.SetIcon(abilityData.Info.Icon);
                        OnCanUseAbilityChangedOwner += ui.CanUseAbilityChanged;
                        ui.CanUseAbilityChanged(CanUseAbility());
                        
                        OnAbilityEnabledOwner?.Invoke();
                        
                        if (_cooldown <= 0)
                                OnAbilityAvailableOwner?.Invoke();
                }
                public void DisableAbility()
                {
                        _abilityEnabled = false;
                        
                        _slotUI.SetIcon(null);
                        OnCanUseAbilityChangedOwner -= _slotUI.CanUseAbilityChanged;
                        
                        _slotUI.SetCooldown(0, abilityData.BaseCooldown);
                        _slotUI = null;
                        
                        OnAbilityDisabledOwner?.Invoke();
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

                public void ApplyCooldown(float newCooldown = float.MinValue)
                {
                        _cooldown = newCooldown == float.MinValue ? abilityData.BaseCooldown : Mathf.Max(0, newCooldown);
                        _slotUI.SetCooldown(_cooldown, _cooldown);
                }
                
                public void ReduceCooldown(float amount)
                {
                        _cooldown -= amount;
                        if (_cooldown < 0) _cooldown = 0;
                        _slotUI.ReduceCooldown(_cooldown);
                }
        }
}