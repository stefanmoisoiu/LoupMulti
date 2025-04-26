using Networking;
using Unity.Netcode;
using UnityEngine;

namespace Player.Networking
{
    public class OfflineProperties : NetworkBehaviour
    {
        [SerializeField] private bool isOfflinePlayer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            TryDeletePlayer();
        }

        private void OnEnable()
        {
            if (NetcodeManager.InGame) TryDeletePlayer();
            else
            {
                NetcodeManager.OnEnterGame += TryDeletePlayer;
                NetworkManager.Singleton.OnConnectionEvent += DelConnectionEvent;
            }
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
            if (isOfflinePlayer)
            {
                transform.root.gameObject.SetActive(false);
                Destroy(transform.root.gameObject);
            }
        }
    }
}