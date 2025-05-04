using UnityEngine;

namespace Game.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
    public class ScriptableItemData : ScriptableObject
    {
        public string itemName;
        public GameObject model;
    }
}
