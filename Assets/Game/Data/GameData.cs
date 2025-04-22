using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Classe GameData utilisant un NetworkDictionaryVariable pour synchroniser les PlayerData individuellement.
/// </summary>
public class GameData : NetworkBehaviour
{
    // Maintenant on peut faire directement players[clientId] = pd;
    [SerializeField] private PlayerDataManager playerDataManager;
    public PlayerDataManager PlayerDataManager => playerDataManager;

    /// <summary>
    /// Met à jour l'état extérieur d'un joueur et ne réplique que cette entrée.
    /// </summary>
    public void SetNotAssignedPlayersToPlayingState()
    {
        if (!IsServer)
        {
            Debug.LogError("SetNotAssignedPlayersToPlayingState() can only be called on the server.");
            return;
        }
        foreach (ulong clientId in playerDataManager.GetKeys())
        {
            if (playerDataManager[clientId].OuterData.playingState != PlayerOuterData.PlayingState.NotAssigned) continue;
            playerDataManager[clientId] = new(playerDataManager[clientId])
                { OuterData = playerDataManager[clientId].OuterData.SetState(PlayerOuterData.PlayingState.Playing) };
        }
    }
}