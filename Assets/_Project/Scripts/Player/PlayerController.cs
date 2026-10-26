using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(PlayerHealth))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Collider2D _bodyCollider;
        [SerializeField] private Transform _feetCheck;
        [SerializeField, Min(0.01f)] private float _groundCheckRadius = 0.08f;
        [SerializeField] private LayerMask _groundLayers;

        private Rigidbody2D _body;
        private PlayerHealth _health;
        private GameSession _session;
        private RunnerSettings _settings;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private RigidbodyConstraints2D _startConstraints;
        private float _dashRemaining;
        private float _cooldownRemaining;
        private bool _jumpQueued;
        private bool _dashQueued;
        private bool _initialized;

        public PlayerHealth Health => _health;
        internal Rigidbody2D Body => _body;
        public bool IsGrounded { get; private set; }
        public bool IsDashing => _dashRemaining > 0f;
        public float DashCooldownRemaining => _cooldownRemaining;
        public float VerticalVelocity => _body != null ? _body.linearVelocity.y : 0f;
        public float Width => _bodyCollider.bounds.size.x;
        public float Height => _bodyCollider.bounds.size.y;
        public float Bottom => _bodyCollider.bounds.min.y;

        public bool Initialize(GameSession session, RunnerSettings settings)
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<PlayerHealth>();
            bool valid = SetupValidation.Require(_bodyCollider, this, "Body Collider")
                & SetupValidation.Require(_feetCheck, this, "Feet Check");
            valid &= SetupValidation.Check(_groundLayers.value != 0
                && (_groundLayers.value & (1 << gameObject.layer)) == 0,
                this, "Ground Layers must contain Ground only and exclude Player.");
            valid &= SetupValidation.Check(_body.bodyType == RigidbodyType2D.Dynamic,
                this, "Player Rigidbody2D must be Dynamic.");
            if (!valid) return false;
            if (!SetupValidation.Check(_bodyCollider.attachedRigidbody == _body && !_bodyCollider.isTrigger,
                this, "Body Collider must be solid and attached to the player's Rigidbody2D.")) return false;

            _session = session;
            _settings = settings;
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _startConstraints = _body.constraints;
            _health.Initialize(settings);
            _initialized = true;
            if (isActiveAndEnabled) _health.Damaged += OnDamaged;
            ResetRun();
            return true;
        }

        public void QueueCommands(bool jump, bool dash)
        {
            if (_session.State != GameState.Playing || _health.IsRecovering || _health.IsDead)
            {
                return;
            }

            _jumpQueued |= jump && IsGrounded;
            _dashQueued |= dash && !IsDashing && _cooldownRemaining <= 0f;
        }

        public void ClearCommands()
        {
            _jumpQueued = false;
            _dashQueued = false;
        }

        public void ResetRun()
        {
            ClearCommands();
            _dashRemaining = 0f;
            _cooldownRemaining = 0f;
            _body.simulated = false;
            transform.SetPositionAndRotation(_startPosition, _startRotation);
            _body.position = _startPosition;
            _body.rotation = _startRotation.eulerAngles.z;
            _body.constraints = _startConstraints | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            _body.gravityScale = _settings.GravityScale;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            IsGrounded = true;
        }

        public void BeginRun()
        {
            _body.simulated = true;
            Physics2D.SyncTransforms();
            RefreshGrounded();
        }

        public void StopRun()
        {
            ClearCommands();
            _dashRemaining = 0f;
            _cooldownRemaining = 0f;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.simulated = false;
        }

        public void PhysicsTick()
        {
            RefreshGrounded();
            if (!_health.IsRecovering && !_health.IsDead)
            {
                if (_jumpQueued && IsGrounded)
                {
                    _body.linearVelocity = new Vector2(0f, _settings.JumpVelocity);
                    IsGrounded = false;
                }

                if (_dashQueued && !IsDashing && _cooldownRemaining <= 0f)
                {
                    _dashRemaining = _settings.DashDurationSeconds;
                }
            }

            ClearCommands();
        }

        // Integrate a partial dash exactly, carrying any overshoot into its cooldown.
        public float ConsumeSpeedMultiplier(float deltaSeconds)
        {
            if (!IsDashing)
            {
                _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - deltaSeconds);
                return 1f;
            }

            float dashSeconds = Mathf.Min(deltaSeconds, _dashRemaining);
            _dashRemaining = Mathf.Max(0f, _dashRemaining - deltaSeconds);
            if (!IsDashing)
            {
                _cooldownRemaining = Mathf.Max(0f, _settings.DashCooldownSeconds - (deltaSeconds - dashSeconds));
            }

            return 1f + (_settings.DashSpeedMultiplier - 1f) * dashSeconds / deltaSeconds;
        }

        private void RefreshGrounded()
        {
            // The probe can still overlap Ground during the first upward step.
            IsGrounded = _body.linearVelocity.y <= 0.01f
                && Physics2D.OverlapCircle(_feetCheck.position, _groundCheckRadius, _groundLayers) != null;
        }

        private void OnDamaged()
        {
            ClearCommands();
            if (IsDashing)
            {
                _dashRemaining = 0f;
                _cooldownRemaining = _settings.DashCooldownSeconds;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_initialized && other.TryGetComponent(out Interactable interactable))
            {
                interactable.TryInteract(this);
            }
            else if (_initialized)
            {
                Interactable parent = other.GetComponentInParent<Interactable>();
                if (parent != null) parent.TryInteract(this);
            }
        }

        private void OnEnable()
        {
            if (_initialized) _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            if (!_initialized) return;
            _health.Damaged -= OnDamaged;
            StopRun();
        }

        private void OnDrawGizmosSelected()
        {
            if (_feetCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_feetCheck.position, _groundCheckRadius);
        }
    }
}
