using System;
using System.Collections;
using System.Collections.Generic;
using Input;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Model.Grabber
{
    public class GrabberShowHideAnim : MonoBehaviour
    {
        private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
        [SerializeField] private Renderer[] armRenderers;
        
        
        private Coroutine mainCoroutine;
        private Coroutine toolCoroutine;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float duration;
        

        [SerializeField] private bool shownOnStart = false;
        
        
        private bool armShown = false;
        public bool ArmShown => armShown;

        private void Start()
        {
            UpdateShowHide(armRenderers, shownOnStart ? 0f : 1f);
        }

        public IEnumerator ShowHideTool(bool show, Renderer[] toolRenderers)
        {
            if (!armShown) yield break;

            if (toolCoroutine != null) StopCoroutine(toolCoroutine);
            toolCoroutine = StartCoroutine(ShowHide(show, toolRenderers));

            yield return toolCoroutine;
        }
        public IEnumerator ShowHideArm(bool show)
        {
            if (armShown == show) yield break;
            armShown = show;

            if (mainCoroutine != null) StopCoroutine(mainCoroutine);
            mainCoroutine = StartCoroutine(ShowHide(show, armRenderers));
            
            yield return mainCoroutine;
        }
        
        private IEnumerator ShowHide(bool show, Renderer[] renderers)
        {
            float adv = 0;
            float start = show ? 1f : 0f;
            float end = show ? 0f : 1f;

            while (adv < 1)
            {
                adv += Time.deltaTime / duration;
                if (adv > 1) break;
                UpdateShowHide(renderers, curve.Evaluate(Mathf.Lerp(start, end, adv)));
                yield return null;
            }
            UpdateShowHide(renderers, end);
        }

        public void UpdateShowHide(Renderer[] renderers, float adv)
        {
            foreach (var _renderer in renderers)
            {
                if (_renderer == null) continue;
                foreach (var mat in _renderer.materials)
                {
                    if (mat == null) continue;
                    mat.SetFloat(DissolveAmountID, adv);
                }
            }
        }
    }
}