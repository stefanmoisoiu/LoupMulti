using System;
using System.Collections.Generic;
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
    
    public Action<string> OnMapLoadedServer;
    public Action<string> OnMapLoadedAll;
    [Rpc(SendTo.Everyone)] private void OnMapLoadedClientRpc(string map) => OnMapLoadedAll?.Invoke(map);
    
    public void LoadRandomGameMap()
    {
        string map = GetRandomMap(gameMaps);
        LoadMap(map);
    }
    private string GetRandomMap(string[] mapPool) => gameMaps[Random.Range(0, mapPool.Length)];
    public void LoadMap(string map)
    {
        if (CurrentMap == map) return;
        
        CurrentMap = map;
        
        GameManager.Instance.LogRpc("Loading map: " + map, GameManager.LogType.MapInfo);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += MapLoaded;
        
        NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
    }

    private void MapLoaded(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= MapLoaded;
        
        GameManager.Instance.LogRpc("Map loaded", GameManager.LogType.MapInfo);
        OnMapLoadedServer?.Invoke(CurrentMap);
        OnMapLoadedClientRpc(CurrentMap);
    }
    
    public void SetPlayerSpawnPositions()
    {
        GameData gameData = GameManager.Instance.gameData;
        ushort[] spawnIndexes = MapSpawnPositions.instance.GetTransformIndexes(gameData.ServerSidePlayerDataList.playerDatas.Count);
        for(int i = 0; i < gameData.ServerSidePlayerDataList.playerDatas.Count; i++)
        {
            PlayerData data = gameData.ServerSidePlayerDataList.playerDatas[i];
            ushort spawnIndex = spawnIndexes[i];
            
            Transform spawnPoint = MapSpawnPositions.instance.GetSpawnPoint(spawnIndex);
            SmoothSyncNetcode sync = NetworkManager.Singleton.ConnectedClients[data.ClientId].PlayerObject.GetComponent<SmoothSyncNetcode>();
            sync.teleportAnyObjectFromServer(spawnPoint.position, spawnPoint.rotation, Vector3.one);
        }
    }
}