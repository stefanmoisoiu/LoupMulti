using System;
using System.Collections;
using Base_Scripts;
using Unity.Netcode;
using UnityEngine;

namespace Game.Game_Loop
{
    public class GameTickManager : NetworkBehaviour
    {
        public static readonly int TICKRATE = 10;
        public static ushort CurrentTick { get; private set; }
        private Coroutine _coroutine;
    
        public static event Action OnTickServer;
        public static event Action OnTickClient;
    
        public void StartTickLoop()
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            CurrentTick = 0;
            _coroutine = StartCoroutine(TickLoop());
        
            NetcodeLogger.Instance.LogRpc("Tick loop started", NetcodeLogger.LogType.TickLoop, new []{NetcodeLogger.AddedEffects.Bold});
        }
        public void StopTickLoop() 
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = null;
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
            CurrentTick = currentTick;
            OnTickClient?.Invoke();
        }
    }
}
