using Player.Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Player.UI.Shop.ShopSelectionInfo
{
    public class ShopInfoBubble : PNetworkBehaviour
    {
        public static ShopInfoBubble Instance { get; private set; }

        private TextMeshProUGUI nameText;
        private TextMeshProUGUI descriptionText;
        private const string InfoBubbleTag = "ShopInfoBubble";
        private Transform infoBubble;
        private CanvasGroup canvasGroup;
        
        private bool bubbleActive;
        public bool BubbleActive => bubbleActive;
        

        protected override void StartAnyOwner()
        {
            infoBubble = PCanvas.CanvasObjects[InfoBubbleTag].transform;
            canvasGroup = infoBubble.GetComponent<CanvasGroup>();
            nameText = infoBubble.GetChild(0).Find("NameText").GetComponent<TextMeshProUGUI>();
            descriptionText = infoBubble.GetChild(0).Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            Instance = this;
            Hide();
        }

        /// <summary>
        /// Affiche la bulle avec les infos fournies.
        /// </summary>
        public void Show(string title, string description, Vector2 position)
        {
            if (bubbleActive) return;
            bubbleActive = true;
            canvasGroup.alpha = 1;
        
            nameText.text = title;
            descriptionText.text = description;
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
            if (!bubbleActive) return;
            bubbleActive = false;
            canvasGroup.alpha = 0;
        }
    }
}