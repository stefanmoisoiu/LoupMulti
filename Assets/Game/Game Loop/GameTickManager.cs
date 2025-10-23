using System;
using System.Collections;
using System.Collections.Generic;
using Base_Scripts;
using Unity.Netcode;
using UnityEngine;

namespace Game.Game_Loop
{
    public class GameTickManager : NetworkBehaviour
    {
        public const ushort TICKRATE = 10;
        public static ushort CurrentTick { get; private set; }
        private Coroutine _coroutine;
    
        public static event Action OnTickServer;
        public static event Action OnTickClient;
    
        public void StartTickLoop()
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            CurrentTick = 0;
            ResetTickLoopClientRpc();
            _coroutine = StartCoroutine(TickLoop());
        
            NetcodeLogger.Instance.LogRpc("Tick loop started", NetcodeLogger.LogType.TickLoop, new []{NetcodeLogger.AddedEffects.Bold});
        }
        public void StopTickLoop() 
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = null;
            CurrentTick = 0;
            ResetTickLoopClientRpc();
        }

        [ClientRpc]
        private void ResetTickLoopClientRpc()
        {
            CurrentTick = 0;
        }
    
        private IEnumerator TickLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f / TICKRATE);
                TickServer();
            }
        }

        private void TickServer()
        {
            CurrentTick++;
            OnTickServer?.Invoke();
            TickClientRpc(CurrentTick);
        }
        [ClientRpc]
        private void TickClientRpc(ushort currentTick)
        {
            if (currentTick < CurrentTick)
            {
                Debug.LogError($"Received tick {currentTick} is less than current tick {CurrentTick}");
            }
            CurrentTick = currentTick;
            
            
            OnTickClient?.Invoke();
        }
    }
    
    public class ClientScheduledAction
    {
        public Action action;
        public int baseTickWaitDuration;
        public int timeLeft;
        public bool isScheduled;
        public bool loop = false;
        
        public ClientScheduledAction(Action action, int baseTickWaitDuration = 10, bool startImmediately = false, bool loop = false)
        {
            this.action = action;
            this.loop = loop;
            this.baseTickWaitDuration = baseTickWaitDuration;
            timeLeft = 0;
            if (startImmediately) Schedule();
        }

        public void Schedule(int tickWaitDuration = -1)
        {
            if (isScheduled) return;
            timeLeft = tickWaitDuration == -1 ? baseTickWaitDuration : tickWaitDuration;
            isScheduled = true;
            GameTickManager.OnTickClient += Tick;
        }
        public void Cancel()
        {
            if (!isScheduled) return;
            isScheduled = false;
            GameTickManager.OnTickClient -= Tick;
        }
        

        private void Tick()
        {
            if (!isScheduled) return;
            timeLeft--;
            if (timeLeft > 0) return;
            action?.Invoke();
            Cancel();
            if (loop) Schedule();
        }
    }
}
