using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerObject;

    private void OnEnable()
    {
        // NetcodeManager.OnCreateGame += TrySpawnSelfPlayer;
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost) NetworkManager.Singleton.OnClientConnectedCallback += TrySpawn;
    }

    private void OnDisable()
    {
        // NetcodeManager.OnCreateGame -= TrySpawnSelfPlayer;
        if (NetworkManager.Singleton != null && IsHost) NetworkManager.Singleton.OnClientConnectedCallback -= TrySpawn;
    }
    private void TrySpawnSelfPlayer()
    {
        TrySpawn(OwnerClientId);
    }
    private void TrySpawn(ulong clientID)
    {
        Debug.Log("Spawning player for client " + clientID);
        GameObject player = Instantiate(playerObject, transform.position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }
}
