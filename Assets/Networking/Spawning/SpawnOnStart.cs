using System;
using Unity.Netcode;
using UnityEngine;

namespace Networking.Spawning
{
    public class SpawnOnStart : NetworkBehaviour
    {
        [SerializeField] private GameObject prefabToSpawn;
        [SerializeField] private SpawnOwner spawnOwner;

        [SerializeField] private bool spawnOnlyOnePerGame = false;
        private static bool hasSpawnedOne = false;
        
        public enum SpawnOwner
        {
            Server,
            Client
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            if (spawnOnlyOnePerGame && hasSpawnedOne) return;
            prefabToSpawn = Instantiate(prefabToSpawn,transform.position,transform.rotation);
            NetworkObject networkObject = prefabToSpawn.GetComponent<NetworkObject>();
            if (spawnOwner == SpawnOwner.Server)
            {
                networkObject.Spawn();
            }
            else
            {
                networkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
            }
            
            hasSpawnedOne = true;
            NetworkManager.OnClientStopped += OnClientStopped;
        }

        private void OnDisable()
        {
            if (NetworkManager != null) NetworkManager.OnClientStopped -= OnClientStopped;
        }

        private void OnClientStopped(bool hostMode)
        {
            hasSpawnedOne = false;
        }
    }
}
