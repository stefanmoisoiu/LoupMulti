using System.Collections;
using Game.Common;
using Game.Data.Extensions;
using Player.Networking;
using TMPro;
using UnityEngine;

namespace Player.UI.Health_unused
{
    public class HealthText : PNetworkBehaviour
    {
        private const string HealthTextTag = "HealthText";
    
        private TMP_Text healthText;

        private Coroutine healthTextLerpCoroutine;
        [SerializeField] private float healthTextLerpDurationPerHealth = 0.1f;
        [SerializeField] private AnimationCurve healthTextLerpCurve;

        protected override void StartAnyOwner()
        {
            healthText = PCanvas.CanvasObjects[HealthTextTag].GetComponent<TMP_Text>();
            PlayerHealth.OnHealthChangedOwner += UpdateHealth;
            healthText.text = GameSettings.PlayerMaxHealth.ToString();
        }

        protected override void DisableAnyOwner()
        {
            PlayerHealth.OnHealthChangedOwner -= UpdateHealth;
            if (healthTextLerpCoroutine != null) StopCoroutine(healthTextLerpCoroutine);
        }

        private void UpdateHealth(ushort previousHealth, ushort currentHealth)
        {
            if (healthTextLerpCoroutine != null) StopCoroutine(healthTextLerpCoroutine);
            healthTextLerpCoroutine = StartCoroutine(UpdateHealthText(previousHealth, currentHealth));
        }
    
        private IEnumerator UpdateHealthText(ushort previousHealth, ushort newHealth)
        {
            float time = 0;
            float duration = healthTextLerpDurationPerHealth * Mathf.Abs(newHealth - previousHealth);
            float startValue = previousHealth;
            float endValue = newHealth;
            while (time < duration)
            {
                time += Time.deltaTime;
                healthText.text = ((int) Mathf.Lerp(startValue, endValue, healthTextLerpCurve.Evaluate(time / duration))).ToString();
                yield return null;
            }
            healthText.text = newHealth.ToString();
        }
    }
}
