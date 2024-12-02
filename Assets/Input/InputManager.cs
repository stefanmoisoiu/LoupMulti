using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }
    private Controls _controls;
    
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    
    public bool Jumping { get; private set; }
    public Action OnJump { get; set; }
    public Action OnStopJump { get; set; }
    
    public bool Running { get; private set; }
    public Action OnRun { get; set; }
    public Action OnStopRun { get; set; }
    
    public bool Action1Pressed { get; set; }
    public Action OnAction1 { get; set; }
    public Action OnStopAction1 { get; set; }
    
    public bool Action2Pressed { get; set; }
    public Action OnAction2 { get; set; }
    public Action OnStopAction2 { get; set; }
    
    public bool Action3Pressed { get; set; }
    public Action OnAction3 { get; set; }
    public Action OnStopAction3 { get; set; }
    
    public bool ScoreboardPressed { get; set; }
    public Action OnScoreboard { get; set; }
    public Action OnStopScoreboard { get; set; }
    
    public enum AbilityInput
    {
        Action1,
        Action2,
        Action3,
        None
    }

    public enum ActionType
    {
        Start,
        Stop
    }

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CreateControls();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateControls()
    {
        _controls = new Controls();
        _controls.Enable();
        
        _controls.BaseMovement.Horizontal.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        _controls.BaseMovement.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        
        
        _controls.BaseMovement.Jump.performed += ctx => { OnJump?.Invoke(); Jumping = true; };
        _controls.BaseMovement.Jump.canceled += ctx => { OnStopJump?.Invoke(); Jumping = false; };
        
        _controls.BaseMovement.Run.performed += ctx => { OnRun?.Invoke(); Running = true; };
        _controls.BaseMovement.Run.canceled += ctx => { OnStopRun?.Invoke(); Running = false; };
        
        
        _controls.BaseMovement.Action1.performed += ctx => { OnAction1?.Invoke(); Action1Pressed = true; };
        _controls.BaseMovement.Action1.canceled += ctx => { OnStopAction1?.Invoke(); Action1Pressed = false; };
        
        _controls.BaseMovement.Action2.performed += ctx => { OnAction2?.Invoke(); Action2Pressed = true; };
        _controls.BaseMovement.Action2.canceled += ctx => { OnStopAction2?.Invoke(); Action2Pressed = false; };
        
        _controls.BaseMovement.Action3.performed += ctx => { OnAction3?.Invoke(); Action3Pressed = true; };
        _controls.BaseMovement.Action3.canceled += ctx => { OnStopAction3?.Invoke(); Action3Pressed = false; };
        
        _controls.UI.Scoreboard.performed += ctx => { OnScoreboard?.Invoke(); ScoreboardPressed = true; };
        _controls.UI.Scoreboard.canceled += ctx => { OnStopScoreboard?.Invoke(); ScoreboardPressed = false; };
    }
    
    public void AddAbilityInputListener(AbilityInput abilityInput, ActionType type, Action action)
    {
        switch (abilityInput)
        {
            case AbilityInput.Action1:
                switch (type)
                {
                    case ActionType.Start:
                        OnAction1 += action;
                        break;
                    case ActionType.Stop:
                        OnStopAction1 += action;
                        break;
                }
                break;
            case AbilityInput.Action2:
                switch (type)
                {
                    case ActionType.Start:
                        OnAction2 += action;
                        break;
                    case ActionType.Stop:
                        OnStopAction2 += action;
                        break;
                }
                break;
            case AbilityInput.Action3:
                switch (type)
                {
                    case ActionType.Start:
                        OnAction3 += action;
                        break;
                    case ActionType.Stop:
                        OnStopAction3 += action;
                        break;
                }
                break;
        }
    }
    public void RemoveAbilityInputListener(AbilityInput abilityInput, ActionType type, Action action)
    {
        switch (abilityInput)
        {
            case AbilityInput.Action1:
                switch (type)
                {
                    case ActionType.Start:
                        OnAction1 -= action;
                        break;
                    case ActionType.Stop:
                        OnStopAction1 -= action;
                        break;
                }
                break;
            case AbilityInput.Action2:
                switch (type)
                {
                    case ActionType.Start:
                        OnAction2 -= action;
                        break;
                    case ActionType.Stop:
                        OnStopAction2 -= action;
                        break;
                }
                break;
            case AbilityInput.Action3:
                switch (type)
                {
                    case ActionType.Start:
                        OnAction3 -= action;
                        break;
                    case ActionType.Stop:
                        OnStopAction3 -= action;
                        break;
                }
                break;
        }
    }
}
