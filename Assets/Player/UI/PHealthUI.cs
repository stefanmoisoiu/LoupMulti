using System.Collections;
using TMPro;
using UnityEngine;

public class PHealthUI : PNetworkBehaviour
{
    [SerializeField] private string healthTextTag = "HealthText";
    
    private TMP_Text healthText;

    private Coroutine healthTextLerpCoroutine;
    [SerializeField] private AnimationCurve healthTextLerpCurve;
    

    protected override void StartAnyOwner()
    {
        healthText = GameObject.FindGameObjectWithTag(healthTextTag).GetComponent<TMP_Text>();
        CachedOwnerClientData.onOwnerDataChanged += UpdateHealth;
    }

    protected override void DisableAnyOwner()
    {
        CachedOwnerClientData.onOwnerDataChanged -= UpdateHealth;
        if (healthTextLerpCoroutine != null) StopCoroutine(healthTextLerpCoroutine);
    }

    private void UpdateHealth(PlayerData previousPlayerData, PlayerData newPlayerData)
    {
        if (newPlayerData.ClientId == ulong.MaxValue) return;
        if (healthTextLerpCoroutine != null) StopCoroutine(healthTextLerpCoroutine);
        healthTextLerpCoroutine = StartCoroutine(UpdateHealthText(previousPlayerData.InGameData.health,
            newPlayerData.InGameData.health));
    }
    
    private IEnumerator UpdateHealthText(ushort previousHealth, ushort newHealth)
    {
        float time = 0;
        float duration = 1f;
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
