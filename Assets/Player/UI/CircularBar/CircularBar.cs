using System.Collections;
using Player.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Player.UI.CircularBar
{
    public class CircularBar : PNetworkBehaviour
    {
        
        private static readonly int AdvID = Shader.PropertyToID("_adv");
        internal Material target;
        
        [SerializeField] private float lerpTime = 0.2f;
        [SerializeField] private AnimationCurve lerpCurve;
        private Coroutine coroutine;
        internal void AnimateBar(float previoousAdv, float newAdv)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(Animate(previoousAdv, newAdv));
        }
        
        private IEnumerator Animate(float previousAdv, float newAdv)
        {
            float time = 0;
            while (time < lerpTime)
            {
                time += Time.deltaTime;
                float value = Mathf.Lerp(previousAdv, newAdv, lerpCurve.Evaluate(time / lerpTime));
                SetBarAdv(value);
                yield return null;
            }
        }
        
        internal void SetBarAdv(float value)
        {
            target.SetFloat(AdvID, value);
        }
    }
}