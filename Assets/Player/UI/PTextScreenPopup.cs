using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PTextScreenPopup : PNetworkBehaviour
{
    private RectTransform _canvasObj;
    

    [SerializeField] private GameObject textPopupPrefab;
    [SerializeField] private PCanvas canvas;
    
    
    private List<Popup> _popups = new List<Popup>();
    protected override void StartAnyOwner()
    {
        _canvasObj = canvas.Canvas.GetComponent<RectTransform>();
    }

    public void CreatePopup(PopupData popupData, Vector2 position, float rotation = 0)
    {
        Transform instance = Instantiate(textPopupPrefab, _canvasObj).transform;
        TMP_Text text = instance.GetChild(0).GetComponent<TMP_Text>();
        
        text.text = popupData.text;
        text.color = popupData.color;
        text.fontSize = popupData.fontSize;
        
        instance.localPosition = position;
        instance.localRotation = Quaternion.Euler(0, 0, rotation);
        
        Popup popup = new Popup
        {
            data = popupData,
            text = text,
            OnEnd = OnPopupEnd
        };
        _popups.Add(popup);
    }

    protected override void UpdateAnyOwner()
    {
        for (int i = 0; i < _popups.Count; i++)
            _popups[i].Step(Time.deltaTime);
    }

    private void OnPopupEnd(Popup popup)
    {
        _popups.Remove(popup);
    }

    [Serializable]
    public class PopupData
    {
        public string text = "Popup";
        public Color color = Color.white;
        public int fontSize = 32;
        public float lifetime = 1;
        public Vector2 endOffset;
        public AnimationCurve xCurve = AnimationCurve.Constant(0,1,0);
        public AnimationCurve yCurve = AnimationCurve.Constant(0,1,0);
        public AnimationCurve sizeCurve = AnimationCurve.Constant(0,1,1);
        public AnimationCurve rotCurve = AnimationCurve.Constant(0,1,0);
        public AnimationCurve opacityCurve = AnimationCurve.Constant(0,1,1);


        public bool IsEnd(float currentLifetime)
        {
            return currentLifetime >= lifetime;
        }
    }

    public class Popup
    {
        public PopupData data;
        public float time = 0;
        public TMP_Text text;
        
        public Action<Popup> OnEnd;
        
        public void ApplyEffects(float currentLifetime)
        {
            float x = data.xCurve.Evaluate(currentLifetime / data.lifetime) * data.endOffset.x;
            float y = data.yCurve.Evaluate(currentLifetime / data.lifetime) * data.endOffset.y;
            float size = data.sizeCurve.Evaluate(currentLifetime / data.lifetime);
            float rot = data.rotCurve.Evaluate(currentLifetime / data.lifetime);
            float opacity = data.opacityCurve.Evaluate(currentLifetime / data.lifetime);
            
            text.rectTransform.localPosition = new Vector3(x, y, 0);
            text.rectTransform.localScale = new Vector3(size, size, 1);
            text.rectTransform.localRotation = Quaternion.Euler(0, 0, rot);
            text.color = new Color(text.color.r,text.color.g,text.color.b, opacity);
        }
        
        public void Step(float deltaTime)
        {
            time += deltaTime;
            ApplyEffects(time);
            
            if (data.IsEnd(time))
            {
                DestroyObj();
            }
        }
        
        public void DestroyObj()
        {
            Destroy(text.transform.parent.gameObject);
            OnEnd?.Invoke(this);
        }
        
        public bool IsEnd()
        {
            return data.IsEnd(time);
        }
    }
}
