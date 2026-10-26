using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerHitFeedback : MonoBehaviour
    {
        private SpriteRenderer _sprite;
        private Color _originalColor;
        private RunnerSettings _settings;
        private PlayerHealth _health;
        private PlayerController _player;
        private float _flashRemaining;
        private bool _initialized;

        public void Initialize(PlayerController player, RunnerSettings settings)
        {
            _sprite = GetComponent<SpriteRenderer>();
            _originalColor = _sprite.color;
            _player = player;
            _health = player.Health;
            _settings = settings;
            _initialized = true;
            if (isActiveAndEnabled) Subscribe();
        }

        public void Tick(float deltaSeconds)
        {
            _flashRemaining = Mathf.Max(0f, _flashRemaining - deltaSeconds);
            _sprite.color = _flashRemaining > 0f ? _settings.HitColor
                : _player.IsDashing ? _settings.DashColor : _originalColor;
        }

        public void ResetVisuals()
        {
            _flashRemaining = 0f;
            if (_sprite != null) _sprite.color = _originalColor;
        }

        private void OnDamaged()
        {
            _flashRemaining = _settings.HitFlashSeconds;
            _sprite.color = _settings.HitColor;
        }

        private void Subscribe()
        {
            _health.Damaged += OnDamaged;
            _health.Died += ResetVisuals;
        }

        private void OnEnable()
        {
            if (_initialized) Subscribe();
        }

        private void OnDisable()
        {
            if (!_initialized) return;
            _health.Damaged -= OnDamaged;
            _health.Died -= ResetVisuals;
            ResetVisuals();
        }
    }
}
