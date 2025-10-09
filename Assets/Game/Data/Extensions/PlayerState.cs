using Game.Common;
using Unity.Netcode;

namespace Game.Data.Extensions
{
    public class PlayerState : NetworkBehaviour
    {
        public void SetNotAssignedPlayersToPlayingState()
        {
            foreach (PlayerData playerData in DataManager.Instance.GetValues())
            {
                if (playerData.outerData.playingState != OuterData.PlayingState.NotAssigned) continue;
                DataManager.Instance[playerData.clientId] = new(playerData)
                    { outerData = playerData.outerData.SetState(OuterData.PlayingState.Playing) };
            }
        }

        public void SetAllPlayersToNotAssigned()
        {
            foreach (PlayerData playerData in DataManager.Instance.GetValues())
            {
                if (playerData.outerData.playingState == OuterData.PlayingState.NotAssigned) continue;
                DataManager.Instance[playerData.clientId] = new(playerData)
                    { outerData = playerData.outerData.SetState(OuterData.PlayingState.NotAssigned) };
            }
        }
    }
}