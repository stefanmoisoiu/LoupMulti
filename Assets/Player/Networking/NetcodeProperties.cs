using Lobby;
using UnityEngine;

namespace Player.Networking
{
    public class NetcodeProperties : PNetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;

        protected override void StartOnlineNotOwner()
        {
            rb.useGravity = false;
            gameObject.name = $"Player [{OwnerClientId}]";
        }

        protected override void StartOnlineOwner()
        {
            gameObject.name = $"MY Player [{OwnerClientId}]";
        }

        protected override void StartOffline()
        {
            gameObject.name = "Player [OFFLINE]";
            MultiplayerDashboard.StartEnterGame += DisableObject;
            MultiplayerDashboard.FailedEnterGame += EnableObject;
        }
        private void EnableObject() => SetEnabledState(true);
        private void DisableObject() => SetEnabledState(false);
        private void SetEnabledState(bool value)
        {
            gameObject.SetActive(value);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            MultiplayerDashboard.StartEnterGame -= DisableObject;
            MultiplayerDashboard.FailedEnterGame -= EnableObject;
        }
    }
}