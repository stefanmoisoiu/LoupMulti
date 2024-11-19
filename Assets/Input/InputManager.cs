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
            Destroy(this);
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
    }
}
