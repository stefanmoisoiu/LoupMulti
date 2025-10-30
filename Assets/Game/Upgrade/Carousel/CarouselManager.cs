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
        [SerializeField] private WeightedProbabilitySelector<CarouselOption.OptionType> carouselOptionProbability = new();
        [SerializeField] private WeightedProbabilitySelector<ObjectRarity> rarityProbability = new();
        
        
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
                     ushort currentLevel = data.inGameData.GetAllOwnedItems().First(x => x.ItemRegistryIndex == chosenOption.ItemRegistryIndex).Level;
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
        
       private struct CarouselPools
        {
            public List<Item> NewAbilities;
            public List<Item> NewPerks;
            public List<Item> UpgradeableItems;
            public List<OwnedItemData> UpgradeableItemsData;

            public int TotalAvailable() => NewAbilities.Count + NewPerks.Count + UpgradeableItems.Count;

            public static CarouselPools Create()
            {
                return new CarouselPools
                {
                    NewAbilities = new List<Item>(),
                    NewPerks = new List<Item>(),
                    UpgradeableItems = new List<Item>(),
                    UpgradeableItemsData = new List<OwnedItemData>()
                };
            }
        }

        private CarouselOption[] GenerateCarouselOptions(PlayerData data, CarouselTypeConfig config)
        {
            List<CarouselOption> finalChoices = new List<CarouselOption>();
            
            CarouselPools pools = BuildAvailablePools(data, config);

            ApplyForcedAbilityRule(data, config, ref finalChoices, pools);

            int choicesNeeded = config.numberOfChoices - finalChoices.Count;

            if (choicesNeeded > 0)
            {
                FillRemainingChoices(choicesNeeded, config, ref finalChoices, pools);
            }
            
            return finalChoices.OrderBy(_ => Random.value).ToArray();
        }

        private CarouselPools BuildAvailablePools(PlayerData data, CarouselTypeConfig config)
        {
            List<OwnedItemData> ownedItemsData = data.inGameData.GetAllOwnedItems();
            List<ushort> ownedItemIndices = ownedItemsData.Select(item => item.ItemRegistryIndex).ToList();
            Item[] selectionPool = ItemRegistry.Instance.ItemsInCarousel;

            CarouselPools pools = CarouselPools.Create();

            foreach (Item item in selectionPool)
            {
                ushort itemIndex = ItemRegistry.Instance.GetItem(item);
                if (ownedItemIndices.Contains(itemIndex))
                {
                    OwnedItemData owned = ownedItemsData.First(oi => oi.ItemRegistryIndex == itemIndex);
                    if (config.allowUpgrades && owned.Level < GameSettings.Instance.MaxItemLevel)
                    {
                        pools.UpgradeableItemsData.Add(owned);
                        pools.UpgradeableItems.Add(item);
                    }
                }
                else
                {
                    if (!config.allowNewItems) continue;
                    switch (item.Type)
                    {
                        case Item.ItemType.Ability when config.allowNewAbilities:
                            pools.NewAbilities.Add(item);
                            break;
                        case Item.ItemType.Perk when config.allowNewPerks:
                            pools.NewPerks.Add(item);
                            break;
                    }
                }
            }
            return pools;
        }
        
        private void ApplyForcedAbilityRule(PlayerData data, CarouselTypeConfig config, ref List<CarouselOption> finalChoices, CarouselPools pools)
        {
            int activeItemCount = data.inGameData.ownedItems.Count(itemData => ItemRegistry.Instance.GetItem(itemData.ItemRegistryIndex).Type == Item.ItemType.Ability);
            bool mustForceActiveCheck = config.forceActiveIfNotMax && activeItemCount < GameSettings.Instance.AbilitySlots;

            if (mustForceActiveCheck && pools.NewAbilities.Count > 0)
            {
                Item forcedItem = SelectItemByRarity(pools.NewAbilities);
                if (forcedItem != null)
                {
                    finalChoices.Add(new CarouselOption(CarouselOption.OptionType.NewAbilty, ItemRegistry.Instance.GetItem(forcedItem)));
                    pools.NewAbilities.Remove(forcedItem);
                }
            }
        }
        
        private void FillRemainingChoices(int choicesNeeded, CarouselTypeConfig config, ref List<CarouselOption> finalChoices, CarouselPools pools)
        {
            while (choicesNeeded > 0 && pools.TotalAvailable() > 0)
            {
                List<WeightedProbabilitySelector<CarouselOption.OptionType>.WeightedOutcome> validWeights = new();
                foreach(var weightedType in carouselOptionProbability.GetAllOutcomes())
                {
                    switch (weightedType.Outcome)
                    {
                        case CarouselOption.OptionType.NewAbilty when pools.NewAbilities.Count > 0:
                            validWeights.Add(weightedType);
                            break;
                        case CarouselOption.OptionType.NewPerk when pools.NewPerks.Count > 0:
                            validWeights.Add(weightedType);
                            break;
                        case CarouselOption.OptionType.UpgradeItem when config.allowUpgrades && pools.UpgradeableItems.Count > 0:
                            validWeights.Add(weightedType);
                            break;
                    }
                }

                if (validWeights.Count == 0) break;

                CarouselOption.OptionType chosenType = carouselOptionProbability.GetRandomOutcome(validWeights);
                
                Item optionItem;
                ushort currentLevel = 0;
                List<Item> selectionList;
                
                switch (chosenType)
                {
                    case CarouselOption.OptionType.UpgradeItem:
                        selectionList = pools.UpgradeableItems;
                        break;
                    case CarouselOption.OptionType.NewAbilty:
                        selectionList = pools.NewAbilities;
                        break;
                    case CarouselOption.OptionType.NewPerk:
                        selectionList = pools.NewPerks;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                optionItem = SelectItemByRarity(selectionList);
                if (optionItem == null) continue;

                int originalIndex = selectionList.IndexOf(optionItem);
                selectionList.RemoveAt(originalIndex);

                if (chosenType == CarouselOption.OptionType.UpgradeItem)
                {
                    currentLevel = pools.UpgradeableItemsData[originalIndex].Level;
                    pools.UpgradeableItemsData.RemoveAt(originalIndex);
                }

                CarouselOption generatedOption = new(chosenType, ItemRegistry.Instance.GetItem(optionItem), currentLevel);
                finalChoices.Add(generatedOption);
                
                choicesNeeded--;
            }
        }
        
        private Item SelectItemByRarity(List<Item> availableItems)
        {
            if (availableItems == null || availableItems.Count == 0)
            {
                return null;
            }

            HashSet<ObjectRarity> availableRarities = new HashSet<ObjectRarity>(
                availableItems.Select(item => item.Info.Rarity)
            );

            List<WeightedProbabilitySelector<ObjectRarity>.WeightedOutcome> validRarityWeights =
                rarityProbability.GetAllOutcomes()
                    .Where(outcome => availableRarities.Contains(outcome.Outcome) && outcome.Weight > 0)
                    .ToList();

            ObjectRarity chosenRarity;

            if (validRarityWeights.Count > 0)
            {
                chosenRarity = rarityProbability.GetRandomOutcome(validRarityWeights);
            }
            else
            {
                chosenRarity = availableRarities.First();
                Debug.LogWarning($"[CarouselManager] No valid rarity weights found for available items. Falling back to first available rarity: {chosenRarity}");
            }

            List<Item> finalPool = availableItems
                .Where(item => item.Info.Rarity == chosenRarity)
                .ToList();

            if (finalPool.Count == 0)
            {
                 Debug.LogError($"[CarouselManager] Rarity selection failed. Final pool for {chosenRarity} was empty. Returning first available item.");
                 return availableItems[0];
            }

            int ind = Random.Range(0, finalPool.Count);
            return finalPool[ind];
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