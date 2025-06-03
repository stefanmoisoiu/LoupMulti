using UnityEngine;

namespace Player.Tool
{
    public class ToolList : MonoBehaviour
    {
        [SerializeField] private Tool[] tools;
        public Tool[] Tools => tools;
        public ushort GetToolID(Tool tool)
        {
            if (tool == null) return ushort.MaxValue;
            for (ushort i = 0; i < tools.Length; i++)
            {
                if (tools[i] == tool) return i;
            }

            Debug.LogError($"Item not found in the list.");
            return ushort.MaxValue;
        }
    }
}