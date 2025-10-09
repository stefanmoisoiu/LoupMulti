using System.Collections;
using Base_Scripts;
using Game.Common;
using Game.Data;
using Game.Data.Extensions;
using Game.Game_Loop;
using Game.Upgrade.Shop;
using Player.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Player.General_UI.Shop
{
    public class ShopHealthBar : PNetworkBehaviour
    {
        [SerializeField] private string shopHealthBarTag = "ShopHealthBar";
        [SerializeField] private string shopHealthBarButtonTag = "ShopHealthBarButton";

        private ushort lastShopOpenedHealth = GameSettings.PlayerMaxHealth;
        
        private HealthBarEffect healthBarEffect;
        private Button shopHealthBarButton;
        private CanvasGroup buttonCanvasGroup;
        protected override void StartAnyOwner()
        {
            healthBarEffect = PCanvas.CanvasObjects[shopHealthBarTag].GetComponent<HealthBarEffect>();
            
            shopHealthBarButton = PCanvas.CanvasObjects[shopHealthBarButtonTag].GetComponent<Button>();
            buttonCanvasGroup = PCanvas.CanvasObjects[shopHealthBarButtonTag].GetComponent<CanvasGroup>();
            buttonCanvasGroup.alpha = 1;
            
            shopHealthBarButton.onClick.AddListener(BuyHealth);
            
            ShopManager.OnShopOpenedChanged += ShopOpenedChanged;
            PlayerDataHealth.OnOwnerPlayerHealthChanged += HealthChanged;
            DataManager.OnEntryUpdatedOwner += OnEntryUpdatedOwner;
        }

        protected override void DisableAnyOwner()
        {
            ShopManager.OnShopOpenedChanged -= ShopOpenedChanged;
            PlayerDataHealth.OnOwnerPlayerHealthChanged -= HealthChanged;
            DataManager.OnEntryUpdatedOwner -= OnEntryUpdatedOwner;
            
            shopHealthBarButton?.onClick.RemoveListener(BuyHealth);
        }

        private void BuyHealth()
        {
            ushort health = DataManager.Instance[OwnerClientId].inGameData.health;
            if (health >= GameSettings.PlayerMaxHealth)
            {
                Debug.Log("Tried to buy health at max health.");
                return;
            }
            GameManager.Instance.ShopManager.BuyHealth(1);
        }
        private void HealthChanged(ushort previousHealth, ushort newHealth)
        {
            if (!GameManager.Instance.ShopManager.ShopOpened) return;
            healthBarEffect.UpdateHealthBar(previousHealth, newHealth, GameSettings.PlayerMaxHealth);
        }

        private void ShopOpenedChanged(bool opened, ShopManager shopManager)
        {
            if (!opened) return;

            ushort health = DataManager.Instance[OwnerClientId].inGameData.health;
            healthBarEffect.UpdateHealthBar(lastShopOpenedHealth, health, GameSettings.PlayerMaxHealth);
            
            lastShopOpenedHealth = health;
        }
        
        private void OnEntryUpdatedOwner(PlayerData previousData, PlayerData newData)
        {
            InGameData igData = newData.inGameData;
            
            bool hasEnoughResources = igData.resources.HasEnough(ResourceType.Common, 1);
            bool hasLifeToRestore = igData.health < GameSettings.PlayerMaxHealth;
            
            SetBuyButtonEnabled(hasEnoughResources && hasLifeToRestore);
        }

        private void SetBuyButtonEnabled(bool buttonEnabled)
        {
            buttonCanvasGroup.alpha = buttonEnabled ? 1 : .5f;
            buttonCanvasGroup.interactable = buttonEnabled;
        }
    }
}