using Game.Common;
using Player.Networking;
using UnityEngine;

namespace Player.Tool
{
        public abstract class Tool : PNetworkBehaviour
        {
                [SerializeField] private ToolData toolData;
                public ToolData ToolData => toolData;
                
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
        }
}