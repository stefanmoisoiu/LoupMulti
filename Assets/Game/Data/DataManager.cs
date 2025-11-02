using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Data.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace Game.Data
{
    public class DataManager : NetworkBehaviour
    {
        public static DataManager Instance { get; private set; }

        [SerializeField] private PlayerResourcesHelper playerResourcesHelper;
        public PlayerResourcesHelper PlayerResourcesHelper => playerResourcesHelper;
        [SerializeField] private PlayerHealthHelper playerHealthHelper;
        public PlayerHealthHelper PlayerHealthHelper => playerHealthHelper;
        [SerializeField] private PlayerStateHelper playerStateHelper;
        public PlayerStateHelper PlayerStateHelper => playerStateHelper;
        
        
    
        private readonly Dictionary<ulong, PlayerData> _data = new Dictionary<ulong, PlayerData>();

        /// <summary>Événement d'ajout d'une entrée (clé, valeur).</summary>
        public static event Action<PlayerData> OnEntryAddedServer;
        public static event Action<PlayerData> OnEntryAddedClient;
        /// <summary>Événement de mise à jour d'une entrée existante.</summary>
        public static event Action<PlayerData, PlayerData> OnEntryUpdatedServer;
        public static event Action<PlayerData, PlayerData> OnEntryUpdatedClient;
        /// <summary>Événement de suppression d'une entrée.</summary>
        public static event Action<PlayerData> OnEntryRemovedServer;
        public static event Action<PlayerData> OnEntryRemovedClient;
    
        
        public static event Action<PlayerData, PlayerData> OnEntryUpdatedOwner;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // --- API Serveur ---
    
        /// <summary>
        /// Ajoute une nouvelle entrée et réplique à tous les clients.
        /// Doit être appelé côté serveur uniquement.
        /// </summary>
        public void AddEntry(PlayerData newPd)
        {
            Debug.Log($"Adding entry {newPd}");
            if (!IsServer)
            {
                Debug.LogError("AddEntry() can only be called on the server.");
                return;
            }
            _data[newPd.clientId] = newPd;
            OnEntryAddedServer?.Invoke(newPd);
            AddEntryClientRpc(newPd);
            UpdateEntryClientRpc(new(NetworkManager.ConnectedClients[newPd.clientId]), newPd);
        }

        /// <summary>
        /// Met à jour l'entrée et réplique à tous les clients.
        /// </summary>
        public void UpdateEntry(PlayerData newPd)
        {
            // Debug.Log($"Updating entry {newPd}");
            if (!IsServer)
            {
                throw new Exception("UpdateEntry() can only be called on the server.");
            }
            if (!_data.ContainsKey(newPd.clientId))
            {
                return;
            }
            PlayerData previousData = _data[newPd.clientId];
            _data[newPd.clientId] = newPd;
            OnEntryUpdatedServer?.Invoke(previousData, newPd);
            UpdateEntryClientRpc(previousData, newPd);
        }

        /// <summary>
        /// Supprime l'entrée et réplique à tous les clients.
        /// </summary>
        public void RemoveEntry(ulong clientId)
        {
            if (!IsServer)
            {
                throw new Exception("RemoveEntry() can only be called on the server.");
            }
            if (!_data.Remove(clientId)) return;
            OnEntryRemovedServer?.Invoke(_data[clientId]);
            RemoveEntryClientRpc(clientId);
        }
        
        public void ClearEntries()
        {
            if (!IsServer)
            {
                throw new Exception("ClearEntries() can only be called on the server.");
            }
            _data.Clear();
            ClearEntriesClientRpc();
        }

        // --- RPCs Clients ---

        [ClientRpc]
        private void AddEntryClientRpc(PlayerData newPd, ClientRpcParams rpcParams = default)
        {
            _data[newPd.clientId] = newPd;
            OnEntryAddedClient?.Invoke(newPd);
        }

        [ClientRpc]
        private void UpdateEntryClientRpc(PlayerData previousPd, PlayerData newPd, ClientRpcParams rpcParams = default)
        {
            _data[newPd.clientId] = newPd;
            OnEntryUpdatedClient?.Invoke(previousPd, newPd);
            if (newPd.clientId == NetworkManager.Singleton.LocalClientId) OnEntryUpdatedOwner?.Invoke(previousPd, newPd);
        }

        [ClientRpc]
        private void RemoveEntryClientRpc(ulong clientId, ClientRpcParams rpcParams = default)
        {
            if (!_data.Remove(clientId, out var pd)) return;
            OnEntryRemovedClient?.Invoke(pd);
        }
        
        [ClientRpc]
        private void ClearEntriesClientRpc(ClientRpcParams rpcParams = default)
        {
            _data.Clear();
        }

        // --- API Commun ---

        /// <summary>
        /// Tente de récupérer les données d'un client.
        /// </summary>
        public bool TryGetValue(ulong clientId, out PlayerData pd)
        {
            return _data.TryGetValue(clientId, out pd);
        }
        public PlayerData this[ulong clientId]
        {
            get
            {
                if (_data.TryGetValue(clientId, out PlayerData pd))
                    return pd;
                throw new KeyNotFoundException($"Client ID {clientId} not found in PlayerDataManager.");
            }
            set
            {
                if (!IsServer)
                {
                    throw new Exception("Set only callable from server.");
                }
                if (_data.ContainsKey(clientId))
                {
                    UpdateEntry(value);
                }
                else
                {
                    AddEntry(value);
                }
            }
        }
    
        public ulong[] GetKeys()
        {
            return _data.Keys.ToArray();
        }
        public PlayerData[] GetValues()
        {
            return _data.Values.ToArray();
        }
        public IReadOnlyDictionary<ulong, PlayerData> GetData()
        {
            return _data;
        }
        public int Count => _data.Count;
        public bool ContainsKey(ulong clientId)
        {
            return _data.ContainsKey(clientId);
        }
    
        public void Reset()
        {
            foreach (ulong clientId in _data.Keys)
            {
                if (NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) continue;
                RemoveEntry(clientId);
            }
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (_data.ContainsKey(client.ClientId)) UpdateEntry(new PlayerData(client));
                else AddEntry(new PlayerData(client));
            }
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer) return;
            Reset();
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerJoinedGame;
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= PlayerJoinedGame;
        }

        private void PlayerJoinedGame(ulong clientId)
        {
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[]{ clientId }
                }
            };

            // Pour chaque paire existante, on appelle notre AddEntryClientRpc
            ulong[] keys = GetKeys();
            foreach (ulong key in keys)
            {
                AddEntryClientRpc(_data[key], rpcParams);
            }
            
            // On ajoute le joueur au dictionnaire
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            
            // On envoie un RPC pour synchroniser l'état actuel
            AddEntry(new PlayerData(client));
        }
    }
}
