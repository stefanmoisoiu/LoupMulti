using Game.Common;
using Player.General_UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.General_UI.Inventory
{
    public class InventoryItem : MonoBehaviour, IInfoTooltipDataProvider
    {
        private OwnedItemData _ownedItemData;
        private Item _item;

        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        
        public void Setup(OwnedItemData data)
        {
            _ownedItemData = data;
            _item = ItemRegistry.Instance.GetItem(data.ItemRegistryIndex);
            icon.sprite = _item.Info.Icon;
            levelText.text = $"LVL {data.Level+1}";
        }

        public void SetActive(bool active)
        {
            canvasGroup.alpha = active ? 1 : 0;
            canvasGroup.blocksRaycasts = active;
        }
        
        public string GetHeaderText()
        {
            return _item.Info.Name;
        }

        public string GetDescriptionText()
        {
            return _item.Info.Description;
        }

        public Sprite GetMainIcon()
        {
            return null;
        }

        public bool ShouldShowPrice(out int price, out ResourceType resourceType)
        {
            price = 0;
            resourceType = ResourceType.Common;
            return false;
        }

        public Color GetHeaderColor()
        {
            return Color.white;
        }
    }
}