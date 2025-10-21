using System;
using System.Collections;
using Game.Common;
using Game.Data.Extensions;
using Player.Health;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Player.Model.Health
{
    public class HealthModelEffect : NetworkBehaviour
    {
        private static readonly int RimSize = Shader.PropertyToID("_RimSize");
        private static readonly int RimColor = Shader.PropertyToID("_RimColor");

        [TitleGroup("References")]
        [SerializeField] private Renderer[] affectedRenderers;
        [TitleGroup("References")] [SerializeField]
        private MonoBehaviour healthScript;
        private IHealable healable => healthScript as IHealable;
        private IDamageable damageable => healthScript as IDamageable;

        
        private Material[] mats;

        [TitleGroup("Properties")]
        [SerializeField] private float duration = 1f;
        [TitleGroup("Properties")] [SerializeField]
        private AnimationCurve curve;

    
        [TitleGroup("Properties")]
        [SerializeField] private float size;
        [TitleGroup("Properties")]
        [SerializeField] private Color damageColor = Color.red;
        [TitleGroup("Properties")]
        [SerializeField] private Color healColor = Color.green;
    
        private Coroutine coroutine;

        private void OnEnable()
        {
            GenerateMaterials();
            
            if (healable != null)
                healable.OnHealed += OnHeal;
            if (damageable != null)
                damageable.OnDamaged += OnDamage;
        }

        private void OnDisable()
        {
            if (healable != null)
                healable.OnHealed -= OnHeal;
            if (damageable != null)
                damageable.OnDamaged -= OnDamage;
        }

        private void OnHeal(ushort amount)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(Anim(true));
        }
        private void OnDamage(ushort amount)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(Anim(false));
        }
    
        private void GenerateMaterials()
        {
            mats = new Material[affectedRenderers.Length];
            for (int i = 0; i < affectedRenderers.Length; i++)
            {
                mats[i] = new(affectedRenderers[i].sharedMaterial);
                affectedRenderers[i].material = mats[i];
            }
        }

        private void UpdateMats(float adv, bool heal)
        {
            adv = curve.Evaluate(adv);
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                mats[i].SetFloat(RimSize, Mathf.Lerp(size,10,adv));
                Color baseColor = heal ? healColor : damageColor;
                mats[i].SetColor(RimColor, Color.Lerp(baseColor,new Color(baseColor.r,baseColor.g,baseColor.b,Mathf.Lerp(baseColor.a,0,adv)),adv));
            }
        }
        private IEnumerator Anim(bool heal)
        {
            float adv = 0;
            while (adv < 1)
            {
                adv += Time.deltaTime / duration;
                UpdateMats(adv, heal);
                yield return null;
            }
            UpdateMats(1, heal);
        }
    }
}
