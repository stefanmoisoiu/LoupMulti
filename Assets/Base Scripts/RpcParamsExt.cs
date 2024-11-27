using System;
using Unity.Netcode;

public class RpcParamsExt : NetworkBehaviour
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
            Target = RpcTarget.Group(clientIDs, RpcTargetUse.Temp)
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
            Target = RpcTarget.Everyone
        }
    };
}