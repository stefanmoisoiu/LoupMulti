using System;
using Unity.Netcode;
using UnityEngine;

public class SpawnOnStart : NetworkBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    private void Start()
    {
        if (IsServer)
        {
            prefabToSpawn = Instantiate(prefabToSpawn,transform.position,transform.rotation);
            NetworkObject networkObject = prefabToSpawn.GetComponent<NetworkObject>();
            networkObject.Spawn();
        }
    }
}
