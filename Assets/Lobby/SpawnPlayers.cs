using Base_Scripts;
using Game.Game_Loop;
using Unity.Netcode;
using UnityEngine;

namespace Lobby
{
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
            if (GameManager.Instance != null && GameManager.Instance.gameState.Value == GameManager.GameState.InGame)
            {
                NetcodeLogger.Instance.LogRpc("Player " + clientID + " joined IN game, not spawning player.", NetcodeLogger.LogType.Netcode);
                return;
            }
            Debug.Log("<color=#FF00FF>Spawning player for client " + clientID);
            GameObject player = Instantiate(playerObject, transform.position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
        }
    }
}
