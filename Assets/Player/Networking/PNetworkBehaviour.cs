using System;
using Networking;
using Networking.Connection;
using Player.General_UI;
using Unity.Netcode;
using UnityEngine;

namespace Player.Networking
{
    public abstract class PNetworkBehaviour : NetworkBehaviour
    {
        protected bool IsOnline => NetcodeManager.InGame || NetcodeManager.LoadingGame;
        private bool _initializedAny = false;
        private bool _onNetworkSpawnCalled = false;

        private void OnEnable()
        {
            if (IsOnline)
            {
                if (_onNetworkSpawnCalled) OnEnableOnline();
            }
            else
            {
                OnEnableOffline();
            }
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _onNetworkSpawnCalled = true;
            if (!enabled) return;
            OnEnableOnline();
        }

        private void OnEnableOffline()
        {
            StartOffline();
            if (!_initializedAny) StartAnyOwner();
            _initializedAny = true;
        }

        private void OnEnableOnline()
        {
            if (IsServer) StartOnlineServer();
            if (IsOwner)  StartOnlineOwner();
            else
            {
                StartOnlineNotOwner();
                return;
            }
            if (!_initializedAny) StartAnyOwner();
            _initializedAny = true;
        }
        
        private void OnDisable()
        {
            _initializedAny = false;
            if (IsOnline)
            {
                if (IsServer) DisableOnlineServer();
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
            if (!enabled) return;
            if (NetcodeManager.InGame)
            {
                if (IsServer) UpdateOnlineServer();
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
            if (!enabled) return;
            if (NetcodeManager.InGame)
            {
                if (IsServer) FixedUpdateOnlineServer();
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


        protected virtual void StartOnlineServer() {}
        
        
        protected virtual void StartOnlineOwner() {}
        protected virtual void StartOnlineNotOwner() {}
        protected virtual void StartOffline() {}
        protected virtual void StartAnyOwner() {}
    
        
        protected virtual void DisableOnlineServer() {}
        
        protected virtual void DisableOnlineOwner() {}
        protected virtual void DisableOnlineNotOwner() {}
        protected virtual void DisableOffline() {}
        protected virtual void DisableAnyOwner() {}
    
        
        protected virtual void UpdateOnlineServer() {}
        
        
        protected virtual void UpdateOnlineOwner() {}
        protected virtual void UpdateAnyOwner() {}
        protected virtual void UpdateOffline() {}
        protected virtual void UpdateOnlineNotOwner() {}
    
        
        protected virtual void FixedUpdateOnlineServer() {}
        
        protected virtual void FixedUpdateOnlineOwner() {}
        protected virtual void FixedUpdateAnyOwner() {}
        protected virtual void FixedUpdateOffline() {}
        protected virtual void FixedUpdateOnlineNotOwner() {}
    }
}