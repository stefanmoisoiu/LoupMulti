using Game.Game_Loop;
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
            GameManager.OnGameStateChanged += GameStateChanged;
        }

        protected override void DisableAnyOwner()
        {
            InputManager.OnSlotUse -= SelectSlot;   
            InputManager.OnDrillUse -= SelectDrill;
            GameManager.OnGameStateChanged -= GameStateChanged;
        }

        private void GameStateChanged(GameManager.GameState newState, float serverTime)
        {
            if (newState == GameManager.GameState.Lobby) SelectSlot(-1);
        }
        
        private const ushort DrillSlotId = ushort.MaxValue - 1;
        private void SelectDrill()
        {
            SelectSlot(DrillSlotId);
        }
        private void SelectSlot(int slotId)
        {
            if (GameManager.Instance.gameState.Value == GameManager.GameState.Lobby) return;
            
            if (slotId == selectedSlot.Value || slotId == -1) slotId = ushort.MaxValue;
            else if (slotId >= toolSlots.Length && slotId != DrillSlotId)
            {
                Debug.LogError($"Tool ID {slotId} is out of range.");
                return;
            }
            
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