using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Base_Scripts;
using Game.Data;
using Game.Data.Extensions;
using Networking;
using Plugins.Smooth_Sync.Netcode_for_GameObjects.Smooth_Sync_Assets;
using Scenes.Scene_Load;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Manager
{
    public class MapManager : NetworkBehaviour
    {
        private const string LobbyMap = "MultiLobby";
        [SerializeField] private string[] gameMaps;
    
        public string CurrentMap { get; private set; }
    
        public static event Action<string> OnMapLoadedServer;
        public static event Action<string> OnMapLoadedAll;
        [Rpc(SendTo.Everyone)] private void OnMapLoadedClientRpc(string map) => OnMapLoadedAll?.Invoke(map);
    
        public void LoadRandomGameMap()
        {
            string map = GetRandomMap(gameMaps);
            LoadMap(map);
        }
        private string GetRandomMap(string[] mapPool) => gameMaps[Random.Range(0, mapPool.Length)];
        public void LoadMap(string map) => StartCoroutine(LoadMapCoroutine(map));

        private IEnumerator LoadMapCoroutine(string map)
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
            NetworkClient[] clients = NetworkManager.ConnectedClients.Values.ToArray();
            Transform[] spawnPoints = MapSpawnPositions.instance.GetSpawnPoints(PlayerDataManager.Instance.PlayingPlayersCount());
            for(int i = 0; i < clients.Length; i++)
            {
                Transform spawnPoint = spawnPoints[i];
                SmoothSyncNetcode sync = clients[i].PlayerObject.GetComponent<SmoothSyncNetcode>();
                sync.teleportAnyObjectFromServer(spawnPoint.position, spawnPoint.rotation, Vector3.one);
            }
        }
    }
}