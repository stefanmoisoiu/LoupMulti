using System.Collections;
using Game.Game_Loop;
using Networking.Connection;
using Player.Networking;
using UnityEngine;

namespace Player.Movement
{
    public class FreezePlayer : PNetworkBehaviour
    {
        [SerializeField] private Component[] componentsToFreeze;
        [SerializeField] private float startWaitBuffer = 0.5f;
        
        protected override void StartOnlineOwner()
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
            NetcodeManager.OnEnterGame += UpdateFreeze;
            NetcodeManager.OnLeaveGame += UpdateFreeze;
            
            StartCoroutine(StartFreezeBuffer());
        }

        private IEnumerator StartFreezeBuffer()
        {
            SetFreeze(true);
            yield return new WaitForSeconds(startWaitBuffer);
            UpdateFreeze();
        }

        protected override void DisableAnyOwner()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            NetcodeManager.OnEnterGame -= UpdateFreeze;
            NetcodeManager.OnLeaveGame -= UpdateFreeze;
            
            SetFreeze(false);
        }

        private void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState newState)
        {
            UpdateFreeze();
        }

        private void UpdateFreeze()
        {
            SetFreeze(ShouldFreeze());
        }

        private bool ShouldFreeze() => GameManager.CurrentGameState != GameManager.GameState.Loading &&
                                      GameManager.CurrentGameState != GameManager.GameState.NotConnected &&
                                      NetcodeManager.LoadingGame &&
                                      !NetcodeManager.InGame;

        private void SetFreeze(bool freeze)
        {
            foreach (var component in componentsToFreeze)
            {
                if (component == null) throw new System.Exception("Component to freeze is null");
                switch (component)
                {
                    case MonoBehaviour mb:
                        mb.enabled = !freeze;
                        break;
                    case Collider col:
                        col.enabled = !freeze;
                        break;
                    case Rigidbody rb:
                        rb.isKinematic = freeze;
                        break;
                    default:
                        throw new System.Exception("Component type not supported for freezing");
                }
            }
        }
    }
}
