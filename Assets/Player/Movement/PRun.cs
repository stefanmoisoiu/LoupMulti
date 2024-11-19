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
        InputManager.instance.OnRun += StartRun;
        InputManager.instance.OnStopRun += StopRun;
    }

    protected override void DisableAnyOwner()
    {
        InputManager.instance.OnRun -= StartRun;
        InputManager.instance.OnStopRun -= StopRun;
    }
    
    private void Update()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        if (Running)
        {
            stamina.DecreaseStamina(loseRate * Time.deltaTime);
            if (stamina.Stamina <= 0 ||
                !grounded.FullyGrounded() ||
                InputManager.instance.MoveInput.magnitude < runInputThreshold) StopRun();
        }
    }

    private void StartRun()
    {
        if (InputManager.instance.MoveInput.magnitude < runInputThreshold) return;
        Running = true;
        OnRun?.Invoke();
    }
    private void StopRun()
    {
        Running = false;
        OnStopRun?.Invoke();
    }
}
