using Game.Upgrade.Shop;
using Player.Networking;
using UnityEngine;

namespace Player.UI.Perks
{
    public class ShopOpen : PNetworkBehaviour
    {
        private const string ShopCanvasTag = "ShopCanvas";
        
        private CanvasGroup canvasGroup;

        protected override void StartAnyOwner()
        {
            canvasGroup = PCanvas.CanvasObjects[ShopCanvasTag].GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            ShopManager.OnShopOpenedChanged += OnShopOpenedChanged;
        }

        protected override void DisableAnyOwner()
        {
            ShopManager.OnShopOpenedChanged -= OnShopOpenedChanged;
        }

        private void OnShopOpenedChanged(bool opened, ShopManager shopManager)
        {
            canvasGroup.alpha = opened ? 1f : 0f;
            canvasGroup.interactable = opened;
            canvasGroup.blocksRaycasts = opened;
            
            Cursor.lockState = opened ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = opened;
        }
    }
}