using System;
using Game.Common;
using Game.Data;
using Input;
using Player.Networking;
using UnityEngine;

namespace Player.Movement
{
    public class Run : PNetworkBehaviour
    {

        [SerializeField] private float loseRate = 50;
    
        public bool Running { get; private set; }
    
        public Action OnRun;
        public Action OnStopRun;

        private float runInputThreshold = 0.8f;

        [SerializeField] private Grounded grounded;
        [SerializeField] private Stamina.Stamina stamina;

        protected override void StartAnyOwner()
        {
            InputManager.OnRunStarted += StartRun;
            InputManager.OnRunCanceled += StopRun;
        }

        protected override void DisableAnyOwner()
        {
            InputManager.OnRunStarted -= StartRun;
            InputManager.OnRunCanceled -= StopRun;
        }
    
        protected override void UpdateAnyOwner()
        {
            if (!Running) return;
        
            stamina.DecreaseStamina(loseRate * Time.deltaTime);
            if (stamina.StaminaValue <= 0 ||
                !grounded.FullyGrounded() ||
                InputManager.Move.magnitude < runInputThreshold) StopRun();
        }

        private void StartRun()
        {
            if (DataManager.Instance[NetworkManager.LocalClientId].outerData.playingState ==
                OuterData.PlayingState.SpectatingGame) return;
            if (Running) return;
            if (InputManager.Move.magnitude < runInputThreshold) return;
            Running = true;
            OnRun?.Invoke();
        }
        private void StopRun()
        {
            if (!Running) return;
            Running = false;
            OnStopRun?.Invoke();
        }
    }
}
