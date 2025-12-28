using UnityEngine;
using HockeyGame.Input;
using HockeyGame.Gameplay.Player;

namespace HockeyGame.Gameplay.AI
{
    public class AgentBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;

        [Header("AI Settings")]
        [SerializeField] private float _reactionTime = 0.2f;
        [SerializeField] private float _pursuitSpeed = 0.8f;
        [SerializeField] private float _positioningWeight = 0.5f;

        private AIInputProvider _inputProvider;
        private Transform _puckTransform;
        private Vector3 _homePosition;
        private float _nextDecisionTime;

        public void Initialize(PlayerController controller, Vector3 homePosition)
        {
            _playerController = controller;
            _homePosition = homePosition;
            _inputProvider = new AIInputProvider();

            // Initialize the controller with AI input
            if (_playerController != null && _playerController.Config != null)
            {
                _playerController.Initialize(_inputProvider, _playerController.Config, false);
            }
        }

        public void SetPuckReference(Transform puck)
        {
            _puckTransform = puck;
        }

        private void Update()
        {
            if (_playerController == null || _inputProvider == null) return;

            if (Time.time >= _nextDecisionTime)
            {
                MakeDecision();
                _nextDecisionTime = Time.time + _reactionTime;
            }
        }

        private void MakeDecision()
        {
            _inputProvider.Reset();

            // Simple positioning AI - move toward home position with puck awareness
            Vector3 targetPosition = _homePosition;

            if (_puckTransform != null)
            {
                // Blend between home position and puck position
                targetPosition = Vector3.Lerp(_homePosition, _puckTransform.position, _positioningWeight);
            }

            Vector3 directionToTarget = targetPosition - transform.position;
            directionToTarget.y = 0;

            if (directionToTarget.magnitude > 1f)
            {
                Vector2 moveInput = new Vector2(directionToTarget.x, directionToTarget.z).normalized;
                moveInput *= _pursuitSpeed;
                _inputProvider.MoveInput = moveInput;
            }
            else
            {
                _inputProvider.MoveInput = Vector2.zero;
            }
        }

        public void SetHomePosition(Vector3 position)
        {
            _homePosition = position;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw home position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_homePosition, 0.5f);

            // Draw line to home
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _homePosition);
        }
    }
}
