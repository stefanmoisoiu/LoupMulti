using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Base_Scripts
{
    public class HealthBarEffect : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Slider damageTransitionSlider;
        [SerializeField] private Slider healTransitionSlider;
        [Space]
        [SerializeField] private float valueAnimLength = .2f;
        [SerializeField] private float transitionAnimLength = .5f;
        [SerializeField] private AnimationCurve valueCurve;
        [SerializeField] private AnimationCurve transitionCurve;
        
        private Coroutine _coroutine;
        
        public void UpdateHealthBar(ushort previousHealth, ushort newHealth, ushort maxHealth)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(LerpHealthBar(previousHealth, newHealth, maxHealth));
        }
        private IEnumerator LerpHealthBar(ushort previousHealth, ushort newHealth, ushort maxHealth)
        {
            if (previousHealth == newHealth) yield break;

            healTransitionSlider.value = 0;
            damageTransitionSlider.value = 0;
                
            bool healing = newHealth > previousHealth;
                
            Slider valueSlider = healing ? healTransitionSlider : slider;
            Slider transitionSlider = healing ? slider : damageTransitionSlider;
                
            float startValue = previousHealth / (float)maxHealth;
            float endValue = newHealth / (float)maxHealth;
            
            valueSlider.value = startValue;
            transitionSlider.value = startValue;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < valueAnimLength)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / valueAnimLength);
                float curveValue = valueCurve.Evaluate(t);
                valueSlider.value = Mathf.Lerp(startValue, endValue, curveValue);
                yield return null;
            }
            valueSlider.value = endValue;
                
            elapsedTime = 0f;
                
            while (elapsedTime < transitionAnimLength)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionAnimLength);
                float curveValue = transitionCurve.Evaluate(t);
                transitionSlider.value = Mathf.Lerp(startValue, endValue, curveValue);
                yield return null;
            }
                
            transitionSlider.value = endValue;
        }
    }
}