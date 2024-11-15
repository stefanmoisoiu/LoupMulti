using System;
using Unity.Netcode;
using UnityEngine;

public class POfflinePlayer : NetworkBehaviour
{
    [SerializeField] private bool isOfflinePlayer;

    private void OnEnable()
    {
        if (NetcodeManager.InGame) TryDeletePlayer();
        else NetworkManager.Singleton.OnConnectionEvent += DelConnectionEvent;
    }

    private void OnDisable()
    {
        if(NetworkManager != null && NetworkManager.Singleton != null) NetworkManager.Singleton.OnConnectionEvent -= DelConnectionEvent;
    }

    private void DelConnectionEvent(NetworkManager a, ConnectionEventData b)
    {
        if (b.EventType == ConnectionEvent.ClientConnected) TryDeletePlayer();
    }

    private void TryDeletePlayer()
    {
        try
        {
            if (isOfflinePlayer) Destroy(transform.root.gameObject);
        }
        catch
        {
            // ignored
        }
    }
}