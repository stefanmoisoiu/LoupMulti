using System;
using Game.Common;
using Game.Common.List;
using Unity.Netcode;
using UnityEngine;

namespace Game.Data.Extensions
{
    public class PlayerResourcesHelper : NetworkBehaviour
    {
        [SerializeField] private ResourceList resourceList;
        
        public static event Action<ushort, ushort, ResourceData> OnResourceCollectedOwner;
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void OnResourceCollectedClientRpc(ushort amountCollected, ushort total, ushort resourceIndex, RpcParams @params) => OnResourceCollectedOwner?.Invoke(amountCollected, total, resourceList.GetResource(resourceIndex));
        
        /// <summary>
        /// SERVER ONLY
        /// </summary>
        public void CollectResource(ulong origin, ResourceData resourceData, ushort amount)
        {
            if (!IsServer) throw new Exception("CollectResource can only be called on the server");
            if (amount == 0) return;
            
            PlayerData player = DataManager.Instance[origin];
            if (player.clientId == ulong.MaxValue) throw new System.Exception($"Player with ClientId {origin} not found");

            InGameData inGameData = player.inGameData;
            inGameData = new(player.inGameData)
            {
                resources = inGameData.resources.AddResource(resourceData.ResourceType, amount)
            };
            
            
            DataManager.Instance[origin] = new(player)
            {
                inGameData = inGameData
            };
            
            OnResourceCollectedClientRpc(
                amount, 
                inGameData.resources.GetResourceAmount(resourceData.ResourceType), 
                resourceList.GetResource(resourceData), 
                player.SendRpcTo());
        }
    }
}