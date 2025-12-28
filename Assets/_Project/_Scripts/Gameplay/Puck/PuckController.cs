using UnityEngine;

namespace HockeyGame.Gameplay.Puck
{
    [RequireComponent(typeof(Rigidbody))]
    public class PuckController : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private float _maxSpeed = 50f;
        [SerializeField] private float _friction = 0.98f;
        [SerializeField] private float _bounciness = 0.8f;

        [Header("References")]
        [SerializeField] private TrailRenderer _trailRenderer;

        private Rigidbody _rigidbody;
        private Transform _owner;
        private bool _isLoose = true;

        public bool IsLoose => _isLoose;
        public Transform Owner => _owner;
        public Vector3 Velocity => _rigidbody.linearVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        private void ConfigureRigidbody()
        {
            _rigidbody.mass = 0.17f; // ~170 grams (standard puck)
            _rigidbody.linearDamping = 0.1f;
            _rigidbody.angularDamping = 0.5f;

            // 2.5D constraint: freeze Y position
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Keep puck on ice level
            Vector3 pos = transform.position;
            pos.y = 0.05f; // Slightly above ice
            transform.position = pos;
        }

        private void FixedUpdate()
        {
            ApplyFriction();
            ClampSpeed();
            KeepOnIce();
        }

        private void ApplyFriction()
        {
            if (_isLoose)
            {
                Vector3 vel = _rigidbody.linearVelocity;
                vel.x *= _friction;
                vel.z *= _friction;
                _rigidbody.linearVelocity = vel;
            }
        }

        private void ClampSpeed()
        {
            Vector3 vel = _rigidbody.linearVelocity;
            vel.y = 0;

            if (vel.magnitude > _maxSpeed)
            {
                vel = vel.normalized * _maxSpeed;
                _rigidbody.linearVelocity = vel;
            }
        }

        private void KeepOnIce()
        {
            Vector3 pos = transform.position;
            if (Mathf.Abs(pos.y - 0.05f) > 0.01f)
            {
                pos.y = 0.05f;
                transform.position = pos;

                Vector3 vel = _rigidbody.linearVelocity;
                vel.y = 0;
                _rigidbody.linearVelocity = vel;
            }
        }

        public void Shoot(Vector3 direction, float power)
        {
            direction.y = 0;
            direction.Normalize();

            _rigidbody.linearVelocity = direction * power;
            _isLoose = true;
            _owner = null;

            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = true;
            }

            Debug.Log($"[Puck] Shot with power {power}, direction {direction}");
        }

        public void Pass(Vector3 direction, float power)
        {
            Shoot(direction, power * 0.6f); // Passes are softer
        }

        public void AttachToPlayer(Transform player)
        {
            _owner = player;
            _isLoose = false;
            _rigidbody.linearVelocity = Vector3.zero;

            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
            }
        }

        public void Drop()
        {
            _owner = null;
            _isLoose = true;
        }

        public void ResetToPosition(Vector3 position)
        {
            transform.position = new Vector3(position.x, 0.05f, position.z);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _owner = null;
            _isLoose = true;

            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Add bounce effect on wall hits (check by name since Wall tag may not exist)
            if (collision.gameObject.name.Contains("Wall"))
            {
                Vector3 vel = _rigidbody.linearVelocity;
                vel *= _bounciness;
                _rigidbody.linearVelocity = vel;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_rigidbody != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, _rigidbody.linearVelocity.normalized * 2f);
            }
        }
    }
}
