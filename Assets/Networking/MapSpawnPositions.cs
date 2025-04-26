using System.Linq;
using UnityEngine;

namespace Networking
{
    public class MapSpawnPositions : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        public static MapSpawnPositions instance;
    
        private void Awake()
        {
            instance = this;
        }
        public Transform[] GetSpawnPoints(int spawnCount)
        {
            if (spawnCount > spawnPoints.Length)
            {
                Debug.LogError("Spawn count exceeds available spawn points.");
                return null;
            }
        
            int[] indexes = Enumerable.Repeat(999, spawnCount).ToArray();
        
            for (int i = 0; i < indexes.Length; i++)
            {
                int index;
                do index = Random.Range(0, spawnPoints.Length);
                while (System.Array.Exists(indexes, x => x == index));
                indexes[i] = index;
            }
        
            Transform[] res = new Transform[spawnCount];
            for (int i = 0; i < indexes.Length; i++)
                res[i] = spawnPoints[indexes[i]];
        
            return res;
        }
        public Transform GetSpawnPoint(ushort index) => spawnPoints[index];
    }
}