using System.Collections;
using Input;
using Player.Hotbar.Item;
using Player.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Player.Hotbar
{
    public class Hotbar : PNetworkBehaviour
    {
        [SerializeField] private Item.Item[] itemSlots;
        [SerializeField] private ItemSelectTransition transition;
        
        public NetworkVariable<ushort> selectedSlot = new (ushort.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        protected override void StartOnlineOwner()
        {
            InputManager.OnSlotUse += SelectSlot;
        }

        protected override void DisableAnyOwner()
        {
            InputManager.OnSlotUse -= SelectSlot;   
        }
        private void SelectSlot(int id)
        {
            if (id >= itemSlots.Length)
            {
                Debug.LogError($"Item ID {id} is out of range.");
                return;
            }
            if (id == selectedSlot.Value)
            {
                Debug.LogError($"Already selected item.");
                return;
            }

            if (selectedSlot.Value != ushort.MaxValue)
                itemSlots[selectedSlot.Value]?.SetSelected(false);

            StartCoroutine(SelectCoroutine((ushort)id));
        }

        private IEnumerator SelectCoroutine(ushort newId)
        {
            yield return StartCoroutine(transition.Select(
                selectedSlot.Value == ushort.MaxValue ? null : itemSlots[selectedSlot.Value],
                newId == ushort.MaxValue ? null : itemSlots[newId]));
            selectedSlot.Value = newId;
            itemSlots[newId]?.SetSelected(true);
        }
    }
}