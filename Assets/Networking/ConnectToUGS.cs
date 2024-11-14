using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

public class ConnectToUGS : MonoBehaviour
{
    public UnityEvent uOnConnected;
    public Action OnConnected;
    private void Start()
    {
        Connect();
    }

    public async void Connect()
    {
        await UnityServices.InitializeAsync();
        
        if (AuthenticationService.Instance.IsSignedIn) return;
        
#if UNITY_EDITOR 
        if (ParrelSync.ClonesManager.IsClone())
        {
            // When using a ParrelSync clone, switch to a different authentication profile to force the clone
            // to sign in as a different anonymous user account.
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif

        AuthenticationService.Instance.SignedIn += () => Debug.Log("Connected to UGS as Player " + AuthenticationService.Instance.PlayerId);
        
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        OnConnected?.Invoke();
        uOnConnected.Invoke();
    }
}
