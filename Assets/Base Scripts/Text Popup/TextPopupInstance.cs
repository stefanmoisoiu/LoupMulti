using System.Collections;
using TMPro;
using UnityEngine;

namespace Base_Scripts.Text_Popup
{
    public class TextPopupInstance : MonoBehaviour
    {
        [SerializeField] private float lifetime;

        [SerializeField] private Transform textParent;
        [SerializeField] private TMP_Text text;
        
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AnimationCurve alphaCurve;
        
        
        [SerializeField] private float verticalMoveDistance = 0.5f;
        [SerializeField] private AnimationCurve moveCurve;
        
        [SerializeField] private AnimationCurve sizeCurve;
        
        private float _time;
        
        private void Start()
        {
            StartCoroutine(Life());
        }

        private IEnumerator Life()
        {
            float t = 0;

            while (t < lifetime)
            {
                float adv = t / lifetime;
                textParent.localPosition = new Vector3(0, verticalMoveDistance * moveCurve.Evaluate(adv), 0);
                textParent.localScale = Vector3.one * sizeCurve.Evaluate(adv);
                canvasGroup.alpha = alphaCurve.Evaluate(adv);
                t += Time.deltaTime;
                yield return null;
            }
            
            Destroy(gameObject);
        }

        public void SetData(string value, Color color)
        {
            text.text = value;
            text.color = color;
        }
    }
}