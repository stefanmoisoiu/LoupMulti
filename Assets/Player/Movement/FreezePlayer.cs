using System.Collections;
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Networking.Connection;
using Player.Networking;
using UnityEngine;

namespace Player.Movement
{
    public class FreezePlayer : PNetworkBehaviour
    {
        [SerializeField] private Component[] componentsToFreeze;
        [SerializeField] private float startWaitBuffer = 0.5f;
        
        private bool _frozen;
        public bool Frozen => _frozen;

        private bool _startFreezeBuffer;
        private bool _gameStateFreeze;
        private bool _gameRoundFreeze;
        
        protected override void StartOnlineOwner()
        {
            GameManager.OnGameStateChangedAll += OnGameStateChangedAll;
            GameLoopEvents.OnRoundStateChangedAll += OnRoundStateChangedAll;
            StartCoroutine(StartFreezeBuffer());

            NetcodeManager.OnEnterGame += OnEnteredGame;
            NetcodeManager.OnLeaveGame += OnLeftGame;
        }



        private IEnumerator StartFreezeBuffer()
        {
            _startFreezeBuffer = true;
            SetFreeze(true);
            yield return new WaitForSeconds(startWaitBuffer);
            _startFreezeBuffer = false;
        }

        protected override void DisableAnyOwner()
        {
            GameManager.OnGameStateChangedAll -= OnGameStateChangedAll;
            
            SetFreeze(false);
        }

        private void OnEnteredGame()
        {
            
        }

        private void OnLeftGame()
        {
            _gameStateFreeze = false;
            _gameRoundFreeze = false;
            _startFreezeBuffer = false;
        }

        private void OnGameStateChangedAll(GameManager.GameState previousState, GameManager.GameState newState)
        {
            _gameStateFreeze = newState == GameManager.GameState.Loading;
        }
        private void OnRoundStateChangedAll(GameRoundState newRoundState, float serverTime)
        {
            _gameRoundFreeze = newRoundState == GameRoundState.Countdown;
        }

        protected override void UpdateAnyOwner()
        {
            UpdateFreeze();
        }

        private void UpdateFreeze() => SetFreeze(_gameStateFreeze || _gameRoundFreeze || _startFreezeBuffer);
        private void SetFreeze(bool freeze)
        {
            if (_frozen == freeze) return;
            _frozen = freeze;
            
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
