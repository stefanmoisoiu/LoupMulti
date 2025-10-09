using System.Collections;
using Game.Common;
using Player.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.General_UI.Shop.ShopSelectionInfo
{
    public class ShopInfoBubble : PNetworkBehaviour
    {
        public static ShopInfoBubble Instance { get; private set; }

        private TextMeshProUGUI nameText;
        private TextMeshProUGUI priceText;
        private Image priceIcon;
        private TextMeshProUGUI descriptionText;
        private const string InfoBubbleTag = "ShopInfoBubble";
        private Transform infoBubble;
        private CanvasGroup canvasGroup;

        private bool _bubbleActive;

        [SerializeField] private AnimationCurve alphaCurve;
        [SerializeField] private float alphaAnimLength;
        private Coroutine _alphaCoroutine;

        public bool BubbleActive => _bubbleActive;
        private ShopInfoBubbleTarget _currentTarget;
        public ShopInfoBubbleTarget CurrentTarget => _currentTarget;

        [SerializeField] private Sprite commonResourceIcon;
        [SerializeField] private Sprite rareResourceIcon;


        protected override void StartAnyOwner()
        {
            infoBubble = PCanvas.CanvasObjects[InfoBubbleTag].transform;
            canvasGroup = infoBubble.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            nameText = infoBubble.GetChild(0).Find("NameText").GetComponent<TextMeshProUGUI>();
            priceText = infoBubble.GetChild(0).Find("Price").Find("Amount").GetComponent<TextMeshProUGUI>();
            priceIcon = infoBubble.GetChild(0).Find("Price").Find("Icon").GetComponent<Image>();
            descriptionText = infoBubble.GetChild(0).Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            Instance = this;
            Hide();
        }

        /// <summary>
        /// Affiche la bulle avec les infos fournies.
        /// </summary>
        public void Show(ShopInfoBubbleTarget target, Vector2 position)
        {
            if (_bubbleActive) return;
            _bubbleActive = true;

            if (_alphaCoroutine != null) StopCoroutine(_alphaCoroutine);
            _alphaCoroutine = StartCoroutine(AnimateAlpha(true));

            nameText.text = target.Name;
            priceText.text = target.Price.ToString();
            descriptionText.text = target.Description;
            priceIcon.sprite = target.ResourceType == ResourceType.Common ? commonResourceIcon : rareResourceIcon;
            // Positionne la bulle à la position donnée
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameText.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionText.rectTransform);


            infoBubble.position = new Vector3(position.x, position.y, infoBubble.position.z);
        }

        /// <summary>
        /// Cache la bulle.
        /// </summary>
        public void Hide()
        {
            if (!_bubbleActive) return;
            _bubbleActive = false;

            if (_alphaCoroutine != null) StopCoroutine(_alphaCoroutine);
            _alphaCoroutine = StartCoroutine(AnimateAlpha(false));
        }

        public void EnteredTarget(ShopInfoBubbleTarget target, Vector2 position)
        {
            if (_bubbleActive) return;
            _currentTarget = target;
            Show(target, position);
        }

        public void ExitedTarget(ShopInfoBubbleTarget target)
        {
            if (!_bubbleActive || _currentTarget != target) return;
            _currentTarget = null;
            Hide();
        }

        private IEnumerator AnimateAlpha(bool fadeIn)
        {
            float timeElapsed = 0;
            while (timeElapsed < alphaAnimLength)
            {
                timeElapsed += Time.deltaTime;
                float alpha = alphaCurve.Evaluate(timeElapsed / alphaAnimLength);
                canvasGroup.alpha = fadeIn ? alpha : 1 - alpha;
                yield return null;
            }

            canvasGroup.alpha = fadeIn ? 1 : 0;
            _alphaCoroutine = null;
        }
    }
}