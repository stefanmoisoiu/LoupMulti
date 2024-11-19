using Unity.Netcode;
using UnityEditor;

public abstract class PNetworkBehaviour : NetworkBehaviour
{
    private bool _initialized = false;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner || _initialized) return;
        StartOnlineOwner();
        StartAnyOwner();
        _initialized = true;
    }
    private void Start()
    {
        if (IsSpawned || NetcodeManager.InGame || _initialized) return;
        StartOffline();
        StartAnyOwner();
        _initialized = true;
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