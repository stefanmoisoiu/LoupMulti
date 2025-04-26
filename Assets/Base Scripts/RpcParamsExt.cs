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

        public RpcParams SendToClientIDs(ulong[] clientIDs, NetworkManager manager) => new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = manager.RpcTarget.Group(clientIDs, RpcTargetUse.Temp)
            }
        };
        public RpcParams SenderClientID(ulong clientID) => new RpcParams
        {
            Receive = new()
            {
                SenderClientId = clientID
            }
        };
        public RpcParams SendToAllClients(NetworkManager manager) => new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = manager.RpcTarget.Everyone
            }
        };
    }
}