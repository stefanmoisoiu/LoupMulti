using Game.Common;
using Game.Common.List;
using Unity.Netcode;
using UnityEngine;

namespace Game.Data.Extensions
{
    public class PlayerResources : NetworkBehaviour
    {
        [SerializeField] private ResourceList resourceList;
        
        // public Action<ushort, Game_Loop.Round.Collect.Resource.ResourceData> OnResourceCollectedOwner;
        
        // [Rpc(SendTo.SpecifiedInParams)]
        // private void OnResourceCollectedClientRpc(ushort amount, ushort resourceIndex, RpcParams rpcParams) => OnResourceCollectedOwner?.Invoke(amount, resourceList.GetResource(resourceIndex));
        
        /// <summary>
        /// SERVER ONLY
        /// </summary>
        public void CollectResource(ulong origin, ResourceData resourceData, ushort amount)
        {
            PlayerData player = DataManager.Instance[origin];
            if (player.ClientId == ulong.MaxValue) throw new System.Exception($"Player with ClientId {origin} not found");

            DataManager.Instance[origin] = new(player)
            {
                inGameData = new(player.inGameData)
                {
                    resources = player.inGameData.resources.AddResource(resourceData.ResourceType, amount)
                }
            };
        }
    }
}