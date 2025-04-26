using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Data.Extensions
{
    public static class DataExt
    {
        public enum HealthSearhConditions
        {
            Alive,
            Dead,
            All
        }
        public static PlayerData[] Search(this DataManager dataManager, OuterData.PlayingState[] playingStates = null, HealthSearhConditions healthSearhCondition = HealthSearhConditions.All)
        {
            if (playingStates == null) playingStates = new[] { OuterData.PlayingState.Playing };
            return Array.FindAll(dataManager.GetValues(), player => Array.Exists(playingStates, state => player.outerData.playingState == state)
                                                                          && (healthSearhCondition == HealthSearhConditions.All
                                                                              || (healthSearhCondition == HealthSearhConditions.Alive && player.inGameData.IsAlive())
                                                                              || (healthSearhCondition == HealthSearhConditions.Dead && !player.inGameData.IsAlive())));
        }
    
        public static PlayerData[] AlivePlayers(this DataManager dataManager) => dataManager.Search(new[] { OuterData.PlayingState.Playing }, HealthSearhConditions.Alive);
        public static int AlivePlayerCount(this DataManager dataManager) => dataManager.AlivePlayers().Length;
    
        public static int PlayingPlayersCount(this DataManager dataManager) => dataManager.Search(new[] { OuterData.PlayingState.Playing }).Length;
        public static int SpectatingPlayersCount(this DataManager dataManager) => dataManager.Search(new[] { OuterData.PlayingState.SpectatingGame }).Length;
        
        
        public static void SetNotAssignedPlayersToPlayingState(this DataManager dataManager)
        {
            foreach (PlayerData playerData in dataManager.GetValues())
            {
                if (playerData.outerData.playingState != OuterData.PlayingState.NotAssigned) continue;
                dataManager[playerData.ClientId] = new(playerData)
                    { outerData = playerData.outerData.SetState(OuterData.PlayingState.Playing) };
            }
        }
    }
}