using System;
using Game.Common;

namespace Game.Data.Extensions
{
    public static class DataExt
    {
        public enum HealthSearchConditions
        {
            Alive,
            Dead,
            All
        }
        public static PlayerData[] Search(this DataManager dataManager, OuterData.PlayingState[] playingStates = null, HealthSearchConditions healthSearchCondition = HealthSearchConditions.All)
        {
            playingStates ??= new[] { OuterData.PlayingState.Playing };
            return Array.FindAll(dataManager.GetValues(), player => Array.Exists(playingStates, state => player.outerData.playingState == state)
                                                                    && (healthSearchCondition == HealthSearchConditions.All
                                                                        || (healthSearchCondition == HealthSearchConditions.Alive && player.inGameData.IsAlive())
                                                                        || (healthSearchCondition == HealthSearchConditions.Dead && !player.inGameData.IsAlive())));
        }
        public static int SpectatingPlayersCount(this DataManager dataManager) => dataManager.Search(new[] { OuterData.PlayingState.SpectatingGame }).Length;
    }
}