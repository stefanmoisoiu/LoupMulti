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

        private ushort _lastShopOpenedHealth;
        
        private HealthBarEffect _healthBarEffect;
        private Button _shopHealthBarButton;
        private CanvasGroup _buttonCanvasGroup;
        protected override void StartAnyOwner()
        {
            _healthBarEffect = PCanvas.CanvasObjects[shopHealthBarTag].GetComponent<HealthBarEffect>();
            
            _shopHealthBarButton = PCanvas.CanvasObjects[shopHealthBarButtonTag].GetComponent<Button>();
            _buttonCanvasGroup = PCanvas.CanvasObjects[shopHealthBarButtonTag].GetComponent<CanvasGroup>();
            _buttonCanvasGroup.alpha = 1;

            _lastShopOpenedHealth = ushort.MaxValue;
            
            _shopHealthBarButton.onClick.AddListener(BuyHealth);
            
            ShopManager.OnShopOpenedChanged += ShopOpenedChanged;
            PlayerHealthHelper.OnPlayerHealthChangedOwner += HealthChanged;
            DataManager.OnEntryUpdatedOwner += OnEntryUpdatedOwner;
        }

        protected override void DisableAnyOwner()
        {
            ShopManager.OnShopOpenedChanged -= ShopOpenedChanged;
            PlayerHealthHelper.OnPlayerHealthChangedOwner -= HealthChanged;
            DataManager.OnEntryUpdatedOwner -= OnEntryUpdatedOwner;
            
            _shopHealthBarButton?.onClick.RemoveListener(BuyHealth);
        }

        private void BuyHealth()
        {
            ushort health = DataManager.Instance[OwnerClientId].inGameData.health;
            ushort maxHealth = DataManager.Instance[OwnerClientId].inGameData.maxHealth;
            if (health >= maxHealth)
            {
                Debug.Log("Tried to buy health at max health.");
                return;
            }
            GameManager.Instance.ShopManager.BuyHealth(1);
        }
        private void HealthChanged(ushort previousHealth, ushort newHealth)
        {
            if (!GameManager.Instance.ShopManager.ShopOpened) return;
            _healthBarEffect.UpdateHealthBar(previousHealth, newHealth, DataManager.Instance[OwnerClientId].inGameData.maxHealth);
        }

        private void ShopOpenedChanged(bool opened, ShopManager shopManager)
        {
            if (!opened) return;

            ushort health = DataManager.Instance[OwnerClientId].inGameData.health;
            ushort maxHealth = DataManager.Instance[OwnerClientId].inGameData.maxHealth;
            if (_lastShopOpenedHealth == ushort.MaxValue) _lastShopOpenedHealth = maxHealth;
            
            _healthBarEffect.UpdateHealthBar(_lastShopOpenedHealth, health, maxHealth);
            
            _lastShopOpenedHealth = health;
        }
        
        private void OnEntryUpdatedOwner(PlayerData previousData, PlayerData newData)
        {
            InGameData igData = newData.inGameData;
            
            bool hasEnoughResources = igData.resources.HasEnough(ResourceType.Common, 1);
            bool hasLifeToRestore = igData.health < igData.maxHealth;
            
            SetBuyButtonEnabled(hasEnoughResources && hasLifeToRestore);
        }

        private void SetBuyButtonEnabled(bool buttonEnabled)
        {
            _buttonCanvasGroup.alpha = buttonEnabled ? 1 : .5f;
            _buttonCanvasGroup.interactable = buttonEnabled;
        }
    }
}