using System;
using System.Collections;
using Game.Data.Extensions;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class HealthModelEffect : NetworkBehaviour
{
    private static readonly int RimSize = Shader.PropertyToID("_RimSize");
    private static readonly int RimColor = Shader.PropertyToID("_RimColor");

    [TitleGroup("References")]
    [SerializeField] private Renderer[] affectedRenderers;
    private Material[] mats;

    [TitleGroup("Properties")]
    [SerializeField] private float durationPerHealth = 0.1f;
    [TitleGroup("Properties")] [SerializeField]
    private AnimationCurve curve;

    
    [TitleGroup("Properties")]
    [SerializeField] private float size;
    [TitleGroup("Properties")]
    [SerializeField] private Color damageColor = Color.red;
    [TitleGroup("Properties")]
    [SerializeField] private Color healColor = Color.green;
    
    private Coroutine coroutine;
    
    private void GenerateMaterials()
    {
        mats = new Material[affectedRenderers.Length];
        for (int i = 0; i < affectedRenderers.Length; i++)
        {
            mats[i] = new(affectedRenderers[i].material);
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

    private void Start()
    {
        GenerateMaterials();
        
        PlayerHealth.OnHealthChangedClient += OnHealthChanged;
    }
    private void OnDisable()
    {
        PlayerHealth.OnHealthChangedClient -= OnHealthChanged;
    }
    
    private void OnHealthChanged(ushort previousHealth, ushort newHealth, ulong origin)
    {
        if (origin != OwnerClientId) return;
        if (coroutine != null) StopCoroutine(coroutine);
        bool heal = newHealth > previousHealth;
        float duration = Mathf.Abs(newHealth - previousHealth) * durationPerHealth;
        coroutine = StartCoroutine(Anim(heal,duration));
    }

    private IEnumerator Anim(bool heal, float duration)
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
