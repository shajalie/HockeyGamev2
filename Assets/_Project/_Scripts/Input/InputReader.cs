using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HockeyGame.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Hockey/Input Reader")]
    public class InputReader : ScriptableObject, IInputProvider
    {
        [Header("Input Actions Asset")]
        [SerializeField] private InputActionAsset _inputActionsAsset;

        private InputActionMap _playerActionMap;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _interactAction;
        private InputAction _sprintAction;
        private InputAction _nextAction;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool ShootPressed { get; private set; }
        public bool ShootHeld { get; private set; }
        public bool PassPressed { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool SwitchPlayerPressed { get; private set; }

        public event Action OnShootPerformed;
        public event Action OnPassPerformed;
        public event Action OnSwitchPlayerPerformed;

        private bool _isInitialized = false;
        private bool _useFallback = false;

        public bool IsInitialized => _isInitialized;

        private void OnEnable()
        {
            Initialize();
        }

        /// <summary>
        /// Explicitly initialize the InputReader. Can be called multiple times safely.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            // Try to load the InputActions asset if not assigned
            if (_inputActionsAsset == null)
            {
                _inputActionsAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");

                // If still null, try to find it in the project
                if (_inputActionsAsset == null)
                {
#if UNITY_EDITOR
                    // In editor, try to find the asset in the AssetDatabase
                    var guids = UnityEditor.AssetDatabase.FindAssets("t:InputActionAsset");
                    if (guids.Length > 0)
                    {
                        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        _inputActionsAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                        Debug.Log($"[InputReader] Found InputActionAsset at: {path}");
                    }
#endif
                }
            }

            if (_inputActionsAsset == null)
            {
                Debug.LogWarning("[InputReader] No InputActionAsset found. Using fallback keyboard input.");
                _useFallback = true;
                _isInitialized = true;
                return;
            }

            SetupActions();
            _isInitialized = true;
        }

        private void SetupActions()
        {
            _playerActionMap = _inputActionsAsset.FindActionMap("Player");

            if (_playerActionMap == null)
            {
                Debug.LogError("[InputReader] Could not find 'Player' action map in InputActionAsset");
                return;
            }

            // Get action references
            _moveAction = _playerActionMap.FindAction("Move");
            _lookAction = _playerActionMap.FindAction("Look");
            _attackAction = _playerActionMap.FindAction("Attack");
            _interactAction = _playerActionMap.FindAction("Interact");
            _sprintAction = _playerActionMap.FindAction("Sprint");
            _nextAction = _playerActionMap.FindAction("Next");

            // Subscribe to actions
            if (_moveAction != null)
            {
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }

            if (_lookAction != null)
            {
                _lookAction.performed += OnLookPerformed;
                _lookAction.canceled += OnLookCanceled;
            }

            if (_attackAction != null)
            {
                _attackAction.started += OnAttackStarted;
                _attackAction.performed += OnAttackPerformed;
                _attackAction.canceled += OnAttackCanceled;
            }

            if (_interactAction != null)
            {
                _interactAction.started += OnInteractStarted;
            }

            if (_sprintAction != null)
            {
                _sprintAction.performed += OnSprintPerformed;
                _sprintAction.canceled += OnSprintCanceled;
            }

            if (_nextAction != null)
            {
                _nextAction.started += OnNextStarted;
            }

            // Enable the action map
            _playerActionMap.Enable();

            Debug.Log("[InputReader] Input actions initialized successfully");
        }

        private void OnDisable()
        {
            UnsubscribeActions();
            _playerActionMap?.Disable();
        }

        private void UnsubscribeActions()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }

            if (_lookAction != null)
            {
                _lookAction.performed -= OnLookPerformed;
                _lookAction.canceled -= OnLookCanceled;
            }

            if (_attackAction != null)
            {
                _attackAction.started -= OnAttackStarted;
                _attackAction.performed -= OnAttackPerformed;
                _attackAction.canceled -= OnAttackCanceled;
            }

            if (_interactAction != null)
            {
                _interactAction.started -= OnInteractStarted;
            }

            if (_sprintAction != null)
            {
                _sprintAction.performed -= OnSprintPerformed;
                _sprintAction.canceled -= OnSprintCanceled;
            }

            if (_nextAction != null)
            {
                _nextAction.started -= OnNextStarted;
            }
        }

        // Move callbacks
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            MoveInput = Vector2.zero;
        }

        // Look callbacks
        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            LookInput = Vector2.zero;
        }

        // Attack (Shoot) callbacks
        private void OnAttackStarted(InputAction.CallbackContext context)
        {
            ShootPressed = true;
            OnShootPerformed?.Invoke();
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            ShootHeld = true;
        }

        private void OnAttackCanceled(InputAction.CallbackContext context)
        {
            ShootHeld = false;
        }

        // Interact (Pass) callbacks
        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            PassPressed = true;
            OnPassPerformed?.Invoke();
        }

        // Sprint callbacks
        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            SprintHeld = true;
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            SprintHeld = false;
        }

        // Next (Switch Player) callbacks
        private void OnNextStarted(InputAction.CallbackContext context)
        {
            SwitchPlayerPressed = true;
            OnSwitchPlayerPerformed?.Invoke();
        }

        public void ResetFrameInputs()
        {
            ShootPressed = false;
            PassPressed = false;
            SwitchPlayerPressed = false;
        }

        /// <summary>
        /// Call this every frame to ensure input is read (handles fallback mode)
        /// </summary>
        public void PollInput()
        {
            if (_useFallback || _playerActionMap == null || !_playerActionMap.enabled)
            {
                UpdateFallbackInput();
            }
        }

        // Fallback method to manually read input if InputActions aren't working
        public void UpdateFallbackInput()
        {
            // Fallback to direct keyboard reading
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // WASD movement
            Vector2 move = Vector2.zero;
            if (keyboard.wKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed) move.y -= 1f;
            if (keyboard.aKey.isPressed) move.x -= 1f;
            if (keyboard.dKey.isPressed) move.x += 1f;
            MoveInput = move.normalized;

            // Sprint
            SprintHeld = keyboard.leftShiftKey.isPressed;

            // Shoot
            if (keyboard.enterKey.wasPressedThisFrame || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                ShootPressed = true;
                OnShootPerformed?.Invoke();
            }

            // Pass
            if (keyboard.eKey.wasPressedThisFrame)
            {
                PassPressed = true;
                OnPassPerformed?.Invoke();
            }
        }
    }
}
