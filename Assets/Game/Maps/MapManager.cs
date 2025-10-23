using System;
using System.Collections;
using Base_Scripts;
using Game.Common;
using Game.Data;
using Game.Data.Extensions;
using Networking;
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
            yield return StartCoroutine(LoadMapCoroutine(map));
        }

        public IEnumerator LoadLobbyMap()
        {
            yield return StartCoroutine(LoadMapCoroutine(LobbyMap));
        }

        private string GetRandomMap(string[] mapPool) => gameMaps[Random.Range(0, mapPool.Length)];
        public IEnumerator LoadMapCoroutine(string map)
        {
            if (CurrentMap == map) yield break;

            CurrentMap = map;
            NetcodeLogger.Instance.LogRpc("Loading map: " + map, NetcodeLogger.LogType.Map);

            yield return StartCoroutine(
                NetcodeSceneChanger.Instance.NetcodeLoadScene(map, SceneType.Active));

            NetcodeLogger.Instance.LogRpc("Map loaded: " + map, NetcodeLogger.LogType.Map);
            OnMapLoadedServer?.Invoke(CurrentMap);
            OnMapLoadedClientRpc(CurrentMap);
        }

        public void SetPlayerSpawnPositions()
        {
            PlayerData[] players = DataManager.Instance.Search(new[] { OuterData.PlayingState.Playing });
            for (ushort i = 0; i < players.Length; i++) TeleportPlayerClientRpc(i,players[i].SendRpcTo());
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void TeleportPlayerClientRpc(ushort spawnPointInd, RpcParams @params)
        {
            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(@params.Receive.SenderClientId, out NetworkClient client))
                throw new Exception("My Client not found " + @params.Receive.SenderClientId);
            Debug.Log($"Teleporting player to spawn position [{spawnPointInd}]");
            Transform spawnPoint = MapSpawnPositions.instance.GetSpawnPoint(spawnPointInd);
            NetworkObject player = client.PlayerObject;
            SmoothSyncNetcode sync = player.GetComponent<SmoothSyncNetcode>();
            
            sync.transform.position = spawnPoint.position;
            sync.transform.rotation = spawnPoint.rotation;
            sync.rb.linearVelocity = Vector3.zero;
            sync.teleportOwnedObjectFromOwner();
        }
    }
}