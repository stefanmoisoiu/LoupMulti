using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Data;
using Input;
using Player.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.General_UI.Inventory
{
    public class InventoryManager : PNetworkBehaviour
    {
        [SerializeField] private GameObject ownedPassivePrefab;
        
        private const string AbilitiesLayoutTag = "AbilitiesLayout";
        private const string InventoryLayoutTag = "InventoryLayout";
        private const string InventoryTag = "Inventory";
        
        private Transform _abilitiesLayout;
        private Transform _inventoryLayout;
        private RectTransform _inventoryRect;
        private CanvasGroup _inventoryCanvasGroup;

        struct ItemInstance
        {
            public OwnedItemData OwnedData;
            public GameObject Instance;
            public InventoryItem InventoryItem;

            public ItemInstance(OwnedItemData ownedData, GameObject instance)
            {
                OwnedData = ownedData;
                Instance = instance;
                InventoryItem = instance.GetComponent<InventoryItem>();
                InventoryItem.Setup(ownedData);
            }
        }

        private List<ItemInstance> _passiveInstances = new();
        private List<ItemInstance> _activeInstances = new();
        private ItemInstance _drillInstance = new();
        
        private List<OwnedItemData> _cachedOwnedPassives;
        private List<OwnedItemData> _cachedOwnedActives;
        private OwnedItemData _cachedOwnedDrill;

        protected override void StartAnyOwner()
        {
            _inventoryLayout = PCanvas.CanvasObjects[InventoryLayoutTag].transform;
            _inventoryRect = _inventoryLayout.GetComponent<RectTransform>();
            _abilitiesLayout = PCanvas.CanvasObjects[AbilitiesLayoutTag].transform;
            _inventoryCanvasGroup = PCanvas.CanvasObjects[InventoryTag].GetComponent<CanvasGroup>();
            
            // DataManager.OnEntryUpdatedOwner += OnDataEntryUpdated;
            InputManager.OnInventoryOpened += OpenInventory;
            InputManager.OnInventoryClosed += CloseInventory;

            GameObject drillObject = _abilitiesLayout.GetChild(0).GetChild(0).gameObject;
            ItemInstance drillInstance = new(default, drillObject);
            _drillInstance  = drillInstance;
            _activeInstances = new();
            for (int i = 1; i < _abilitiesLayout.childCount; i++)
            {
                GameObject child = _abilitiesLayout.GetChild(i).GetChild(0).gameObject;
                ItemInstance instance = new(default, child);
                instance.InventoryItem.SetActive(false);
                _activeInstances.Add(instance);
            }
            UpdateActives();
        }
        protected override void DisableAnyOwner()
        {
            // DataManager.OnEntryUpdatedOwner -= OnDataEntryUpdated;
            InputManager.OnInventoryOpened -= OpenInventory;
            InputManager.OnInventoryClosed -= CloseInventory;
            
            CursorManager.Instance.ReleaseCursorUnlock(this);
        }
        
        private void OpenInventory()
        {
            UpdatePassives();
            UpdateActives();
            SetInventoryOpened(true);
        }
        private void CloseInventory() => SetInventoryOpened(false);
        
        private void SetInventoryOpened(bool opened)
        {
            _inventoryCanvasGroup.alpha = opened ? 1 : 0;
            _inventoryCanvasGroup.blocksRaycasts = opened;
            _inventoryCanvasGroup.interactable = opened;
            
            if (opened)
                CursorManager.Instance.RequestCursorUnlock(this);
            else
                CursorManager.Instance.ReleaseCursorUnlock(this);
        }

        private void OnDataEntryUpdated(PlayerData previousData, PlayerData newData)
        {
            UpdatePassives();
            UpdateActives();
        }

        private void UpdateActives()
        {
            OwnedItemData ownedDrillData = DataManager.Instance[OwnerClientId].inGameData.ownedDrillData;
            if (!ownedDrillData.Equals(_cachedOwnedDrill))
            {
                _cachedOwnedDrill = ownedDrillData;
            
                _drillInstance.OwnedData = ownedDrillData;
                _drillInstance.InventoryItem.Setup(ownedDrillData);
                _drillInstance.InventoryItem.SetActive(true);
            }
            
            List<OwnedItemData> ownedItemDatas = DataManager.Instance[OwnerClientId].inGameData.ownedItems;
            List<OwnedItemData> ownedActives = ownedItemDatas.FindAll(x => ItemRegistry.Instance.GetItem(x.ItemRegistryIndex).Type == Item.ItemType.Ability);
            
            if (ownedActives.Equals(_cachedOwnedActives)) return;
            _cachedOwnedActives = ownedActives;
            
            for (int i = 0; i < _activeInstances.Count; i++)
            {
                ItemInstance instance = _activeInstances[i];

                if (i >= ownedActives.Count)
                {
                    instance.OwnedData = default;
                    instance.InventoryItem.SetActive(false);
                }
                else
                {
                    if (!instance.OwnedData.Equals(ownedActives[i]))
                        instance = new(ownedActives[i], instance.Instance);
                    instance.InventoryItem.SetActive(true);
                }
                
                _activeInstances[i] = instance;
            }
            
        }
        private void UpdatePassives()
        {
            List<OwnedItemData> ownedItemDatas = DataManager.Instance[OwnerClientId].inGameData.ownedItems;
            List<OwnedItemData> ownedPassives = ownedItemDatas.FindAll(x => ItemRegistry.Instance.GetItem(x.ItemRegistryIndex).Type == Item.ItemType.Perk);
            
            if (ownedPassives.Equals(_cachedOwnedPassives)) return;
            _cachedOwnedPassives = ownedPassives;

            for (int i = 0; i < _passiveInstances.Count; i++)
            {
                ItemInstance instance = _passiveInstances[i];

                if (i >= ownedPassives.Count)
                {
                    instance.OwnedData = default;
                    instance.InventoryItem.SetActive(false);
                }
                else
                {
                    if (!instance.OwnedData.Equals(ownedPassives[i]))
                        instance = new(ownedPassives[i], instance.Instance);
                    instance.InventoryItem.SetActive(true);
                }
                
                _passiveInstances[i] = instance;
            }
            
            for (int i = _passiveInstances.Count; i < ownedPassives.Count; i++)
            {
                GameObject newInstance = Instantiate(ownedPassivePrefab, _inventoryLayout);
                ItemInstance instance = new(ownedPassives[i], newInstance);
                _passiveInstances.Add(instance);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_inventoryRect.GetComponent<RectTransform>());
        }
    }
}