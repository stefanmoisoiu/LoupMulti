using System.Collections;
using Game.Common.CircularBar;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Abilities.UI
{
    public class AbilitySlotUI : MonoBehaviour
    {
        [SerializeField] private CircularBar circularBar;
        [SerializeField] private Image icon;
        [SerializeField] private Image canUseAbilityGraphic;

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
        
        public void UpdateCooldownDisplay(float currentCooldown, float maxCooldown)
        {
            if (maxCooldown <= 0 || currentCooldown <= 0)
            {
                circularBar.SetAdv(0);
            }
            else
            {
                // Calcule la progression (ex: 3s / 10s = 0.3)
                float progress = currentCooldown / maxCooldown;
                circularBar.SetAdv(progress);
            }
        }
    }
}