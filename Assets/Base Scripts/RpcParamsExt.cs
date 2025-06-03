using Unity.Netcode;
using UnityEngine;

namespace Base_Scripts
{
    public class RpcParamsExt : MonoBehaviour
    {
        public static RpcParamsExt Instance { get; private set; }

        private void Start()
        {
            Instance = this;
        }

        public RpcParams SendToClientIDs(ulong[] clientIDs) => new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = NetworkManager.Singleton.RpcTarget.Group(clientIDs, RpcTargetUse.Temp)
            }
        };
        
        public RpcParams SendToAllExcept(ulong[] clientID) => new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = NetworkManager.Singleton.RpcTarget.Not(clientID, RpcTargetUse.Temp)
            }
        };
        
        public RpcParams SenderClientID(ulong clientID) => new RpcParams
        {
            Receive = new()
            {
                SenderClientId = clientID
            }
        };
        public RpcParams SendToAllClients() => new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = NetworkManager.Singleton.RpcTarget.Everyone
            }
        };
    }
}