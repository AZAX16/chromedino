using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class WorldScrollController : MonoBehaviour
    {
        private RunnerSettings _settings;
        private PlayerController _player;
        private ScoreTracker _score;
        private bool _running;
        private bool _harderDifficulty;
        private float _effectiveSpeed;

        public double TotalDistance { get; private set; }
        public float StepDistance { get; private set; }
        public float DifficultyMultiplier => _harderDifficulty ? _settings.HarderSpeedMultiplier : 1f;
        public float NormalSpeed => _settings.BaseSpeed * DifficultyMultiplier;
        public float DashSpeed => NormalSpeed * _settings.DashSpeedMultiplier;
        // Average speed actually used by the last Tick, including a partial dash.
        public float EffectiveSpeed => _running ? _effectiveSpeed : 0f;

        public void Initialize(RunnerSettings settings, PlayerController player, ScoreTracker score)
        {
            _settings = settings;
            _player = player;
            _score = score;
            ResetRun();
        }

        public void SetRunning(bool running) => _running = running;

        public void ResetRun()
        {
            _running = false;
            _harderDifficulty = false;
            _effectiveSpeed = _settings.BaseSpeed;
            TotalDistance = 0d;
            StepDistance = 0f;
        }

        public void Tick(float deltaSeconds)
        {
            StepDistance = 0f;
            if (!_running || deltaSeconds <= 0f) return;

            // Poll total current-run score before movement. Coins collected by a
            // sweep or physics callback in the prior step apply by this step.
            _harderDifficulty |= _score.CurrentScore >= _settings.DifficultyScoreThreshold;
            _effectiveSpeed = NormalSpeed * _player.ConsumeSpeedMultiplier(deltaSeconds);
            StepDistance = _effectiveSpeed * deltaSeconds;
            TotalDistance += StepDistance;
        }
    }
}
