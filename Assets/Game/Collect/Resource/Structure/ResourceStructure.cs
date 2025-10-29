using System;
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
        public class ResourceStructure : NetworkBehaviour,IDamageable
        {
            [SerializeField] private ResourceStructureData data;
            
            public ResourceStructureData Data => data;
            
            public NetworkVariable<ushort> currentDurability = new(value:ushort.MaxValue);

            public Action<ushort, ushort, ulong> OnExtractServer;
            public Action<ushort, ushort, ulong> OnExtractAll;
            [ClientRpc] private void OnExtractClientRpc(ushort amount, ushort collectedAmount, ulong origin) => OnExtractAll?.Invoke(amount, collectedAmount, origin);
            public Action OnFullyExtractedServer;
            public Action OnFullyExtractedAll;

            private ushort GetMaxDurability() => (ushort)(data.durabilityPerResource * data.resourceAmount);
            
            public override void OnNetworkSpawn()
            {
                base.OnNetworkSpawn();

                if (IsServer)
                {
                    currentDurability.Value = GetMaxDurability();
                }
                currentDurability.OnValueChanged += DurabilityChanged;
            }
            private void DurabilityChanged(ushort previous, ushort current)
            {
                if (current <= 0)
                {
                    OnFullyExtractedAll?.Invoke();
                }
            }

            public event Action<ushort> OnDamaged;

            public void TakeDamage(IDamageable.DamageInfo info)
            {
                ushort tick = GameTickManager.CurrentTick;
                
                TryExtractServerRpc(info.Origin, tick, info.ExtractAmount);
            }

            [ServerRpc(RequireOwnership = false)]
            private void TryExtractServerRpc(ulong origin, ushort tick, ushort extractAmount = 1)
            {
                if (tick > GameTickManager.CurrentTick)
                    throw new System.Exception($"Tick {tick} is greater than current tick {GameTickManager.CurrentTick}");
                
                if (currentDurability.Value <= 0)
                    throw new Exception($"Client {origin} tried extracting resource {Data.structureName} but it is already fully exploited");
                
                ExtractServer(extractAmount, origin);

                if (currentDurability.Value <= 0)
                {
                    NetcodeLogger.Instance.LogRpc($" {data.structureName} fully extracted", NetcodeLogger.LogType.GameLoop);
                    OnFullyExtractedServer?.Invoke();
                }
            }

            private void ExtractServer(ushort amount, ulong origin)
            {
                if (currentDurability.Value < amount) amount = currentDurability.Value;
                
                ushort collectedAmount = 0;
                for (int i = 0; i < amount; i++)
                {
                    currentDurability.Value--;
                    if (GiveResource()) collectedAmount++;
                }
                DataManager.Instance.PlayerResourcesHelper.CollectResource(origin, data.collectedResource,
                    collectedAmount);
                
                NetcodeLogger.Instance.LogRpc($"Client {origin} extracted {collectedAmount} {data.collectedResource.ResourceName} from {data.structureName}. Durability: {currentDurability.Value}/{GetMaxDurability()}", NetcodeLogger.LogType.GameLoop);
                
                OnExtractServer?.Invoke(amount, collectedAmount, origin);
                OnExtractClientRpc(amount, collectedAmount, origin);
                
                OnDamaged?.Invoke(collectedAmount);
            }
            
            private bool GiveResource() => currentDurability.Value % data.durabilityPerResource == 0;
        }
    }