using UnityEngine;
using HockeyGame.Input;

namespace HockeyGame.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PlayerConfig _config;

        [Header("Visual")]
        [SerializeField] private Renderer _playerRenderer;

        private Rigidbody _rigidbody;
        private IInputProvider _inputProvider;
        private PlayerStateMachine _stateMachine;

        private Vector3 _currentVelocity;
        private float _lastShotTime;
        private bool _isHumanControlled;

        public PlayerConfig Config => _config;
        public PlayerStateMachine StateMachine => _stateMachine;
        public bool IsHumanControlled => _isHumanControlled;
        public bool HasPuck { get; set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _stateMachine = new PlayerStateMachine();
        }

        public void Initialize(IInputProvider inputProvider, PlayerConfig config, bool isHuman = false)
        {
            _inputProvider = inputProvider;
            _config = config;
            _isHumanControlled = isHuman;

            ConfigureRigidbody();
            SubscribeToInput();
        }

        private void ConfigureRigidbody()
        {
            _rigidbody.mass = _config.mass;
            _rigidbody.linearDamping = _config.drag;

            // 2.5D constraints: lock Y position and X/Z rotation
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY |
                                      RigidbodyConstraints.FreezeRotationX |
                                      RigidbodyConstraints.FreezeRotationZ;

            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            if (_config.physicsMaterial != null)
            {
                var collider = GetComponent<Collider>();
                if (collider != null)
                {
                    collider.material = _config.physicsMaterial;
                }
            }
        }

        private void SubscribeToInput()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnShootPerformed += HandleShoot;
                _inputProvider.OnPassPerformed += HandlePass;
            }
        }

        private void Update()
        {
            // Poll input every frame (handles fallback keyboard input)
            if (_inputProvider is InputReader inputReader)
            {
                inputReader.PollInput();
            }
        }

        private void FixedUpdate()
        {
            if (_inputProvider == null)
            {
                // Only log once to avoid spam
                if (_isHumanControlled && Time.frameCount % 300 == 0)
                {
                    Debug.LogWarning($"[{name}] No input provider assigned!");
                }
                return;
            }

            HandleMovement();
            UpdateState();
        }

        private void LateUpdate()
        {
            // Reset single-frame inputs
            if (_inputProvider is InputReader inputReader)
            {
                inputReader.ResetFrameInputs();
            }
        }

        private void HandleMovement()
        {
            Vector2 input = _inputProvider.MoveInput;

            // Convert 2D input to 3D movement on X-Z plane
            Vector3 targetDirection = new Vector3(input.x, 0f, input.y).normalized;

            float speedMultiplier = _inputProvider.SprintHeld ? _config.sprintMultiplier : 1f;
            Vector3 targetVelocity = targetDirection * _config.moveSpeed * speedMultiplier;

            // Smooth acceleration/deceleration
            float accel = targetVelocity.magnitude > 0.1f ? _config.acceleration : _config.deceleration;
            _currentVelocity = Vector3.MoveTowards(_currentVelocity, targetVelocity, accel * Time.fixedDeltaTime);

            // Apply movement (preserve Y velocity for any physics interactions)
            _rigidbody.linearVelocity = new Vector3(_currentVelocity.x, _rigidbody.linearVelocity.y, _currentVelocity.z);

            // Rotate to face movement direction
            if (targetDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _config.rotationSpeed * Time.fixedDeltaTime
                );
            }
        }

        private void UpdateState()
        {
            if (!_stateMachine.CanTransitionTo(PlayerState.Idle)) return;

            if (_inputProvider.MoveInput.magnitude > 0.1f)
            {
                _stateMachine.ChangeState(_inputProvider.SprintHeld ? PlayerState.Sprinting : PlayerState.Skating);
            }
            else
            {
                _stateMachine.ChangeState(PlayerState.Idle);
            }
        }

        private void HandleShoot()
        {
            if (Time.time - _lastShotTime < _config.shotCooldown) return;
            if (!HasPuck) return;

            _lastShotTime = Time.time;
            _stateMachine.ChangeState(PlayerState.Shooting);

            // Shooting will be handled by PuckController
            Debug.Log($"{_config.playerName} shoots!");
        }

        private void HandlePass()
        {
            if (!HasPuck) return;

            _stateMachine.ChangeState(PlayerState.Passing);

            // Passing will be handled by PuckController
            Debug.Log($"{_config.playerName} passes!");
        }

        public void SetTeamColor(Color color)
        {
            if (_playerRenderer != null)
            {
                var propBlock = new MaterialPropertyBlock();
                _playerRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_BaseColor", color);
                _playerRenderer.SetPropertyBlock(propBlock);
            }
        }

        private void OnDestroy()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnShootPerformed -= HandleShoot;
                _inputProvider.OnPassPerformed -= HandlePass;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw movement direction
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            // Draw velocity
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, _currentVelocity.normalized * 2f);
            }
        }
    }
}
