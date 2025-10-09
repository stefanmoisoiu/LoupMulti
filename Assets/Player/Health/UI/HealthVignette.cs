using System;
using System.Collections;
using Game.Data.Extensions;
using Player.Networking;
using UnityEngine;

namespace Player.Health
{
    public class HealthVignette : PNetworkBehaviour
    {
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int MultID = Shader.PropertyToID("_Mult");
        [SerializeField] private Material vignetteMat;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float lerpTime = 0.5f;
        [SerializeField] private float maxOpacityHealthChange = 30;
        [SerializeField] private float mult = 30;
        

        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color damageColor = Color.red;


        private Coroutine coroutine;

        protected override void StartOnlineOwner()
        {
            if (vignetteMat == null)
            {
                Debug.LogError("Vignette material is not assigned.");
                return;
            }

            PlayerDataHealth.OnOwnerPlayerHealthChanged += OnHealthChanged;
        }

        protected override void DisableAnyOwner()
        {
            PlayerDataHealth.OnOwnerPlayerHealthChanged -= OnHealthChanged;
            vignetteMat.SetFloat(MultID, 0);
        }

        private void OnHealthChanged(ushort previousHealth, ushort newHealth)
        {
            if (coroutine != null) StopCoroutine(coroutine);

            ushort delta = (ushort)Math.Abs(previousHealth - newHealth);
            if (delta > 0) coroutine = StartCoroutine(Animate(newHealth > previousHealth, delta));
            Debug.Log(name);
        }

        private IEnumerator Animate(bool heal, float amount)
        {
            float opacityMult = Mathf.Clamp01(amount / maxOpacityHealthChange) * mult;
            vignetteMat.SetColor(ColorID, heal ? healColor : damageColor);
            float time = 0;
            while (time < lerpTime)
            {
                time += Time.deltaTime;
                vignetteMat.SetFloat(MultID, curve.Evaluate(time / lerpTime) * opacityMult);
                yield return null;
            }

            vignetteMat.SetFloat(MultID, 0);
        }
    }
}