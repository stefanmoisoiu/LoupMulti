using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Networking
{
    public class ConnectToUGS : MonoBehaviour
    {
        public UnityEvent uOnConnected;
        public static event Action OnConnected;
        private void Start()
        {
            Connect();
        }

        public async void Connect()
        {
#if UNITY_EDITOR 
            /*if (ParrelSync.ClonesManager.IsClone())
        {
            // When using a ParrelSync clone, switch to a different authentication profile to force the clone
            // to sign in as a different anonymous user account.
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            InitializationOptions options = new();
            options.SetProfile($"Clone_{customArgument}_Profile");
            await UnityServices.InitializeAsync(options);
        }
        else*/
            await UnityServices.InitializeAsync();
#else
        await UnityServices.InitializeAsync();
#endif
        
            if (AuthenticationService.Instance.IsSignedIn) return;
        


            AuthenticationService.Instance.SignedIn += () => Debug.Log("Connected to UGS as Player " + AuthenticationService.Instance.PlayerId);
        
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
            OnConnected?.Invoke();
            uOnConnected.Invoke();
        }
    }
}
