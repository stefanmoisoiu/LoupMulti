using System.Collections;
using Base_Scripts;
using Game.Common;
using Game.Data.Extensions;
using Player.Health;
using Player.Networking;
using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private MonoBehaviour healthScript;
    private IHealth healthComponent => healthScript as IHealth;

    [SerializeField] private HealthBarEffect healthBarEffect;
    

    private Coroutine _coroutine;

    [SerializeField] private CanvasGroup canvasGroup;
    
    [SerializeField] private float alphaFadeDuration;
    private Coroutine _alphaFadeCoroutine;
    [SerializeField] private AnimationCurve alphaFadeCurve;
    
    private Coroutine _updateHealthBarCoroutine;

    private void OnEnable()
    {
        healthComponent.OnHealthChanged += UpdateHealthBar;
    }


    private void OnDisable()
    {
        // Très important de se désabonner !
        healthComponent.OnHealthChanged -= UpdateHealthBar;
    }
    


    // La signature change légèrement : plus besoin du clientId
    private void UpdateHealthBar(ushort previousHealth, ushort newHealth)
    {
        if (previousHealth == newHealth) return;
        canvasGroup.alpha = 1;
        
        ushort maxHealth = healthComponent.GetMaxHealth(); // On utilise le maxHealth local
        
        healthBarEffect.UpdateHealthBar(previousHealth, newHealth, maxHealth);
        
        if (newHealth == maxHealth)
        {
            if (_alphaFadeCoroutine != null) StopCoroutine(_alphaFadeCoroutine);
            _alphaFadeCoroutine = StartCoroutine(CanvasAlphaFade(false));
        }
    }
    

    private IEnumerator CanvasAlphaFade(bool fadeIn)
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        float endAlpha = fadeIn ? 1f : 0f;

        while (elapsedTime < alphaFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / alphaFadeDuration);
            float curveValue = alphaFadeCurve.Evaluate(t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
}
