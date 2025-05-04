using Game.Items;
using Player.Networking;
using UnityEngine;

namespace Player.Hotbar.Item
{
        public abstract class Item : PNetworkBehaviour
        {
                [SerializeField] private ScriptableItemData itemData;
                public ScriptableItemData ItemData => itemData;
                
                public bool Selected { get; private set; } = false;
                public void SetSelected(bool selected)
                {
                        Selected = selected;
                        if (selected)
                                Select();
                        else
                                Deselect();
                }
        
                protected abstract void Select();
                protected abstract void Deselect();
        
                public abstract void StartUse();
                public abstract void UpdateUse();
                public abstract void CancelUse();
        }
}