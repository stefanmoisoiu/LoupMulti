using System.Collections;
using Game.Common; // Pour ResourceType
using Player.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.General_UI.Tooltips
{
    public class InfoTooltipManager : PNetworkBehaviour
    {
        public static InfoTooltipManager Instance { get; private set; }

        private TextMeshProUGUI _headerText;
        private TextMeshProUGUI _descriptionText;
        
        private GameObject _mainIconRoot;
        private Image _mainIcon;
        
        private GameObject _priceRoot;
        private TextMeshProUGUI _priceText;
        private Image _priceIcon;
        
        private Transform _infoTooltip;
        private CanvasGroup _canvasGroup;

        [Header("Resource Sprites")]
        [SerializeField] private Sprite commonResourceIcon;
        [SerializeField] private Sprite rareResourceIcon;

        [Header("Animation")]
        [SerializeField] private AnimationCurve alphaCurve;
        [SerializeField] private float alphaAnimLength;
        
        private const string InfoTooltipTag = "InfoTooltip";
        private const string InfoTooltipHeaderTextTag = "InfoTooltipHeaderText";
        private const string InfoTooltipDescriptionTextTag = "InfoTooltipDescriptionText";
        private const string InfoTooltipIconTag = "InfoTooltipIcon";
        private const string InfoTooltipPriceTag = "InfoTooltipPrice";
        private const string InfoTooltipPriceValueTag = "InfoTooltipPriceValue";
        private const string InfoTooltipPriceIconTag = "InfoTooltipPriceIcon";
        private Coroutine _alphaCoroutine;

        private bool _tooltipActive;
        public bool TooltipActive => _tooltipActive;
        
        private IInfoTooltipDataProvider _currentTarget;
        public IInfoTooltipDataProvider CurrentTarget => _currentTarget;

        protected override void StartAnyOwner()
        {
            _infoTooltip = PCanvas.CanvasObjects[InfoTooltipTag].transform;
            _canvasGroup = _infoTooltip.GetComponent<CanvasGroup>();
            
            _headerText = PCanvas.CanvasObjects[InfoTooltipHeaderTextTag].GetComponent<TextMeshProUGUI>();
            _descriptionText = PCanvas.CanvasObjects[InfoTooltipDescriptionTextTag].GetComponent<TextMeshProUGUI>();
            
            _mainIconRoot = PCanvas.CanvasObjects[InfoTooltipIconTag];
            _mainIcon = _mainIconRoot.GetComponentInChildren<Image>();
            
            _priceRoot = PCanvas.CanvasObjects[InfoTooltipPriceTag];
            
            _priceText = PCanvas.CanvasObjects[InfoTooltipPriceValueTag].GetComponent<TextMeshProUGUI>();
            _priceIcon = PCanvas.CanvasObjects[InfoTooltipPriceIconTag].GetComponent<Image>();
            
            Instance = this;
            Hide(null, true); 
        }

        private void Show(IInfoTooltipDataProvider provider, Vector2 position)
        {
            if (provider == null) return;
            
            _tooltipActive = true;
            if (_alphaCoroutine != null) StopCoroutine(_alphaCoroutine);
            _alphaCoroutine = StartCoroutine(AnimateAlpha(true));

            // --- Titre & Description (Obligatoires) ---
            _headerText.text = provider.GetHeaderText();
            _headerText.color = provider.GetHeaderColor();
            _descriptionText.text = provider.GetDescriptionText();
            
            // --- Icône Principale (Optionnelle) ---
            Sprite icon = provider.GetMainIcon();
            bool hasMainIcon = icon != null;
            _mainIconRoot.SetActive(hasMainIcon);
            if(hasMainIcon) _mainIcon.sprite = icon;
            
            // --- Prix (Optionnel) ---
            bool showPrice = provider.ShouldShowPrice(out int price, out ResourceType resourceType);
            _priceRoot.SetActive(showPrice);
            if (showPrice)
            {
                _priceText.text = price.ToString();
                _priceIcon.sprite = resourceType == ResourceType.Common ? commonResourceIcon : rareResourceIcon;
            }
            
            // --- Repositionnement ---
            LayoutRebuilder.ForceRebuildLayoutImmediate(_headerText.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_descriptionText.rectTransform);

            _infoTooltip.position = new Vector3(position.x, position.y, _infoTooltip.position.z);
        }

        private void Hide(IInfoTooltipDataProvider provider, bool force = false)
        {
            if (!_tooltipActive && !force) return;
            if (_currentTarget != provider && !force) return;
            
            _tooltipActive = false;
            _currentTarget = null;

            if (_alphaCoroutine != null) StopCoroutine(_alphaCoroutine);
            _alphaCoroutine = StartCoroutine(AnimateAlpha(false));
        }

        public void EnteredTarget(IInfoTooltipDataProvider provider, Vector2 position)
        {
            if (_tooltipActive) return;
            _currentTarget = provider;
            Show(provider, position);
        }

        public void ExitedTarget(IInfoTooltipDataProvider provider)
        {
            Hide(provider);
        }

        private IEnumerator AnimateAlpha(bool fadeIn)
        {
            float timeElapsed = 0;
            float startAlpha = _canvasGroup.alpha;
            float targetAlpha = fadeIn ? 1 : 0;

            while (timeElapsed < alphaAnimLength)
            {
                timeElapsed += Time.deltaTime;
                float curveValue = alphaCurve.Evaluate(timeElapsed / alphaAnimLength);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _alphaCoroutine = null;
        }
    }
}