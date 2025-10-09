using System;
using System.Collections.Generic;
using System.Linq;
using Base_Scripts;
using Game.Common;
using Game.Common.List;
using Game.Data;
using Unity.Netcode;
using UnityEngine;

namespace Game.Upgrade.Perks
{
    public class ItemSelectionManager : NetworkBehaviour
    {
        public readonly Dictionary<ulong,ushort[]> PlayerDrawnItems = new();
        public readonly Dictionary<ulong,ushort> PlayerItemChoice = new();
    
        public static event Action<ushort[]> OnStartChooseItem;
        [Rpc(SendTo.SpecifiedInParams)] private void OnItemsDrawnClientRpc(ushort[] itemIndex, RpcParams @params) => OnStartChooseItem?.Invoke(itemIndex);
    
        public static event Action<ushort> OnItemChosenOwner;
        [Rpc(SendTo.SpecifiedInParams)]private void OnItemChosenClientRpc(ushort itemIndex, RpcParams @params) => OnItemChosenOwner?.Invoke(itemIndex);
    
        // server
        public void GiveChosenItemToPlayers()
        {
            foreach (ulong clientId in DataManager.Instance.GetKeys())
            {
                if (!PlayerItemChoice.TryGetValue(clientId, out var choiceInd)) // premier choix = 0, etc
                    PlayerItemChoice.Add(clientId, 0);

                ushort[] drawnItems = PlayerDrawnItems[clientId];

                if (drawnItems == null)
                {
                    NetcodeLogger.Instance.LogRpc($"Player {clientId} did not have any items to choose from.", NetcodeLogger.LogType.ItemSelection);
                }
                else
                {
                    ushort chosenItemInd = drawnItems[choiceInd];
                    Item chosenItem = ItemRegistry.Instance.GetItem(chosenItemInd);

                    NetcodeLogger.Instance.LogRpc($"Player {clientId} had these choices: {string.Join(", ", drawnItems)}", NetcodeLogger.LogType.ItemSelection);
                    NetcodeLogger.Instance.LogRpc($"and chose index {choiceInd} (item {chosenItemInd} {chosenItem.Info.Name})", NetcodeLogger.LogType.ItemSelection);

                    PlayerData data = DataManager.Instance[clientId];
                    DataManager.Instance[clientId] = new(data) {inGameData = data.inGameData.AddItem(chosenItemInd)};
                
                    OnItemChosenClientRpc(chosenItemInd, data.SendRpcTo());
                }
            }
            ResetChoices();
        }

        public void ChooseItemsForPlayersServer()
        {
            foreach (PlayerData data in Data.DataManager.Instance.GetValues())
            {
                OuterData.PlayingState state = data.outerData.playingState;
                if (state != OuterData.PlayingState.Playing) continue;
                PlayerDrawnItems[data.clientId] = DrawItems(GameSettings.Instance.ItemSelectionChoices, data);
                OnItemsDrawnClientRpc(PlayerDrawnItems[data.clientId], data.SendRpcTo());
            }
        }
        public ushort[] DrawItems(int amount, PlayerData data)
        {
            ushort[] ownedItems = data.inGameData.items;
            List<ushort> availableItems = new List<ushort>();
            Item[] selectionItems = ItemRegistry.Instance.ItemsInSelection;
            ushort[] allItemIndices = new ushort[selectionItems.Length];
            for (ushort i = 0; i < selectionItems.Length; i++) allItemIndices[i] = ItemRegistry.Instance.GetItem(selectionItems[i]);
            
            for (ushort i = 0; i < allItemIndices.Length; i++)
            {
                if (ownedItems.Contains(allItemIndices[i])) continue;
                availableItems.Add(allItemIndices[i]);
            }

            if (availableItems.Count == 0) return null;
            
            amount = Mathf.Min(amount, availableItems.Count);
            return availableItems.OrderBy(_ => UnityEngine.Random.value).Take(amount).ToArray();
        }
    
        public void ResetChoices()
        {
            PlayerDrawnItems.Clear();
            PlayerItemChoice.Clear();
        }
    
    
        // client
        public void ChooseItemClient(ushort itemIndex) => ClientChoseItemServerRpc(itemIndex, RpcParamsExt.Instance.SenderClientID(NetworkManager.LocalClientId));

        [Rpc(SendTo.Server)]
        public void ClientChoseItemServerRpc(ushort itemIndex, RpcParams @params)
        {
            PlayerItemChoice[@params.Receive.SenderClientId] = itemIndex;
        }
    }
}