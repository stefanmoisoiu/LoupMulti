using Game.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player.General_UI.Shop.ShopSelectionInfo
{
    public class ShopInfoBubbleTarget : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        public string Name { get; private set; } = "Default Name";
        public string Description { get; private set; } = "Default Description";
        
        public int Price { get; private set; } = 0;
        
        public ResourceType ResourceType { get; private set; } = ResourceType.Common;
        
        public void SetData(string newName = "Default Name", string newDescription = "Default Description", int newPrice = 0, ResourceType resourceType = ResourceType.Common)
        {
            Name = newName;
            Description = newDescription;
            Price = newPrice;
            ResourceType = resourceType;
        }

        public void OnPointerEnter(PointerEventData eventData = null)
        {
            ShopInfoBubble.Instance.EnteredTarget(this, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData = null)
        {
            // Cache la bulle
            ShopInfoBubble.Instance.ExitedTarget(this);
        }
    }
}