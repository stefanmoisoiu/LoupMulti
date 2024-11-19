using Unity.Netcode;
using UnityEditor;

public abstract class PNetworkBehaviour : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        StartOnlineOwner();
        StartAnyOwner();
    }
    private void Start()
    {
        if (IsSpawned) return;
        StartOffline();
        StartAnyOwner();
    }

    protected virtual void StartOnlineOwner() {}
    protected virtual void StartOffline() {}
    protected virtual void StartAnyOwner() {}
    
    protected virtual void DisableOnlineOwner() {}
    protected virtual void DisableOffline() {}
    protected virtual void DisableAnyOwner() {}
    

    
    
    private void OnDisable()
    {
        if (IsSpawned)
        {
            if (!IsOwner) return;
            DisableOnlineOwner();
        }
        else
        {
            DisableOffline();
        }
        
        DisableAnyOwner();
    }
}