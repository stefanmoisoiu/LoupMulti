using UnityEngine;

namespace Player.Hotbar.Item
{
    public class ItemList : MonoBehaviour
    {
        [SerializeField] private Item[] items;
        public Item[] Items => items;
        public ushort GetItemID(Item item)
        {
            for (ushort i = 0; i < items.Length; i++)
            {
                if (items[i] == item) return i;
            }

            Debug.LogError($"Item not found in the list.");
            return ushort.MaxValue;
        }
    }
}