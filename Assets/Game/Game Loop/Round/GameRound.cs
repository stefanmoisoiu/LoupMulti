using System.Collections;
using Unity.Netcode;

namespace Game.Game_Loop.Round
{
    public abstract class GameRound : NetworkBehaviour
    {
        public abstract IEnumerator Execute(GameManager gameManager, GameLoopEvents gameLoopEvents);
    }
}