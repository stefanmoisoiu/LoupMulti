using Game.Game_Loop;
using Game.Game_Loop.Round;
using Game.Upgrade.Shop;
using Input;
using Player.Networking;
using Player.Perks.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Player.General_UI.Shop
{
    public class ShopOpen : PNetworkBehaviour
    {
        private const string ShopCanvasTag = "ShopCanvas";
        private const string OpenShopKeyTag = "OpenShopKey";
        private const string CloseShopButtonTag = "CloseShopButton";
        
        private CanvasGroup openShopCanvas;
        private CanvasGroup shopCanvas;

        protected override void StartAnyOwner()
        {
            shopCanvas = PCanvas.CanvasObjects[ShopCanvasTag].GetComponent<CanvasGroup>();
            shopCanvas.alpha = 0;
            shopCanvas.interactable = false;
            shopCanvas.blocksRaycasts = false;
            
            openShopCanvas = PCanvas.CanvasObjects[OpenShopKeyTag].GetComponent<CanvasGroup>();
            openShopCanvas.alpha = 0f;
            openShopCanvas.interactable = false;
            
            Button closeShopButton = PCanvas.CanvasObjects[CloseShopButtonTag].GetComponent<Button>();
            closeShopButton.onClick.RemoveAllListeners();
            closeShopButton.onClick.AddListener(() => GameManager.Instance.ShopManager.SetOpened(false));

            InputManager.OnShopOpened += OpenCloseShopKey;
            ItemSelectionWindow.OnItemChosen += ItemChosen;
            GameLoopEvents.OnRoundStateChangedAll += OnRoundStateChanged;
            
            ShopManager.OnShopOpenedChanged += OnShopOpenedChanged;
        }

        private void OpenCloseShopKey()
        {
            if (GameManager.CurrentGameState != GameManager.GameState.InGame) return;
            if (GameManager.Instance.GameLoop.GameLoopEvents.roundState.Value != GameRoundState.Upgrade) return;
            
            GameManager.Instance.ShopManager.SetOpened(!GameManager.Instance.ShopManager.ShopOpened);
        }

        private void ItemChosen(ushort index)
        {
            if (GameManager.Instance.GameLoop.GameLoopEvents.roundState.Value != GameRoundState.Upgrade) return;
            GameManager.Instance.ShopManager.SetOpened(true);
        }

        private void OnRoundStateChanged(GameRoundState newState, float serverTime)
        {
            SetOpenShopCanvasVisibility(newState == GameRoundState.Upgrade);
            if (newState != GameRoundState.Upgrade) GameManager.Instance.ShopManager.SetOpened(false);
        }

        protected override void DisableAnyOwner()
        {
            ShopManager.OnShopOpenedChanged -= OnShopOpenedChanged;
        }

        private void OnShopOpenedChanged(bool opened, ShopManager shopManager)
        {
            SetOpenShopCanvasVisibility(!opened);
            SetShopCanvasVisibility(opened);
            Cursor.lockState = opened ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = opened;
        }
        
        private void SetShopCanvasVisibility(bool visible)
        {
            shopCanvas.alpha = visible ? 1f : 0f;
            shopCanvas.interactable = visible;
            shopCanvas.blocksRaycasts = visible;
        }
        private void SetOpenShopCanvasVisibility(bool visible)
        {
            openShopCanvas.alpha = visible ? 1f : 0f;
            openShopCanvas.interactable = visible;
            openShopCanvas.blocksRaycasts = visible;
        }
    }
}