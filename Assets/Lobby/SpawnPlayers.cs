using System;
using Base_Scripts;
using Game.Game_Loop;
using Networking.Connection;
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
            if (IsServer)
            {
                if (NetcodeManager.InGame) SpawnAllExistingPlayers();
                else NetcodeManager.OnEnterGame += SpawnAllExistingPlayers;
                NetworkManager.Singleton.OnClientConnectedCallback += TrySpawn;
            }
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton != null && IsServer) NetworkManager.Singleton.OnClientConnectedCallback -= TrySpawn;
        }
        private void TrySpawn(ulong clientID)
        {
            if (NetcodeManager.LoadingGame) return;
            if (NetworkManager.ConnectedClients[clientID].PlayerObject != null)
            {
                Debug.LogError("Player object already exists for client " + clientID);
                return;
            }
            if (GameManager.CurrentGameState == GameManager.GameState.InGame)
            {
                NetcodeLogger.Instance.LogRpc("Player " + clientID + " joined IN game, not spawning player.", NetcodeLogger.LogType.Netcode);
                return;
            }
            Debug.Log("<color=#FF00FF>Spawning player for client " + clientID);
            GameObject player = Instantiate(playerObject, transform.position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
            
            Debug.Log($"Player scene : {player.scene.name}, active scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }
        
        private void SpawnAllExistingPlayers()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                TrySpawn(client.ClientId);
            }
            NetcodeManager.OnEnterGame -= SpawnAllExistingPlayers;
        }
    }
}
