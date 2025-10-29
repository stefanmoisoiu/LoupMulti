using System;
using System.Collections.Generic;
using System.Linq;
using Base_Scripts;
using Game.Common;
using Game.Common.List;
using Game.Data;
using Game.Data.Extensions;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Upgrade.Carousel
{
    public class CarouselManager : NetworkBehaviour
    {
        [SerializeField] private WeightedProbabilitySelector<CarouselOption.OptionType> carouselOptionProbability;
        
        
        private readonly Dictionary<ulong, CarouselOption[]> _playerCurrentOptions = new();
        private readonly Dictionary<ulong, CarouselTypeConfig> _playerActiveCarouselConfig = new();

        public static event Action<CarouselOption[], int> OnShowCarouselOptionsOwner;
        public static event Action<ushort> OnItemChosenOrUpgradedOwner;
        public static event Action OnCarouselClosedOwner;

        public void TriggerCommonCarouselServer(CarouselTypeConfig config)
        {
            if (!IsServer || config == null) return;

            foreach (PlayerData data in DataManager.Instance.Search(new[] { OuterData.PlayingState.Playing }))
            {
                TriggerPersonalCarouselServer(data.clientId, config);
            }
            Debug.Log("[CarouselManager] Common carousel triggered.");
        }
        public void TriggerPersonalCarouselServer(ulong targetClientId, CarouselTypeConfig config)
        {
            if (!IsServer || config == null) return;
            PlayerData data = DataManager.Instance[targetClientId];
            if (data.outerData.playingState != OuterData.PlayingState.Playing) return;

            ApplyChoice(targetClientId, 0);

            GenerateAndSendOptionsForPlayer(targetClientId, config);
            Debug.Log($"[CarouselManager] Personal carousel triggered for player {targetClientId}.");
        }

        public void ForceApplyAllRemainingCarousels()
        {
            if (!IsServer) return;
            
            List<ulong> playersToProcess = _playerActiveCarouselConfig.Keys.ToList();
            foreach (ulong clientId in playersToProcess)
            {
                ApplyChoice(clientId, 0);
            }
            
            Debug.Log("[CarouselManager] Applied default choices for all remaining players.");
        }

        private void GenerateAndSendOptionsForPlayer(ulong clientId, CarouselTypeConfig config)
        {
             PlayerData data = DataManager.Instance[clientId];
             CarouselOption[] options = GenerateCarouselOptions(data, config);

             _playerCurrentOptions[clientId] = options;
             _playerActiveCarouselConfig[clientId] = config;

             OnShowCarouselOptionsOwnerRpc(options, data.inGameData.rerollsAvailable, data.SendRpcTo());
             Debug.Log($"[CarouselManager] Options sent to player {clientId}.");
        }

        /// <summary>
        /// SERVEUR: Applique le choix d'un joueur et ferme son carousel actif.
        /// </summary>
        /// <param name="clientId">ID du joueur</param>
        /// <param name="choiceIndex">Index choisi</param>
        /// <param name="isForcedDefault">Indique si c'est un choix par défaut (timer écoulé)</param>
        private void ApplyChoice(ulong clientId, ushort choiceIndex)
        {
            // Vérifie si un carousel est VRAIMENT actif pour ce joueur
            if (!_playerActiveCarouselConfig.ContainsKey(clientId)) return;

            // Récupère les options qui étaient affichées
            if (!_playerCurrentOptions.TryGetValue(clientId, out var drawnOptions) || drawnOptions == null)
            {
                Debug.LogError($"[CarouselManager] Inconsistency: Player {clientId} had an active config but no drawn options recorded.");
                // Nettoie l'état même en cas d'erreur
                _playerActiveCarouselConfig.Remove(clientId);
                _playerCurrentOptions.Remove(clientId);
                 if (DataManager.Instance.TryGetValue(clientId, out var pData)) OnCarouselClosedClientRpc(pData.SendRpcTo());
                return;
            }

            PlayerData data = DataManager.Instance[clientId]; // Données actuelles

            if (drawnOptions.Length > 0)
            {
                 if (choiceIndex >= drawnOptions.Length)
                 {
                     Debug.LogWarning($"[CarouselManager] Invalid choice index {choiceIndex} received from player {clientId}. Ignoring.");
                     return;
                 }

                 CarouselOption chosenOption = drawnOptions[choiceIndex];

                 string logMsgAction = "";
                 if (chosenOption.Type == CarouselOption.OptionType.NewAbilty || chosenOption.Type == CarouselOption.OptionType.NewPerk)
                 {
                     data.inGameData = data.inGameData.AddItem(chosenOption.ItemRegistryIndex);
                     logMsgAction = "added";
                 }
                 else // UpgradeItem
                 {
                     // TODO: Intégrer la logique de chance pour multi-niveaux ici plus tard
                     ushort levelsToUpgrade = 1;
                     ushort currentLevel = data.inGameData.ownedItems.First(x => x.ItemRegistryIndex == chosenOption.ItemRegistryIndex).Level;
                     logMsgAction = $"upgraded [{currentLevel+1}] -> [{currentLevel+levelsToUpgrade+1}]";
                     data.inGameData = data.inGameData.UpgradeItem(chosenOption.ItemRegistryIndex, levelsToUpgrade);
                 }
                 DataManager.Instance[clientId] = data; // Met à jour le DataManager
                 OnItemChosenOrUpgradedOwnerRpc(chosenOption.ItemRegistryIndex, data.SendRpcTo());
                 Debug.Log($"Player {clientId} chose from carousel {choiceIndex}: {logMsgAction} {ItemRegistry.Instance.GetItem(chosenOption.ItemRegistryIndex).Info.Name}");
            }
            else {
                 Debug.LogWarning($"[CarouselManager] Player {clientId} had an active carousel but no options drawn.");
            }

            _playerCurrentOptions.Remove(clientId);
            _playerActiveCarouselConfig.Remove(clientId);
            OnCarouselClosedClientRpc(data.SendRpcTo());
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestRerollServerRpc(bool useRareResource, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            
            if (!_playerActiveCarouselConfig.TryGetValue(clientId, out CarouselTypeConfig activeConfig) || activeConfig == null)
            {
                 Debug.LogWarning($"Player {clientId} requested reroll but no carousel is active.");
                 return;
            }

            PlayerData data = DataManager.Instance[clientId];
            if (data.outerData.playingState != OuterData.PlayingState.Playing) return;

            bool canAffordRare = data.inGameData.resources.HasEnough(ResourceType.Rare, GameSettings.Instance.RareResourceRerollCost);
            int currentRerolls = data.inGameData.rerollsAvailable;
            bool rerollGranted = false;

            if (currentRerolls > 0 && !useRareResource)
            {
                DataManager.Instance[clientId] = new(data) { inGameData = data.inGameData.UseReroll() };
                rerollGranted = true;
                Debug.Log($"[CarouselManager] Player {clientId} used a reroll.");
            }
            else if (useRareResource && canAffordRare)
            {
                 InGameData ingData = data.inGameData.SetResources(data.inGameData.resources.RemoveResource(ResourceType.Rare, GameSettings.Instance.RareResourceRerollCost));
                 DataManager.Instance[clientId] = new(data) { inGameData = ingData };
                 rerollGranted = true;
                 Debug.Log($"[CarouselManager] Player {clientId} paid for a reroll.");
            }

            if (!rerollGranted)
            {
                Debug.LogError(
                    $"[CarouselManager] Player {clientId} failed to reroll (Rerolls: {currentRerolls}, UseRare: {useRareResource}, CanAfford: {canAffordRare}).");
            }
            else
            {
                GenerateAndSendOptionsForPlayer(clientId, activeConfig); // Regénère avec la même config
            }
        }

        private CarouselOption[] GenerateCarouselOptions(PlayerData data, CarouselTypeConfig config)
        {
            List<OwnedItemData> ownedItemsData = data.inGameData.ownedItems;
            List<ushort> ownedItemIndices = ownedItemsData.Select(item => item.ItemRegistryIndex).ToList();
            Item[] selectionPool = ItemRegistry.Instance.Items;

            List<Item> availableNewAbilities = new List<Item>();
            List<Item> availableNewPerks = new List<Item>();
            List<OwnedItemData> availableUpgradesOwned = new List<OwnedItemData>();
            List<Item> availableUpgrades = new List<Item>();

            foreach (Item item in selectionPool)
            {
                ushort itemIndex = ItemRegistry.Instance.GetItem(item);
                if (ownedItemIndices.Contains(itemIndex))
                {
                    OwnedItemData owned = ownedItemsData.First(oi => oi.ItemRegistryIndex == itemIndex);
                    if (config.allowUpgrades && owned.Level < GameSettings.Instance.MaxItemLevel)
                    {
                        availableUpgradesOwned.Add(owned);
                        availableUpgrades.Add(item);
                    }
                }
                else
                {
                    if (!config.allowNewItems) continue;
                    switch (item.Type)
                    {
                        case Item.ItemType.Ability when config.allowNewAbilities:
                            availableNewAbilities.Add(item);
                            break;
                        case Item.ItemType.Perk when config.allowNewPerks:
                            availableNewPerks.Add(item);
                            break;
                    }
                }
            }

            // 2. Assemblage
            List<CarouselOption> finalChoices = new List<CarouselOption>();
            int choicesNeeded = config.numberOfChoices;

            // Logique de forçage (réservation)
            int activeItemCount = ownedItemsData.Count(itemData => ItemRegistry.Instance.GetItem(itemData.ItemRegistryIndex).Type == Item.ItemType.Ability);
            bool mustForceActiveCheck = config.forceActiveIfNotMax && activeItemCount < GameSettings.Instance.AbilitySlots;

            if (mustForceActiveCheck && availableNewAbilities.Count > 0 && choicesNeeded > 0)
            {
                int idx = Random.Range(0, availableNewAbilities.Count);
                Item forcedItem = availableNewAbilities[idx];
                finalChoices.Add(new CarouselOption(CarouselOption.OptionType.NewAbilty, ItemRegistry.Instance.GetItem(forcedItem)));
                availableNewAbilities.RemoveAt(idx);
                choicesNeeded--;
            }
            List<Item> currentNewAbilities = new List<Item>(availableNewAbilities);
            List<Item> currentNewPerks = new List<Item>(availableNewPerks);
            List<OwnedItemData> currentUpgradesOwned = new List<OwnedItemData>(availableUpgradesOwned);
            List<Item> currentUpgrades = new List<Item>(availableUpgrades);

            while (choicesNeeded > 0 && (currentNewAbilities.Count + currentNewPerks.Count + currentUpgrades.Count > 0))
            {
                List<WeightedProbabilitySelector<CarouselOption.OptionType>.WeightedOutcome> validWeights = new();
                foreach(var weightedType in carouselOptionProbability.GetAllOutcomes())
                {
                    switch (weightedType.Outcome)
                    {
                        case CarouselOption.OptionType.NewAbilty when currentNewAbilities.Count > 0:
                            validWeights.Add(weightedType);
                            break;
                        case CarouselOption.OptionType.NewPerk when currentNewPerks.Count > 0:
                            validWeights.Add(weightedType);
                            break;
                        case CarouselOption.OptionType.UpgradeItem when config.allowUpgrades && currentUpgrades.Count > 0:
                            validWeights.Add(weightedType);
                            break;
                    }
                }

                if (validWeights.Count == 0) break;

                CarouselOption.OptionType chosenType = carouselOptionProbability.GetRandomOutcome(validWeights);

                List<Item> list = chosenType switch
                {
                    CarouselOption.OptionType.UpgradeItem => currentUpgrades,
                    CarouselOption.OptionType.NewAbilty => currentNewAbilities,
                    CarouselOption.OptionType.NewPerk => currentNewPerks,
                    _ => throw new ArgumentOutOfRangeException()
                };

                int ind = Random.Range(0, list.Count);
                Item optionItem = list[ind];
                ushort currentLevel = chosenType == CarouselOption.OptionType.UpgradeItem ? currentUpgradesOwned[ind].Level : (ushort)0;
                CarouselOption generatedOption = new(chosenType, ItemRegistry.Instance.GetItem(optionItem), currentLevel);
                
                list.RemoveAt(ind);
                if (chosenType == CarouselOption.OptionType.UpgradeItem) currentUpgradesOwned.RemoveAt(ind);

                finalChoices.Add(generatedOption);
                
                choicesNeeded--;
            }
            
            return finalChoices.OrderBy(_ => Random.value).ToArray();
        }

        /// <summary>
        /// SERVEUR: RPC appelé par le client quand il clique sur une option.
        /// Applique immédiatement le choix.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ClientChoseItemServerRpc(ushort choiceIndex, ServerRpcParams rpcParams = default)
        {
             ulong clientId = rpcParams.Receive.SenderClientId;
             // Applique le choix reçu et ferme le carousel pour ce joueur
             ApplyChoice(clientId, choiceIndex);
        }

        // Renommé pour plus de clarté
        public void ResetServerData()
        {
            if (!IsServer) return;
            _playerActiveCarouselConfig.Clear();
        }

        // ==============================
        //          RPCs CLIENT (inchangés)
        // ==============================
        [Rpc(SendTo.SpecifiedInParams)] private void OnShowCarouselOptionsOwnerRpc(CarouselOption[] options, int currentRerolls, RpcParams rpcParams = default) { OnShowCarouselOptionsOwner?.Invoke(options, currentRerolls); }
        [Rpc(SendTo.SpecifiedInParams)] private void OnItemChosenOrUpgradedOwnerRpc(ushort itemIndex, RpcParams rpcParams = default) { OnItemChosenOrUpgradedOwner?.Invoke(itemIndex); }
        [Rpc(SendTo.SpecifiedInParams)] private void OnCarouselClosedClientRpc(RpcParams rpcParams = default) { OnCarouselClosedOwner?.Invoke(); }
    }
}