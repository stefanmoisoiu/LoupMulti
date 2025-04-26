using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Game.Data
{
    public class PlayerDataManager : NetworkBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }
    
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
    
        public static event Action<PlayerData> OnEntryUpdatedOwner;

        private void Awake()
        {
            Instance = this;
        }

        // --- API Serveur ---
    
        /// <summary>
        /// Ajoute une nouvelle entrée et réplique à tous les clients.
        /// Doit être appelé côté serveur uniquement.
        /// </summary>
        public void AddEntry(PlayerData newPd, ClientRpcParams rpcParams = default)
        {
            if (!IsServer)
            {
                Debug.LogError("AddEntry() can only be called on the server.");
                return;
            }
            _data[newPd.ClientId] = newPd;
            OnEntryAddedServer?.Invoke(newPd);
            AddEntryClientRpc(newPd);
        }

        /// <summary>
        /// Met à jour l'entrée et réplique à tous les clients.
        /// </summary>
        public void UpdateEntry(PlayerData newPd, ClientRpcParams rpcParams = default)
        {
            if (!IsServer)
            {
                throw new Exception("UpdateEntry() can only be called on the server.");
            }
            if (!_data.ContainsKey(newPd.ClientId))
            {
                return;
            }
            PlayerData previousData = _data[newPd.ClientId];
            _data[newPd.ClientId] = newPd;
            OnEntryUpdatedServer?.Invoke(previousData, newPd);
            UpdateEntryClientRpc(previousData, newPd);
        }

        /// <summary>
        /// Supprime l'entrée et réplique à tous les clients.
        /// </summary>
        public void RemoveEntry(ulong clientId, ClientRpcParams rpcParams = default)
        {
            if (!IsServer)
            {
                throw new Exception("RemoveEntry() can only be called on the server.");
            }
            if (!_data.Remove(clientId)) return;
            OnEntryRemovedServer?.Invoke(_data[clientId]);
            RemoveEntryClientRpc(clientId);
        }

        // --- RPCs Clients ---

        [ClientRpc]
        private void AddEntryClientRpc(PlayerData newPd, ClientRpcParams rpcParams = default)
        {
            _data[newPd.ClientId] = newPd;
            OnEntryAddedClient?.Invoke(newPd);
        }

        [ClientRpc]
        private void UpdateEntryClientRpc(PlayerData previousPd, PlayerData newPd, ClientRpcParams rpcParams = default)
        {
            _data[newPd.ClientId] = newPd;
            OnEntryUpdatedClient?.Invoke(previousPd, newPd);
            if (newPd.ClientId == NetworkManager.Singleton.LocalClientId) OnEntryUpdatedOwner?.Invoke(newPd);
        }

        [ClientRpc]
        private void RemoveEntryClientRpc(ulong clientId, ClientRpcParams rpcParams = default)
        {
            if (!_data.Remove(clientId, out var pd)) return;
            OnEntryRemovedClient?.Invoke(pd);
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
                if (_data.TryGetValue(clientId, out var pd))
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
    
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer) return;

            // … tes initialisations actuelles …
        
            AddEntry(new PlayerData(NetworkManager.Singleton.LocalClient));

            // À chaque connexion, on "sync" tout l'état actuel au newcomer
            NetworkManager.Singleton.OnClientConnectedCallback += clientId =>
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
                    AddEntry(_data[key], rpcParams);
                }
            
                // On ajoute le joueur au dictionnaire
                var client = NetworkManager.Singleton.ConnectedClients[clientId];
            
                // On envoie un RPC pour synchroniser l'état actuel
                AddEntry(new PlayerData(client));
            };
        }
    }
}
