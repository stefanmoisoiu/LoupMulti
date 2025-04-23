using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private Controls _controls;

    // Booléens accessibles
    public static Vector2 Move   { get; private set; }
    public static Vector2 Look   { get; private set; }
    public static bool    Jump   { get; private set; }
    public static bool    Run    { get; private set; }
    public static bool    Action1{ get; private set; }
    public static bool    Action2{ get; private set; }
    public static bool    Action3{ get; private set; }
    public static bool    Score  { get; private set; }

    // Événements pour début / fin
    public static event Action OnJumpStarted;
    public static event Action OnJumpCanceled;
    public static event Action OnRunStarted;
    public static event Action OnRunCanceled;
    public static event Action OnAction1Started;
    public static event Action OnAction1Canceled;
    public static event Action OnAction2Started;
    public static event Action OnAction2Canceled;
    public static event Action OnAction3Started;
    public static event Action OnAction3Canceled;
    public static event Action OnScoreboardOpened;
    public static event Action OnScoreboardClosed;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        _controls = new Controls();

        // Mouvements
        _controls.Gameplay.Move.performed  += ctx => Move = ctx.ReadValue<Vector2>();
        _controls.Gameplay.Look.performed  += ctx => Look = ctx.ReadValue<Vector2>();

        // Jump
        _controls.Gameplay.Jump.performed += ctx =>
        {
            Jump = true;
            OnJumpStarted?.Invoke();
        };
        _controls.Gameplay.Jump.canceled += ctx =>
        {
            Jump = false;
            OnJumpCanceled?.Invoke();
        };

        // Run
        _controls.Gameplay.Run.performed += ctx =>
        {
            Run = true;
            OnRunStarted?.Invoke();
        };
        _controls.Gameplay.Run.canceled += ctx =>
        {
            Run = false;
            OnRunCanceled?.Invoke();
        };

        // Action1
        _controls.Gameplay.Action1.performed += ctx =>
        {
            Action1 = true;
            OnAction1Started?.Invoke();
        };
        _controls.Gameplay.Action1.canceled += ctx =>
        {
            Action1 = false;
            OnAction1Canceled?.Invoke();
        };

        // Action2
        _controls.Gameplay.Action2.performed += ctx =>
        {
            Action2 = true;
            OnAction2Started?.Invoke();
        };
        _controls.Gameplay.Action2.canceled += ctx =>
        {
            Action2 = false;
            OnAction2Canceled?.Invoke();
        };

        // Action3
        _controls.Gameplay.Action3.performed += ctx =>
        {
            Action3 = true;
            OnAction3Started?.Invoke();
        };
        _controls.Gameplay.Action3.canceled += ctx =>
        {
            Action3 = false;
            OnAction3Canceled?.Invoke();
        };

        // Scoreboard (UI)
        _controls.UI.Scoreboard.performed += ctx =>
        {
            Score = true;
            OnScoreboardOpened?.Invoke();
        };
        _controls.UI.Scoreboard.canceled += ctx =>
        {
            Score = false;
            OnScoreboardClosed?.Invoke();
        };

        _controls.Enable();
    }

    private void OnEnable()  => _controls?.Enable();
    private void OnDisable() => _controls?.Disable();

    /// <summary>
    /// Bascule sur la map de gameplay, désactive la map UI.
    /// </summary>
    public void EnableGameplay()
    {
        _controls.UI.Disable();
        _controls.Gameplay.Enable();
    }

    /// <summary>
    /// Bascule sur la map UI (ex: menu), désactive la map gameplay.
    /// </summary>
    public void EnableUI()
    {
        _controls.Gameplay.Disable();
        Look = Vector2.zero;
        Move = Vector2.zero;
        _controls.UI.Enable();
    }
}
