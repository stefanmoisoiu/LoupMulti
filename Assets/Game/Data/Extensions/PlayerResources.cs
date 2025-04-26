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
        [SerializeField] private ResourceInfo[] resources;

#if UNITY_EDITOR
        [Button]
        private void UpdateResources()
        {
            string[] paths = AssetDatabase.FindAssets("t:ResourceInfo");
            resources = new ResourceInfo[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[i]);
                resources[i] = AssetDatabase.LoadAssetAtPath<ResourceInfo>(path);
            }
        }
#endif
        
        
        
        public Action<ushort, ResourceInfo> OnResourceCollectedOwner;
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void OnResourceCollectedClientRpc(ushort amount, ushort resourceIndex, RpcParams rpcParams) => OnResourceCollectedOwner?.Invoke(amount, resources[resourceIndex]);
        
        /// <summary>
        /// SERVER ONLY
        /// </summary>
        public void CollectResource(ulong origin, ResourceInfo resourceInfo, ushort amount)
        {
            PlayerData player = DataManager.Instance[origin];
            if (player.ClientId == ulong.MaxValue) throw new System.Exception($"Player with ClientId {origin} not found");
            
            DataManager.Instance[origin] = new PlayerData(player)
            {
                inGameData = new(player.inGameData)
                {
                    resources = player.inGameData.resources.AddResource(resourceInfo.ResourceType, amount)
                }
            };

            OnResourceCollectedClientRpc(amount, (ushort)Array.IndexOf(resources, resourceInfo), player.SendRpcTo());
        }
    }
}