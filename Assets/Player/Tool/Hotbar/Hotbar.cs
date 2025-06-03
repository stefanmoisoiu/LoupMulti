using Input;
using Player.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Player.Tool.Hotbar
{
    public class Hotbar : PNetworkBehaviour
    {
        [SerializeField] private Tool drillTool;
        [SerializeField] private Tool[] toolSlots;
        [SerializeField] private ToolModels toolModels;
        [SerializeField] private ToolList toolList;
        
        
        
        public NetworkVariable<ushort> selectedSlot = new (ushort.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        protected override void StartOnlineNotOwner()
        {
            ChangeSlotAnim(ushort.MaxValue, selectedSlot.Value);
            selectedSlot.OnValueChanged += ChangeSlotAnim;
        }

        protected override void DisableOnlineNotOwner()
        {
            selectedSlot.OnValueChanged -= ChangeSlotAnim;
        }

        protected override void StartOnlineOwner()
        {
            InputManager.OnSlotUse += SelectSlot;
            InputManager.OnDrillUse += SelectDrill;
        }

        protected override void DisableAnyOwner()
        {
            InputManager.OnSlotUse -= SelectSlot;   
            InputManager.OnDrillUse -= SelectDrill;
        }
        private const ushort DrillSlotId = ushort.MaxValue - 1;
        private void SelectDrill()
        {
            SelectSlot(DrillSlotId);
        }
        private void SelectSlot(int slotId)
        {
            if (slotId >= toolSlots.Length && slotId != DrillSlotId)
            {
                Debug.LogError($"Tool ID {slotId} is out of range.");
                return;
            }
            if (slotId == selectedSlot.Value) slotId = ushort.MaxValue;
            
            ChangeSlotAnim(selectedSlot.Value, (ushort)slotId);

            if (selectedSlot.Value != ushort.MaxValue)
                (selectedSlot.Value == DrillSlotId ? drillTool : toolSlots[selectedSlot.Value]).SetSelected(false);
            
            if (slotId == DrillSlotId)
                drillTool.SetSelected(true);
            else if (slotId != ushort.MaxValue)
                toolSlots[slotId]?.SetSelected(true);
            
            selectedSlot.Value = (ushort)slotId;
        }

        private void ChangeSlotAnim(ushort previousSlotID, ushort newSlotID)
        {
            ushort previousToolId = previousSlotID == ushort.MaxValue ? ushort.MaxValue : toolList.GetToolID(previousSlotID == DrillSlotId ? drillTool : toolSlots[previousSlotID]);
            ushort newToolId = newSlotID == ushort.MaxValue ? ushort.MaxValue : toolList.GetToolID(newSlotID == DrillSlotId ? drillTool : toolSlots[newSlotID]);
            
            toolModels.ChangeToolModel(previousToolId, newToolId);
        }
    }
}