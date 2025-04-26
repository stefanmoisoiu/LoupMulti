using System;

namespace Game.Data.Extensions
{
    public static class PlayerDataExt
    {
        public enum HealthSearhConditions
        {
            Alive,
            Dead,
            All
        }
        public static PlayerData[] Search(this PlayerDataManager playerDataManager, PlayerOuterData.PlayingState[] playingStates = null, HealthSearhConditions healthSearhCondition = HealthSearhConditions.All)
        {
            if (playingStates == null) playingStates = new[] { PlayerOuterData.PlayingState.Playing };
            return Array.FindAll(playerDataManager.GetValues(), player => Array.Exists(playingStates, state => player.OuterData.playingState == state)
                                                                          && (healthSearhCondition == HealthSearhConditions.All
                                                                              || (healthSearhCondition == HealthSearhConditions.Alive && player.InGameData.IsAlive())
                                                                              || (healthSearhCondition == HealthSearhConditions.Dead && !player.InGameData.IsAlive())));
        }
    
        public static PlayerData[] AlivePlayers(this PlayerDataManager playerDataManager) => playerDataManager.Search(new[] { PlayerOuterData.PlayingState.Playing }, HealthSearhConditions.Alive);
        public static int AlivePlayerCount(this PlayerDataManager playerDataManager) => playerDataManager.AlivePlayers().Length;
    
        public static int PlayingPlayersCount(this PlayerDataManager playerDataManager) => playerDataManager.Search(new[] { PlayerOuterData.PlayingState.Playing }).Length;
        public static int SpectatingPlayersCount(this PlayerDataManager playerDataManager) => playerDataManager.Search(new[] { PlayerOuterData.PlayingState.SpectatingGame }).Length;
    }
}