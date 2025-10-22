using Game.Common;
using Player.General_UI;
using Player.Networking;
using UnityEngine;

namespace Player.Abilities.UI
{
    public class AbilityManagerUI : PNetworkBehaviour
    {
        private AbilitySlotUI _drillSlot;
        private AbilitySlotUI[] _abilitySlots;
        [SerializeField] private string drillSlotTag;
        [SerializeField] private string abilitySlotsTag;

        protected override void StartAnyOwner()
        {
            Setup();
        }

        private void Setup()
        {
            _drillSlot = PCanvas.CanvasObjects[drillSlotTag].GetComponent<AbilitySlotUI>();
            GameObject slotsLayout = PCanvas.CanvasObjects[abilitySlotsTag];
            
            if (slotsLayout.transform.childCount != GameSettings.Instance.AbilitySlots)
            {
                throw new System.Exception("Ability slots layout does not have the correct number of children.");
            }

            _abilitySlots = new AbilitySlotUI[GameSettings.Instance.AbilitySlots];
            for (int i = 0; i < _abilitySlots.Length; i++)
                _abilitySlots[i] = slotsLayout.transform.GetChild(i).GetComponent<AbilitySlotUI>();
        }

        public AbilitySlotUI GetSlotUI(int index)
        {
            if (_drillSlot == null || _abilitySlots == null) Setup();
            return index == -1 ? _drillSlot : _abilitySlots[index];
        }
    }
}