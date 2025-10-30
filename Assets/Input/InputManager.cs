using System;
using UnityEngine;

namespace Input
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        private Controls _controls;

        // Booléens accessibles
        public static Vector2 Move => MoveController + MoveKeyboard;
        public static Vector2 Look => LookController + LookKeyboard;
        
        private static Vector2 MoveController { get; set; }
        private static Vector2 LookController { get; set; }
        
        private static Vector2 MoveKeyboard { get; set; }
        private static Vector2 LookKeyboard { get; set; }
        
        public static bool    Jump   { get; private set; }
        public static bool    Run    { get; private set; }
        
        public static bool Primary { get; private set; }
        public static bool Secondary { get; private set; }
        
        public static bool    Drill { get; private set; }
        public static bool    Slot1{ get; private set; }
        public static bool    Slot2{ get; private set; }
        public static bool    Slot3{ get; private set; }
        
        public static bool    Inventory  { get; private set; }
        
        // Booléens pour UI
        
        public static bool    Shop   { get; private set; }
        
        

        // Événements pour début / fin
        public static event Action OnJumpStarted;
        public static event Action OnJumpCanceled;
        public static event Action OnRunStarted;
        public static event Action OnRunCanceled;
        
        public static event Action OnPrimaryStarted;
        public static event Action OnPrimaryCanceled;
        public static event Action OnSecondaryStarted;
        public static event Action OnSecondaryCanceled;
        
        public static event Action<int> OnAbilityUse;
        public static event Action<int> OnSlotCanceled;
        
        public static event Action OnDrillUse;
        public static event Action OnDrillCanceled;
        public static event Action OnSlot1Use;
        public static event Action OnSlot1Canceled;
        public static event Action OnSlot2Use;
        public static event Action OnSlot2Canceled;
        public static event Action OnSlot3Use;
        public static event Action OnSlot3Canceled;
        public static event Action OnInventoryOpened;
        public static event Action OnInventoryClosed;
        
        public static event Action OnShopOpened;
        public static event Action OnShopClosed;

        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        
            _controls = new Controls();

            // Mouvements
            _controls.Gameplay.Move.performed  += ctx => MoveKeyboard = ctx.ReadValue<Vector2>();
            _controls.Gameplay.Look.performed  += ctx => LookKeyboard = ctx.ReadValue<Vector2>();
            
            _controls.Gameplay.MoveController.performed += ctx => MoveController = ctx.ReadValue<Vector2>();
            _controls.Gameplay.LookController.performed += ctx => LookController = ctx.ReadValue<Vector2>();

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
            
            // Drill
            _controls.Gameplay.Drill.performed += ctx =>
            {
                Drill = true;
                OnDrillUse?.Invoke();
            };
            _controls.Gameplay.Drill.canceled += ctx =>
            {
                Drill = false;
                OnDrillCanceled?.Invoke();
            };
            
            // Slot 1, 2, 3
            _controls.Gameplay.Slot1.performed += ctx =>
            {
                Slot1 = true;
                OnAbilityUse?.Invoke(0);
            };
            _controls.Gameplay.Slot1.canceled += ctx =>
            {
                Slot1 = false;
                OnSlotCanceled?.Invoke(0);
            };
            _controls.Gameplay.Slot2.performed += ctx =>
            {
                Slot2 = true;
                OnAbilityUse?.Invoke(1);
            };
            _controls.Gameplay.Slot2.canceled += ctx =>
            {
                Slot2 = false;
                OnSlotCanceled?.Invoke(1);
            };
            _controls.Gameplay.Slot3.performed += ctx =>
            {
                Slot3 = true;
                OnAbilityUse?.Invoke(2);
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
            _controls.UI.Inventory.performed += ctx =>
            {
                Inventory = true;
                OnInventoryOpened?.Invoke();
            };
            _controls.UI.Inventory.canceled += ctx =>
            {
                Inventory = false;
                OnInventoryClosed?.Invoke();
            };
            
            // Shop (UI)
            _controls.UI.ShopOpen.performed += ctx =>
            {
                Shop = true;
                OnShopOpened?.Invoke();
            };
            _controls.UI.ShopOpen.canceled += ctx =>
            {
                Shop = false;
                OnShopClosed?.Invoke();
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
            LookKeyboard = Vector2.zero;
            LookController = Vector2.zero;
            MoveKeyboard = Vector2.zero;
            MoveController = Vector2.zero;
            _controls.UI.Enable();
        }
    }
}
