using System.Collections;
using TMPro;
using UnityEngine;

public class PHealthUI : PNetworkBehaviour
{
    [SerializeField] private string healthTextTag = "HealthText";
    
    private TMP_Text healthText;

    private Coroutine healthTextLerpCoroutine;
    [SerializeField] private float healthTextLerpDurationPerHealth = 0.1f;
    [SerializeField] private AnimationCurve healthTextLerpCurve;

    private ushort previousHealth;

    protected override void StartAnyOwner()
    {
        healthText = GameObject.FindGameObjectWithTag(healthTextTag).GetComponent<TMP_Text>();
        Debug.LogWarning("Cached upgrade pas fait");
        PlayerDataManager.OnEntryUpdatedOwner += UpdateHealth;
        previousHealth = PlayerInGameData.MaxHealth;
        healthText.text = previousHealth.ToString();
    }

    protected override void DisableAnyOwner()
    {
        PlayerDataManager.OnEntryUpdatedOwner -= UpdateHealth;
        if (healthTextLerpCoroutine != null) StopCoroutine(healthTextLerpCoroutine);
    }

    private void UpdateHealth(PlayerData newPlayerData)
    {
        if (newPlayerData.ClientId == ulong.MaxValue) return;
        healthTextLerpCoroutine = StartCoroutine(UpdateHealthText(previousHealth, newPlayerData.InGameData.health));
        previousHealth = newPlayerData.InGameData.health;
    }
    
    private IEnumerator UpdateHealthText(ushort health, ushort newHealth)
    {
        float time = 0;
        float duration = healthTextLerpDurationPerHealth * Mathf.Abs(newHealth - health);
        float startValue = health;
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
