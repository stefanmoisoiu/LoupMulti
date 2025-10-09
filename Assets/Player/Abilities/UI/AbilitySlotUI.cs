using System;
using System.Collections;
using Game.Common.CircularBar;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Abilities.UI
{
    public class AbilitySlotUI : MonoBehaviour
    {
        [SerializeField] private CircularBar circularBar;
        public CircularBar CircularBar => circularBar;

        [SerializeField] private Image icon;
        public Image Icon => icon;

        [SerializeField] private Image canUseAbilityGraphic;
        public Image CanUseAbilityGraphic => canUseAbilityGraphic;
        
        private float _currentCooldown = 0;
        private Coroutine _cooldownCoroutine;

        private void Awake()
        {
            icon.enabled = false;
            canUseAbilityGraphic.enabled = false;
        }

        public void CanUseAbilityChanged(bool canUseAbility)
        {
            canUseAbilityGraphic.enabled = canUseAbility;
        }
        
        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }
        
        public void SetCooldown(float cooldown, float maxCooldown)
        {
            if (_cooldownCoroutine != null) StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = StartCoroutine(CooldownAnimation(cooldown, maxCooldown));
        }
        public void ReduceCooldown(float amount)
        {
            _currentCooldown -= amount;
            if (_currentCooldown < 0) _currentCooldown = 0;
        }

        private IEnumerator CooldownAnimation(float cooldown, float maxCooldown)
        {
            _currentCooldown = cooldown;
            while (_currentCooldown > 0)
            {
                _currentCooldown -= Time.deltaTime;
                circularBar.SetAdv(_currentCooldown / maxCooldown);
                yield return null;
            }
            circularBar.SetAdv(0);
        }
    }
}