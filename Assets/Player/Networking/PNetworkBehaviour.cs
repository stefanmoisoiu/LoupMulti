using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public abstract class PNetworkBehaviour : NetworkBehaviour
{
    private bool _initializedAny = false;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            StartOnlineNotOwner();
            return;
        }
        StartOnlineOwner();
        if (!_initializedAny) StartAnyOwner();
        _initializedAny = true;
    }
    private void Start()
    {
        if (IsSpawned || NetcodeManager.InGame) return;
        StartOffline();
        if (!_initializedAny) StartAnyOwner();
        _initializedAny = true;
    }
    private void OnDisable()
    {
        if (IsSpawned || NetcodeManager.InGame)
        {
            if (IsOwner) DisableOnlineOwner();
            else
            {
                DisableOnlineNotOwner();
                return;
            }
            
        }
        else
        {
            DisableOffline();
        }
        DisableAnyOwner();
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
    protected virtual void StartOnlineNotOwner() {}
    protected virtual void StartOffline() {}
    protected virtual void StartAnyOwner() {}
    
    protected virtual void DisableOnlineOwner() {}
    protected virtual void DisableOnlineNotOwner() {}
    protected virtual void DisableOffline() {}
    protected virtual void DisableAnyOwner() {}
    
    protected virtual void UpdateOnlineOwner() {}
    protected virtual void UpdateAnyOwner() {}
    protected virtual void UpdateOffline() {}
    protected virtual void UpdateOnlineNotOwner() {}
    
    protected virtual void FixedUpdateOnlineOwner() {}
    protected virtual void FixedUpdateAnyOwner() {}
    protected virtual void FixedUpdateOffline() {}
    protected virtual void FixedUpdateOnlineNotOwner() {}
}