using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Rendering.Transitions
{
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance { get; private set; }
        
        private static readonly int TransitionID = Shader.PropertyToID(TransitionProperty);
        [SerializeField] private Image transitionImage;
        private Material _mat;
        [SerializeField] private AnimationCurve transitionCurve;
        [SerializeField] private float transitionDuration = 1f;
        private const string TransitionProperty = "_Transition";
        
        private Coroutine _currentTransition;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            
            Instance = this;
            _mat = new Material(transitionImage.material);
            transitionImage.material = _mat;
        }

        public Coroutine TransitionCoroutine(bool fadeIn)
        {
            if (_currentTransition != null) StopCoroutine(_currentTransition);
            _currentTransition = StartCoroutine(Transition(fadeIn));
            return _currentTransition;
        }
        private IEnumerator Transition(bool fadeIn)
        {
            float startValue = fadeIn ? 0f : 1f;
            float endValue = fadeIn ? 1f : 0f;

            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                float value = Mathf.Lerp(startValue, endValue, transitionCurve.Evaluate(t));
                _mat.SetFloat(TransitionID, value);
                yield return null;
            }

            _mat.SetFloat(TransitionID, endValue);
        }
    }
}
