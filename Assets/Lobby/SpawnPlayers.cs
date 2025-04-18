using Unity.Netcode;
using UnityEngine;

public class SpawnPlayers : NetworkBehaviour
{
    [SerializeField] private GameObject playerObject;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkObject.DestroyWithScene = false;
        if (IsHost) TrySpawnSelfPlayer();
        if (IsServer) NetworkManager.Singleton.OnClientConnectedCallback += TrySpawn;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null && IsServer) NetworkManager.Singleton.OnClientConnectedCallback -= TrySpawn;
    }
    private void TrySpawnSelfPlayer()
    {
        TrySpawn(OwnerClientId);
    }
    private void TrySpawn(ulong clientID)
    {
        if (NetworkManager.ConnectedClients[clientID].PlayerObject != null)
        {
            Debug.LogError("Player object already exists for client " + clientID);
            return;
        }
        Debug.Log("<color=#FF00FF>Spawning player for client " + clientID);
        
        GameObject player;
        if (GameManager.Instance != null && GameManager.Instance.gameState.Value != GameManager.GameState.Lobby)
        {
            Transform spawnPoint = MapSpawnPositions.instance.GetSpawnPoint(MapSpawnPositions.instance.GetRandomSpawnIndex());
            player = Instantiate(playerObject, spawnPoint.position, spawnPoint.rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
        }
        else
        {
            player = Instantiate(playerObject, transform.position, Quaternion.identity);
        }
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }
}
