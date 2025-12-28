using UnityEngine;

namespace HockeyGame.Core
{
    [RequireComponent(typeof(Camera))]
    public class GameCamera : MonoBehaviour
    {
        [Header("Position Settings")]
        [SerializeField] private float _height = 15f;
        [SerializeField] private float _tiltAngle = 60f;
        [SerializeField] private float _followSpeed = 5f;
        [SerializeField] private float _lookAheadDistance = 2f;

        [Header("Rink Bounds (NHL: 60m x 26m)")]
        [SerializeField] private Vector2 _rinkMinBounds = new Vector2(-13f, -30f);
        [SerializeField] private Vector2 _rinkMaxBounds = new Vector2(13f, 30f);

        [Header("Camera Padding")]
        [SerializeField] private float _boundsPadding = 5f;

        private Transform _target;
        private Camera _camera;
        private Vector3 _currentPosition;
        private Vector3 _targetLastPosition;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            ConfigureCamera();
        }

        private void ConfigureCamera()
        {
            // Set up orthographic camera with tilt
            transform.rotation = Quaternion.Euler(_tiltAngle, 0f, 0f);

            // Calculate initial position
            _currentPosition = CalculateCameraPosition(Vector3.zero);
            transform.position = _currentPosition;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                _targetLastPosition = _target.position;
                _currentPosition = CalculateCameraPosition(_target.position);
                transform.position = _currentPosition;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPosition = _target.position;

            // Calculate look-ahead based on velocity
            Vector3 velocity = (targetPosition - _targetLastPosition) / Time.deltaTime;
            Vector3 lookAheadOffset = velocity.normalized * _lookAheadDistance;
            lookAheadOffset.y = 0;

            Vector3 desiredFocusPoint = targetPosition + lookAheadOffset;

            // Clamp focus point to rink bounds with padding
            desiredFocusPoint.x = Mathf.Clamp(desiredFocusPoint.x,
                _rinkMinBounds.x + _boundsPadding,
                _rinkMaxBounds.x - _boundsPadding);
            desiredFocusPoint.z = Mathf.Clamp(desiredFocusPoint.z,
                _rinkMinBounds.y + _boundsPadding,
                _rinkMaxBounds.y - _boundsPadding);

            // Calculate desired camera position
            Vector3 desiredCameraPos = CalculateCameraPosition(desiredFocusPoint);

            // Smooth follow
            _currentPosition = Vector3.Lerp(_currentPosition, desiredCameraPos, _followSpeed * Time.deltaTime);
            transform.position = _currentPosition;

            _targetLastPosition = targetPosition;
        }

        private Vector3 CalculateCameraPosition(Vector3 focusPoint)
        {
            // Calculate Z offset based on tilt angle to keep target in view
            float zOffset = _height / Mathf.Tan(_tiltAngle * Mathf.Deg2Rad);

            return new Vector3(focusPoint.x, _height, focusPoint.z - zOffset);
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            _rinkMinBounds = min;
            _rinkMaxBounds = max;
        }

        public void SetHeight(float height)
        {
            _height = height;
            ConfigureCamera();
        }

        public void SetTilt(float angle)
        {
            _tiltAngle = Mathf.Clamp(angle, 30f, 90f);
            ConfigureCamera();
        }

        public void Shake(float duration, float magnitude)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.position = _currentPosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = _currentPosition;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw rink bounds
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (_rinkMinBounds.x + _rinkMaxBounds.x) / 2f,
                0f,
                (_rinkMinBounds.y + _rinkMaxBounds.y) / 2f
            );
            Vector3 size = new Vector3(
                _rinkMaxBounds.x - _rinkMinBounds.x,
                0.1f,
                _rinkMaxBounds.y - _rinkMinBounds.y
            );
            Gizmos.DrawWireCube(center, size);

            // Draw camera frustum direction
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 10f);
        }
    }
}
