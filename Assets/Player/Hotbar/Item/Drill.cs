using Input;
using UnityEngine;

namespace Player.Hotbar.Item
{
    public class Drill : Item
    {
        protected override void UpdateOnlineOwner()
        {
            if (Selected && InputManager.Primary) UpdateUse();
        }

        protected override void Select()
        {
            Debug.Log($"Selected Drill: {ItemData.itemName}");
            
            InputManager.OnPrimaryStarted += StartUse;
            InputManager.OnPrimaryCanceled += CancelUse;
        }

        protected override void Deselect()
        {
            Debug.Log($"Deselected Drill: {ItemData.itemName}");
            
            InputManager.OnPrimaryStarted -= StartUse;
            InputManager.OnPrimaryCanceled -= CancelUse;
        }

        public override void StartUse()
        {
            Debug.Log($"Started using Drill: {ItemData.itemName}");
        }

        public override void UpdateUse()
        {
            Debug.Log($"Updating use of Drill: {ItemData.itemName}");
            // Implement drill logic here
        }

        public override void CancelUse()
        {
            Debug.Log($"Cancelled use of Drill: {ItemData.itemName}");
            // Implement logic to stop drilling here
        }
    }
}