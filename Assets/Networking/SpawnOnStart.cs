using System;
using Unity.Netcode;
using UnityEngine;

public class SpawnOnStart : NetworkBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private SpawnOwner spawnOwner;
    public enum SpawnOwner
    {
        Server,
        Client
    }
    private void Start()
    {
        if (IsServer)
        {
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
        }
    }
}
