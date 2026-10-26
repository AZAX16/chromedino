using System;
using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour
    {
        private float _invulnerabilityDuration;
        private float _recoveryDuration;
        private float _invulnerabilityRemaining;
        private float _recoveryRemaining;
        private bool _acceptDamage;

        public int CurrentLives { get; private set; } = RunnerSettings.MaximumLives;
        public int MaxLives => RunnerSettings.MaximumLives;
        public bool IsDead => CurrentLives == 0;
        public bool IsInvulnerable => _invulnerabilityRemaining > 0f;
        public bool IsRecovering => !IsDead && _recoveryRemaining > 0f;
        public event Action HealthChanged;
        public event Action Damaged;
        public event Action Died;

        public void Initialize(RunnerSettings settings)
        {
            _invulnerabilityDuration = settings.InvulnerabilitySeconds;
            _recoveryDuration = settings.HitRecoverySeconds;
            ResetRun();
        }

        public void SetDamageEnabled(bool enabled) => _acceptDamage = enabled;

        public void ResetRun()
        {
            CurrentLives = MaxLives;
            _invulnerabilityRemaining = 0f;
            _recoveryRemaining = 0f;
            _acceptDamage = false;
            HealthChanged?.Invoke();
        }

        public void Tick(float deltaSeconds)
        {
            if (!_acceptDamage || IsDead)
            {
                return;
            }

            _invulnerabilityRemaining = Mathf.Max(0f, _invulnerabilityRemaining - deltaSeconds);
            _recoveryRemaining = Mathf.Max(0f, _recoveryRemaining - deltaSeconds);
        }

        public bool TryTakeDamage()
        {
            if (!_acceptDamage || IsDead || IsInvulnerable)
            {
                return false;
            }

            CurrentLives--;
            // Install guards before notifying listeners, including re-entrant contacts.
            _invulnerabilityRemaining = IsDead ? 0f : _invulnerabilityDuration;
            _recoveryRemaining = IsDead ? 0f : _recoveryDuration;
            if (IsDead)
            {
                _acceptDamage = false;
                Died?.Invoke();
            }
            else
            {
                Damaged?.Invoke();
            }

            HealthChanged?.Invoke();
            return true;
        }
    }
}
