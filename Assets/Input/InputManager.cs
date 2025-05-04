using System;
using UnityEngine;

namespace Input
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        private Controls _controls;

        // Booléens accessibles
        public static Vector2 Move   { get; private set; }
        public static Vector2 Look   { get; private set; }
        public static bool    Jump   { get; private set; }
        public static bool    Run    { get; private set; }
        
        public static bool Primary { get; private set; }
        public static bool Secondary { get; private set; }
        
        public static bool    Slot1{ get; private set; }
        public static bool    Slot2{ get; private set; }
        public static bool    Slot3{ get; private set; }
        
        public static bool    Score  { get; private set; }
        
        

        // Événements pour début / fin
        public static event Action OnJumpStarted;
        public static event Action OnJumpCanceled;
        public static event Action OnRunStarted;
        public static event Action OnRunCanceled;
        
        public static event Action OnPrimaryStarted;
        public static event Action OnPrimaryCanceled;
        public static event Action OnSecondaryStarted;
        public static event Action OnSecondaryCanceled;
        
        public static event Action<int> OnSlotUse;
        public static event Action<int> OnSlotCanceled;
        
        public static event Action OnSlot1Use;
        public static event Action OnSlot1Canceled;
        public static event Action OnSlot2Use;
        public static event Action OnSlot2Canceled;
        public static event Action OnSlot3Use;
        public static event Action OnSlot3Canceled;
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
            
            // Primary
            _controls.Gameplay.Primary.performed += ctx =>
            {
                Primary = true;
                OnPrimaryStarted?.Invoke();
            };
            _controls.Gameplay.Primary.canceled += ctx =>
            {
                Primary = false;
                OnPrimaryCanceled?.Invoke();
            };
            
            // Secondary
            _controls.Gameplay.Secondary.performed += ctx =>
            {
                Secondary = true;
                OnSecondaryStarted?.Invoke();
            };
            _controls.Gameplay.Secondary.canceled += ctx =>
            {
                Secondary = false;
                OnSecondaryCanceled?.Invoke();
            };
            
            // Slot 1, 2, 3
            _controls.Gameplay.Slot1.performed += ctx =>
            {
                Slot1 = true;
                OnSlotUse?.Invoke(0);
            };
            _controls.Gameplay.Slot1.canceled += ctx =>
            {
                Slot1 = false;
                OnSlotCanceled?.Invoke(0);
            };
            _controls.Gameplay.Slot2.performed += ctx =>
            {
                Slot2 = true;
                OnSlotUse?.Invoke(1);
            };
            _controls.Gameplay.Slot2.canceled += ctx =>
            {
                Slot2 = false;
                OnSlotCanceled?.Invoke(1);
            };
            _controls.Gameplay.Slot3.performed += ctx =>
            {
                Slot3 = true;
                OnSlotUse?.Invoke(2);
            };
            _controls.Gameplay.Slot3.canceled += ctx =>
            {
                Slot3 = false;
                OnSlotCanceled?.Invoke(2);
            };
            
            // Slot 1
            _controls.Gameplay.Slot1.performed += ctx =>
            {
                Slot1 = true;
                OnSlot1Use?.Invoke();
            };
            _controls.Gameplay.Slot1.canceled += ctx =>
            {
                Slot1 = false;
                OnSlot1Canceled?.Invoke();
            };
            // Slot 2
            _controls.Gameplay.Slot2.performed += ctx =>
            {
                Slot2 = true;
                OnSlot2Use?.Invoke();
            };
            _controls.Gameplay.Slot2.canceled += ctx =>
            {
                Slot2 = false;
                OnSlot2Canceled?.Invoke();
            };
            // Slot 3
            _controls.Gameplay.Slot3.performed += ctx =>
            {
                Slot3 = true;
                OnSlot3Use?.Invoke();
            };
            _controls.Gameplay.Slot3.canceled += ctx =>
            {
                Slot3 = false;
                OnSlot3Canceled?.Invoke();
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
}
