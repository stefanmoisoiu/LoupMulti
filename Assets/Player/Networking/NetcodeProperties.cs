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
            gameObject.name = $"#b#redPlayer [{OwnerClientId}]";
        }

        protected override void StartOnlineOwner()
        {
            gameObject.name = $"#b#blueMY Player [{OwnerClientId}]";
        }

        protected override void StartOffline()
        {
            gameObject.name = $"#b#bluePlayer [OFFLINE]";
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