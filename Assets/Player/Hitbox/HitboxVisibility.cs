using System;
using Game.Common;
using Game.Data;
using Unity.Netcode;
using UnityEngine;

namespace Player.Hitbox
{
    public class HitboxVisibility : NetworkBehaviour
    {
        [SerializeField] private HitboxTarget hitboxTarget;
        
        private void Start()
        {
            DataManager.OnEntryUpdatedClient += EntryUpdatedClient;
        }

        private void OnDisable()
        {
            DataManager.OnEntryUpdatedClient -= EntryUpdatedClient;
        }

        private void EntryUpdatedClient(PlayerData previousValue, PlayerData newValue)
        {
            hitboxTarget.enabled = newValue.outerData.playingState != OuterData.PlayingState.SpectatingGame;
        }
    }
}