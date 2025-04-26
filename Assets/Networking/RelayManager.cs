using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Networking
{
    public static class RelayManager
    {
        public static async Task<Allocation> CreateRelay(int maxConnectionsExcludingOwner = 3)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnectionsExcludingOwner);
                return allocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
                throw e;
            }
        }

        public static async Task<JoinAllocation> JoinRelayByCode(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                return joinAllocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
                throw e;
            }
        }
    }
}