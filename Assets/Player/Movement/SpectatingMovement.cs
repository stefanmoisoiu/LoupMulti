using Game.Common;
using Game.Data;
using Input;
using Player.Networking;
using UnityEngine;

namespace Player.Movement
{
    public class SpectatingMovement : PNetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform head;
        
        [SerializeField] private float speed;

        protected override void StartAnyOwner()
        {
            DataManager.OnEntryUpdatedOwner += OnEntryUpdatedOwner;
        }
        protected override void DisableAnyOwner()
        {
            DataManager.OnEntryUpdatedOwner -= OnEntryUpdatedOwner;
        }

        protected override void FixedUpdateAnyOwner()
        {
            if (DataManager.Instance == null || DataManager.Instance[NetworkManager.LocalClientId].outerData.playingState !=
                OuterData.PlayingState.SpectatingGame) return;
            Vector3 direction = new Vector3(InputManager.Move.x, (InputManager.Jump ? 1 : 0) + (InputManager.Run ? -1 : 0), InputManager.Move.y).normalized;
            if (!(direction.magnitude > 0.1f)) return;
            
            Vector3 moveDirection = head.forward * direction.z + head.right * direction.x + head.up * direction.y;
            
            rb.linearVelocity = moveDirection * speed;
        }

        private void OnEntryUpdatedOwner(PlayerData previousData, PlayerData newData)
        {
            rb.useGravity = newData.outerData.playingState != OuterData.PlayingState.SpectatingGame;
        }
    }
}