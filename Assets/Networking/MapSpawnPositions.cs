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
        ushort[] indexes = Enumerable.Repeat((ushort)999, spawnCount).ToArray();
        
        for (int i = 0; i < indexes.Length; i++)
        {
            ushort index;
            do index = (ushort)Random.Range(0, spawnPoints.Length);
            while (System.Array.Exists(indexes, x => x == index));
            indexes[i] = index;
        }
        
        return indexes;
    }
    public Transform GetSpawnPoint(ushort index)
    {
        return spawnPoints[index];
    }
}