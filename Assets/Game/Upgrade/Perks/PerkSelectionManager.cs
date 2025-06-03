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
    public class PerkSelectionManager : NetworkBehaviour
    {
        public readonly Dictionary<ulong,ushort[]> PlayerAvailablePerks = new();
        public readonly Dictionary<ulong,ushort> PlayerPerkChoice = new();
    
        public static event Action<ushort[]> OnStartChoosePerks;
        [Rpc(SendTo.SpecifiedInParams)] private void OnPerkChoicesAvailableClientRpc(ushort[] perkIndex, RpcParams @params) => OnStartChoosePerks?.Invoke(perkIndex);
    
        public static event Action<ushort> OnPerkChosenOwner;
        [Rpc(SendTo.SpecifiedInParams)]private void OnPerkChosenClientRpc(ushort perkIndex, RpcParams @params) => OnPerkChosenOwner?.Invoke(perkIndex);
    
        // server
        public void GiveChosenPerksToPlayers()
        {
            // If player did not choose, we give them the first perks
            foreach (ulong clientId in DataManager.Instance.GetKeys())
            {
                if (!PlayerPerkChoice.TryGetValue(clientId, out var perkChoiceIndex)) // premier choix = 0, etc
                    PlayerPerkChoice.Add(clientId, 0);

                ushort[] availableChoices = PlayerAvailablePerks[clientId];
                ushort chosenPerk = availableChoices[PlayerPerkChoice[clientId]];

                NetcodeLogger.Instance.LogRpc($"Player {clientId} had these choices: {string.Join(", ", availableChoices)}", NetcodeLogger.LogType.Perks);
                NetcodeLogger.Instance.LogRpc($"and chose {PerkSelectionList.Instance.GetPerk(perkChoiceIndex).PerkName}", NetcodeLogger.LogType.Perks);

                PlayerData data = DataManager.Instance[clientId];
                DataManager.Instance[clientId] = new(data) {inGameData = data.inGameData.AddPerk(chosenPerk)};
            
                OnPerkChosenClientRpc(perkChoiceIndex, data.SendRpcTo());
            }
        
            ResetChoices();
        }

        public void ChoosePerksForPlayersServer()
        {
            foreach (PlayerData data in Data.DataManager.Instance.GetValues())
            {
                OuterData.PlayingState state = data.outerData.playingState;
                if (state != OuterData.PlayingState.Playing) continue;
                PlayerAvailablePerks[data.ClientId] = DrawPerks(GameSettings.Instance.PerkChoices, data);
                OnPerkChoicesAvailableClientRpc(PlayerAvailablePerks[data.ClientId], data.SendRpcTo());
            }
        }
        public ushort[] DrawPerks(int amount, PlayerData data)
        {
            var owned = new HashSet<ushort>(data.inGameData.perksIndexArray);
            List<ushort> availablePerks = Enumerable.Range(0, PerkSelectionList.Instance.perks.Length)
                .Select(i => (ushort)i)
                .Where(i => !owned.Contains(i))
                .ToList();

            return GetDistinctRandomPerks(availablePerks.ToArray(), amount);
        }
    
        private ushort[] GetDistinctRandomPerks(ushort[] availablePerks, int amount)
        {
            amount = Mathf.Min(amount, availablePerks.Length);
            return availablePerks.OrderBy(_ => UnityEngine.Random.value).Take(amount).ToArray();
        }
    
        public void ResetChoices()
        {
            PlayerAvailablePerks.Clear();
            PlayerPerkChoice.Clear();
        }
    
    
        // client
        public void ChoosePerkClient(ushort perkIndex) => ClientChosePerkServerRpc(perkIndex, RpcParamsExt.Instance.SenderClientID(NetworkManager.LocalClientId));

        [Rpc(SendTo.Server)]
        public void ClientChosePerkServerRpc(ushort perkIndex, RpcParams @params)
        {
            PlayerPerkChoice[@params.Receive.SenderClientId] = perkIndex;
        }
    }
}