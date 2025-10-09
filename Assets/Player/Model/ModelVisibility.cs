using System;
using Game.Common;
using Game.Data;
using Unity.Netcode;
using UnityEngine;

namespace Player.Model
{
    public class ModelVisibility : NetworkBehaviour
    {
        [SerializeField] private GameObject modelParent;
        
        private void Start()
        {
            DataManager.OnEntryUpdatedClient += OnEntryUpdated;
        }
        private void OnDisable()
        {
            DataManager.OnEntryUpdatedClient -= OnEntryUpdated;
        }

        private void OnEntryUpdated(PlayerData previousData, PlayerData newData)
        {
            if (newData.clientId != OwnerClientId) return;
            UpdateVisibility(newData.outerData.playingState != OuterData.PlayingState.SpectatingGame);
        }

        private void UpdateVisibility(bool visible)
        {
            modelParent.SetActive(visible);
        }
    }
}