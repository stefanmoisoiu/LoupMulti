using Base_Scripts;
using Game.Common;
using Game.Data;
using Game.Game_Loop;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Collect.Resource.Structure
    {
        public class ResourceStructure : NetworkBehaviour
        {
            [SerializeField] private ResourceStructureData data;
            
            public ResourceStructureData Data => data;
            
            [ShowInInspector] [ReadOnly] private int currentDurability;
            public NetworkVariable<bool> fullyExploited = new();

            [SerializeField] private UnityEvent OnExtractOwner;
            [SerializeField] private UnityEvent<ulong> OnExtractAll;
            [SerializeField] private UnityEvent<ulong> OnGiveResourceAll;
            [SerializeField] private UnityEvent OnGiveResourceOwner;
            [SerializeField] private UnityEvent OnExtractServer;
            [SerializeField] private UnityEvent OnGiveResourceServer;
            [SerializeField] private UnityEvent OnFullyExtractedOwner;
            [SerializeField] private UnityEvent<ulong> OnFullyExtractedAll;
            [SerializeField] private UnityEvent OnFullyExtractedServer;

            private int GetMaxDurability() => data.durabilityPerResource * data.resourceAmount;
            
            public override void OnNetworkSpawn()
            {
                base.OnNetworkSpawn();

                if (!IsServer) return;
                
                currentDurability = GetMaxDurability();
            }
            
            /// <summary>
            /// CLIENT ONLY
            /// </summary>
            public void Extract(ushort amount)
            {
                ushort tick = GameTickManager.CurrentTick;
                ulong origin = NetworkManager.Singleton.LocalClientId;
                
                Debug.Log($"Extracting Resource {origin}");
                
                ExtractServerRpc(origin, tick, amount);
            }

            [ServerRpc(RequireOwnership = false)]
            private void ExtractServerRpc(ulong origin, ushort tick, ushort extractAmount = 1)
            {
                if (tick > GameTickManager.CurrentTick)
                    throw new System.Exception($"Tick {tick} is greater than current tick {GameTickManager.CurrentTick}");
                
                if (currentDurability <= 0)
                {
                    Debug.Log($"Resource {Data.structureName} is already exploited");
                    return;
                }

                if (currentDurability < extractAmount) extractAmount = (ushort)currentDurability;
                
                ushort collectAmount = 0;
                for (int i = 0; i < extractAmount; i++)
                {
                    currentDurability--;
                    if (GiveResource()) collectAmount++;
                }

                
                if (collectAmount > 0)
                {
                    NetcodeLogger.Instance.LogRpc($"{origin} got {collectAmount} resource from {Data.structureName} {currentDurability}/{GetMaxDurability()}", NetcodeLogger.LogType.GameLoop);
                
                    DataManager.Instance.PlayerResources.CollectResource(origin, data.collectedResource, collectAmount);
                
                
                    OnGiveResourceServer?.Invoke();
                    OnGiveResourceOwnerClientRpc(new ClientRpcParams() {Send = new ClientRpcSendParams() {TargetClientIds = new ulong[] { origin }}});
                    OnGiveResourceClientRpc(origin);
                } 
                
                OnExtractServer?.Invoke();
                
                
                OnExtractOwnerClientRpc(new ClientRpcParams() {Send = new ClientRpcSendParams() {TargetClientIds = new ulong[] { origin }}});
                OnExtractAllClientRpc(origin);
                
                if (currentDurability <= 0)
                {
                    NetcodeLogger.Instance.LogRpc($"Resource {Data.structureName} fully exploited", NetcodeLogger.LogType.GameLoop);
                    OnFullyExtractedOwnerClientRpc(new ClientRpcParams() {Send = new ClientRpcSendParams() {TargetClientIds = new ulong[] { origin }}});
                    OnFullyExtractedAllClientRpc(origin);
                    
                    fullyExploited.Value = true;
                    
                    OnFullyExtractedServer?.Invoke();
                }
            }
            
            
            [ClientRpc] private void OnExtractOwnerClientRpc(ClientRpcParams clientRpcParams = default) => OnExtractOwner?.Invoke();
            [ClientRpc] private void OnExtractAllClientRpc(ulong origin, ClientRpcParams clientRpcParams = default) => OnExtractAll?.Invoke(origin);
            
            [ClientRpc] private void OnGiveResourceOwnerClientRpc(ClientRpcParams clientRpcParams = default) => OnGiveResourceOwner?.Invoke();
            [ClientRpc] private void OnGiveResourceClientRpc(ulong origin) => OnGiveResourceAll?.Invoke(origin);
            
            [ClientRpc] private void OnFullyExtractedOwnerClientRpc(ClientRpcParams clientRpcParams = default) => OnFullyExtractedOwner?.Invoke();
            [ClientRpc] private void OnFullyExtractedAllClientRpc(ulong origin, ClientRpcParams clientRpcParams = default) => OnFullyExtractedAll?.Invoke(origin);
            
            private bool GiveResource() => currentDurability % data.durabilityPerResource == 0;
        }
    }