using System;
using System.Collections;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    private static readonly int Transition1 = Shader.PropertyToID(TransitionProperty);
    [SerializeField] private Material transitionMat;
    [SerializeField] private AnimationCurve transitionCurve;
    [SerializeField] private float transitionDuration = 1f;
    private const string TransitionProperty = "_Transition";

    public IEnumerator Transition(bool fadeIn, Action onFinished)
    {
        float startValue = fadeIn ? 0f : 1f;
        float endValue = fadeIn ? 1f : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            float value = Mathf.Lerp(startValue, endValue, transitionCurve.Evaluate(t));
            transitionMat.SetFloat(Transition1, value);
            yield return null;
        }

        transitionMat.SetFloat(Transition1, endValue);
        onFinished?.Invoke();
    }
}
