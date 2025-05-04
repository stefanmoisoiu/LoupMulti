using System;
using Game.Game_Loop.Round.Collect.Resource;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Game.Data.Extensions
{
    public class PlayerResources : NetworkBehaviour
    {
        [SerializeField] private Game_Loop.Round.Collect.Resource.ResourceData[] resources;

#if UNITY_EDITOR
        [Button]
        private void UpdateResources()
        {
            string[] paths = AssetDatabase.FindAssets("t:ResourceInfo");
            resources = new Game_Loop.Round.Collect.Resource.ResourceData[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[i]);
                resources[i] = AssetDatabase.LoadAssetAtPath<Game_Loop.Round.Collect.Resource.ResourceData>(path);
            }
        }
#endif
        
        
        
        public Action<ushort, Game_Loop.Round.Collect.Resource.ResourceData> OnResourceCollectedOwner;
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void OnResourceCollectedClientRpc(ushort amount, ushort resourceIndex, RpcParams rpcParams) => OnResourceCollectedOwner?.Invoke(amount, resources[resourceIndex]);
        
        /// <summary>
        /// SERVER ONLY
        /// </summary>
        public void CollectResource(ulong origin, Game_Loop.Round.Collect.Resource.ResourceData resourceData, ushort amount)
        {
            PlayerData player = DataManager.Instance[origin];
            if (player.ClientId == ulong.MaxValue) throw new System.Exception($"Player with ClientId {origin} not found");
            
            DataManager.Instance[origin] = new PlayerData(player)
            {
                inGameData = new(player.inGameData)
                {
                    resources = player.inGameData.resources.AddResource(resourceData.ResourceType, amount)
                }
            };

            OnResourceCollectedClientRpc(amount, (ushort)Array.IndexOf(resources, resourceData), player.SendRpcTo());
        }
    }
}