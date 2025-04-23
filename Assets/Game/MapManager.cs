using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Smooth;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
        
        NetcodeSceneChanger.Instance.NetworkChangeScene(map);
        
        NetcodeLogger.Instance.LogRpc("Map loaded: " + map, NetcodeLogger.LogType.Map);
        OnMapLoadedServer?.Invoke(CurrentMap);
        OnMapLoadedClientRpc(CurrentMap);
    }
    
    public void SetPlayerSpawnPositions()
    {
        NetworkClient[] clients = NetworkManager.ConnectedClients.Values.ToArray();
        ushort[] spawnIndexes = MapSpawnPositions.instance.GetTransformIndexes(NetworkManager.ConnectedClients.Count);
        for(int i = 0; i < clients.Length; i++)
        {
            ushort spawnIndex = spawnIndexes[i];
            
            Transform spawnPoint = MapSpawnPositions.instance.GetSpawnPoint(spawnIndex);
            SmoothSyncNetcode sync = clients[i].PlayerObject.GetComponent<SmoothSyncNetcode>();
            sync.teleportAnyObjectFromServer(spawnPoint.position, spawnPoint.rotation, Vector3.one);
        }
    }
}