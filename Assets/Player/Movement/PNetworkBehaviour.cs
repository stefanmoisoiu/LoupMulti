using System;
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

    private void Update()
    {
        if (NetcodeManager.InGame)
        {
            if (IsOwner)
            {
                UpdateOnlineOwner();
                UpdateAnyOwner();
            }
            else UpdateOnlineNotOwner();
        }
        else
        {
            UpdateOffline();
            UpdateAnyOwner();
        }
    }

    private void FixedUpdate()
    {
        if (NetcodeManager.InGame)
        {
            if (IsOwner)
            {
                FixedUpdateOnlineOwner();
                FixedUpdateAnyOwner();
            }
            else FixedUpdateOnlineNotOwner();
        }
        else
        {
            FixedUpdateOffline();
            FixedUpdateAnyOwner();
        }
    }


    protected virtual void StartOnlineOwner() {}
    protected virtual void StartOffline() {}
    protected virtual void StartAnyOwner() {}
    
    protected virtual void DisableOnlineOwner() {}
    protected virtual void DisableOffline() {}
    protected virtual void DisableAnyOwner() {}
    
    protected virtual void UpdateOnlineOwner() {}
    protected virtual void UpdateAnyOwner() {}
    protected virtual void UpdateOnlineNotOwner() {}
    protected virtual void UpdateOffline() {}
    
    protected virtual void FixedUpdateOnlineOwner() {}
    protected virtual void FixedUpdateAnyOwner() {}
    protected virtual void FixedUpdateOnlineNotOwner() {}
    protected virtual void FixedUpdateOffline() {}

    
    
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