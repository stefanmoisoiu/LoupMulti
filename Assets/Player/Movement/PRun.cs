using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PRun : PNetworkBehaviour
{

    [SerializeField] private float loseRate = 50;
    
    public bool Running { get; private set; }
    
    public Action OnRun;
    public Action OnStopRun;

    private float runInputThreshold = 0.8f;

    [SerializeField] private PGrounded grounded;
    [SerializeField] private PStamina stamina;

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
        if (stamina.Stamina <= 0 ||
            !grounded.FullyGrounded() ||
            InputManager.Move.magnitude < runInputThreshold) StopRun();
    }

    private void StartRun()
    {
        if (InputManager.Move.magnitude < runInputThreshold) return;
        Running = true;
        OnRun?.Invoke();
    }
    private void StopRun()
    {
        Running = false;
        OnStopRun?.Invoke();
    }
}
