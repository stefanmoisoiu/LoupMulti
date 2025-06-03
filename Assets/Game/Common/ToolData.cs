using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "Tool", menuName = "Game/Tool")]
    public class ToolData : ScriptableObject
    {
        public string toolName;
        public GameObject model;
    }
}
