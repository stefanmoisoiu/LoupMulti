using System;
using System.Collections;
using AYellowpaper.SerializedCollections;
using Base_Scripts;
using UnityEngine;

namespace Player.General_UI
{
    public class PCanvas : MonoBehaviour
    {
        public static PCanvas Instance;
        public static GameObject Canvas => Instance.gameObject;
        private static CanvasReferences _canvasReferences;
        private static CanvasGroup _canvasGroup;
        public static SerializedDictionary<string, GameObject> CanvasObjects => _canvasReferences?.References;

        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private AnimationCurve transitionCurve;
        private Coroutine _transitionCoroutine;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _canvasReferences = GetComponent<CanvasReferences>();
            if (_canvasReferences == null)
                Debug.LogError("CanvasReferences component not found on PCanvas GameObject.");
            
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                Debug.LogError("CanvasGroup component not found on PCanvas GameObject.");
            else
            {
                _canvasGroup.alpha = 0;
            }
        }

        private void OnDisable()
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        public void SetCanvasVisible(bool visible)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            _canvasGroup.alpha = visible ? 1f : 0f;
        }

        public void TransitionCanvasVisible(bool visible)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionCoroutine(visible));
        }
        
        private IEnumerator TransitionCoroutine(bool visible)
        {
            if (_canvasGroup == null) yield break;
            float startValue = _canvasGroup.alpha;
            float endValue = visible ? 1f : 0f;
            
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                t = transitionCurve.Evaluate(t);
                _canvasGroup.alpha = Mathf.Lerp(startValue, endValue, t);
                yield return null;
            }
        }
    }
}