using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Base_Scripts;
using Game.Common;
using Game.Data;
using Game.Data.Extensions;
using Networking.Connection;
using Plugins.Smooth_Sync.Netcode_for_GameObjects.Smooth_Sync_Assets;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Maps
{
    public class MapManager : NetworkBehaviour
    {
        private const string LobbyMap = "MultiLobby";
        [SerializeField] private string[] gameMaps;

        public string CurrentMap { get; private set; }

        public static event Action<string> OnMapLoadedServer;
        public static event Action<string> OnMapLoadedAll;

        [Rpc(SendTo.Everyone)]
        private void OnMapLoadedClientRpc(string map) => OnMapLoadedAll?.Invoke(map);

        public IEnumerator LoadRandomGameMap()
        {
            string map = GetRandomMap(gameMaps);
            yield return LoadMapCoroutine(map);
        }

        public IEnumerator LoadLobbyMap()
        {
            yield return LoadMapCoroutine(LobbyMap);
        }

        private string GetRandomMap(string[] mapPool) => gameMaps[Random.Range(0, mapPool.Length)];
        public IEnumerator LoadMapCoroutine(string map)
        {
            if (CurrentMap == map) yield break;

            CurrentMap = map;
            NetcodeLogger.Instance.LogRpc("Loading map: " + map, NetcodeLogger.LogType.Map);

            Task t = NetcodeSceneChanger.Instance.NetworkChangeScene(map);
            yield return new UnityEngine.WaitUntil(() => t.IsCompleted);

            NetcodeLogger.Instance.LogRpc("Map loaded: " + map, NetcodeLogger.LogType.Map);
            OnMapLoadedServer?.Invoke(CurrentMap);
            OnMapLoadedClientRpc(CurrentMap);
        }

        public void SetPlayerSpawnPositions()
        {
            PlayerData[] players = DataManager.Instance.Search(new[] { OuterData.PlayingState.Playing });
            Transform[] spawnPoints = MapSpawnPositions.instance.GetSpawnPoints(players.Length);
            for (int i = 0; i < players.Length; i++)
            {
                if (!NetworkManager.ConnectedClients.TryGetValue(players[i].ClientId, out NetworkClient client))
                    throw new Exception("Client not found: " + players[i].ClientId);
                Debug.Log("Setting player spawn position for: " + client.ClientId);
                Transform spawnPoint = spawnPoints[i];
                NetworkObject player = client.PlayerObject;
                SmoothSyncNetcode sync = player.GetComponent<SmoothSyncNetcode>();

                sync.rb.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
                sync.rb.linearVelocity = Vector3.zero;
                sync.teleportAnyObjectFromServer(spawnPoint.position, spawnPoint.rotation, Vector3.one);
            }
        }
    }
}