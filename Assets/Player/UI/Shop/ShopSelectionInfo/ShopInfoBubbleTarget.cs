using UnityEngine;
using UnityEngine.EventSystems;

namespace Player.UI.Shop.ShopSelectionInfo
{
    public class ShopInfoBubbleTarget : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        public string Name { get; private set; } = "Default Name";
        public string Description { get; private set; } = "Default Description";
        
        public void SetData(string newName = "Default Name", string newDescription = "Default Description")
        {
            Name = newName;
            Description = newDescription;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Affiche la bulle avec le nom et la description
            ShopInfoBubble.Instance.Show(Name, Description, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Cache la bulle
            ShopInfoBubble.Instance.Hide();
        }
    }
}