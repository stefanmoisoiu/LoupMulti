using Game.Maps;
using Plugins.Smooth_Sync.Netcode_for_GameObjects.Smooth_Sync_Assets;
using Unity.Netcode;
using UnityEngine;

namespace Player.Networking
{
    public class OutOfBoundsReset : PNetworkBehaviour
    {
        [SerializeField] private SmoothSyncNetcode sync;
    
        [SerializeField] private float yThreshold = -50f;
        private bool teleportCooldown = true;
        private void ResetTeleportCooldown()
        {
            teleportCooldown = false;
        }
        protected override void UpdateOnlineOwner()
        {
            if (!teleportCooldown) return;
            if (sync.rb.position.y < yThreshold || sync.rb.position.y > -yThreshold)
            {
                Debug.LogError("OUT OF BOUNDS! Teleporting player to spawn point.");
                teleportCooldown = false;
                TeleportServerRpc();
                Invoke(nameof(ResetTeleportCooldown), 1f); // Cooldown of 1 second before next teleport
            }
        }

        [ServerRpc]
        private void TeleportServerRpc()
        {
            Transform spawnPos = MapSpawnPositions.instance.GetSpawnPoint(0);
            sync.rb.position = spawnPos.position;
            sync.transform.rotation = spawnPos.rotation;
            sync.rb.linearVelocity = Vector3.zero;
            sync.teleportOwnedObjectFromOwner();
        }
    }
}
