using Game.Data;
using Sirenix.OdinInspector;
    using Unity.Netcode;
    using UnityEngine;

    namespace Game.Game_Loop.Round.Collect.Resource
    {
        public class ResourceStructure : NetworkBehaviour
        {
            [SerializeField] private ResourceStructureData data;
            public ResourceStructureData Data => data;
            
            [ShowInInspector] [ReadOnly] private int currentDurability;

            public override void OnNetworkSpawn()
            {
                base.OnNetworkSpawn();

                if (!IsServer) return;
                
                currentDurability = data.durability;
            }

            /// <summary>
            /// CLIENT ONLY
            /// </summary>
            public void Extract()
            {
                ushort tick = GameTickManager.CurrentTick;
                ulong origin = NetworkManager.Singleton.LocalClientId;
                
                ExtractServerRpc(origin, tick);
            }

            [ServerRpc(RequireOwnership = false)]
            private void ExtractServerRpc(ulong origin, ushort tick)
            {
                if (tick > GameTickManager.CurrentTick)
                    throw new System.Exception($"Tick {tick} is greater than current tick {GameTickManager.CurrentTick}");
                
                if (currentDurability <= 0)
                {
                    Debug.Log($"Resource {Data.structureName} is already exploited");
                    return;
                }
                
                currentDurability--;

                if (!GiveResource()) return;

                ushort collectAmount = 1;
                
                DataManager.Instance.PlayerResources.CollectResource(origin, data.collectedResource, collectAmount);
            }
            
            
            private bool GiveResource() => currentDurability % data.resourceAmount == 0;
        }
    }