using System.Linq;
using UnityEngine;

public class MapSpawnPositions : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    public static MapSpawnPositions instance;
    
    private void Awake()
    {
        instance = this;
    }
    public ushort[] GetTransformIndexes(int spawnCount)
    {
        if (spawnCount > spawnPoints.Length)
        {
            Debug.LogError("Spawn count exceeds available spawn points.");
            return null;
        }
        
        ushort[] indexes = Enumerable.Repeat((ushort)999, spawnCount).ToArray();
        
        for (int i = 0; i < indexes.Length; i++)
        {
            ushort index;
            do index = GetRandomSpawnIndex();
            while (System.Array.Exists(indexes, x => x == index));
            indexes[i] = index;
        }
        
        return indexes;
    }
    
    public ushort GetRandomSpawnIndex() => (ushort)Random.Range(0, spawnPoints.Length);
    public Transform GetSpawnPoint(ushort index)
    {
        return spawnPoints[index];
    }
}