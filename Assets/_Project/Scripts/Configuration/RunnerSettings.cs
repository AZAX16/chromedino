using UnityEngine;

namespace ChromeDino
{
    [CreateAssetMenu(fileName = "RunnerSettings", menuName = "Chrome Dino/Runner Settings")]
    public sealed class RunnerSettings : ScriptableObject
    {
        public const int MaximumLives = 3;

        [Header("Movement")]
        [SerializeField, Min(0.01f)] private float _baseSpeed = 6f;
        [SerializeField, Min(0.01f)] private float _jumpVelocity = 10f;
        [SerializeField, Min(0.01f)] private float _gravityScale = 3f;
        [SerializeField, Min(1f)] private float _dashSpeedMultiplier = 2f;
        [SerializeField, Min(0.01f)] private float _dashDurationSeconds = 0.2f;
        [SerializeField, Min(0.01f)] private float _dashCooldownSeconds = 1.5f;

        [Header("Difficulty")]
        [SerializeField, Min(1)] private int _difficultyScoreThreshold = 500;
        [SerializeField, Min(1f)] private float _harderSpeedMultiplier = 1.25f;

        [Header("Damage")]
        [SerializeField, Min(0.01f)] private float _invulnerabilitySeconds = 1f;
        [SerializeField, Min(0.01f)] private float _hitRecoverySeconds = 0.25f;
        [SerializeField, Min(0.01f)] private float _hitFlashSeconds = 0.15f;
        [SerializeField] private Color _hitColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _dashColor = new Color(0.3f, 0.85f, 1f, 1f);

        [Header("Scoring")]
        [SerializeField, Min(0f)] private float _pointsPerWorldUnit = 1f;
        [SerializeField, Min(0)] private int _coinPoints = 50;

        [Header("Spawning (world units and gameplay seconds)")]
        [SerializeField, Min(2f)] private float _firstSpawnDelaySeconds = 2f;
        [SerializeField, Min(0.01f)] private float _minimumSpawnIntervalSeconds = 1.2f;
        [SerializeField, Min(0.01f)] private float _maximumSpawnIntervalSeconds = 2f;
        [SerializeField, Min(0.1f), Tooltip("Extra distance beyond each horizontal camera edge.")]
        private float _boundaryMarginWorldUnits = 2f;
        [SerializeField, Min(0.1f)] private float _reactionSeconds = 0.6f;
        [SerializeField, Min(0.01f)] private float _landingRecoverySeconds = 0.25f;
        [SerializeField, Min(0.1f), Tooltip("Conservative minimum edge-to-edge separation; reachability may increase it.")]
        private float _minimumClearGapWorldUnits = 12f;
        [SerializeField, Min(1)] private int _maximumActiveInteractables = 16;

        [Header("Coin patterns (relative weights; zero disables a pattern)")]
        [SerializeField, Min(0f)] private float _singleCoinWeight = 1f;
        [SerializeField, Min(0f)] private float _coinRowWeight = 2f;
        [SerializeField, Min(0f)] private float _coinArcWeight = 2f;
        [SerializeField, Min(0f)] private float _repeatedJumpCoinWeight = 1f;
        [SerializeField, Min(0.1f), Tooltip("Row center separation, increased if needed to keep coin colliders distinct.")]
        private float _coinRowSpacingWorldUnits = 1f;
        [SerializeField, Range(0.2f, 0.5f), Tooltip("Time from the arc peak to either neighbor, as a fraction of jump time to apex.")]
        private float _coinArcHalfDurationFraction = 0.4f;
        [SerializeField, Min(0.1f), Tooltip("Extra time between repeated coin jumps, after flight and Landing Recovery Seconds. Hazard reaction spacing is separate.")]
        private float _repeatedJumpReactionSeconds = 0.15f;

        public float BaseSpeed => _baseSpeed;
        public float JumpVelocity => _jumpVelocity;
        public float GravityScale => _gravityScale;
        public float DashSpeedMultiplier => _dashSpeedMultiplier;
        // Global safety limit, including a dash in the hardest stage of any run.
        public float MaximumSpeed => _baseSpeed * _dashSpeedMultiplier * _harderSpeedMultiplier;
        public float DashDurationSeconds => _dashDurationSeconds;
        public float DashCooldownSeconds => _dashCooldownSeconds;
        public int DifficultyScoreThreshold => _difficultyScoreThreshold;
        public float HarderSpeedMultiplier => _harderSpeedMultiplier;
        public float InvulnerabilitySeconds => _invulnerabilitySeconds;
        public float HitRecoverySeconds => _hitRecoverySeconds;
        public float HitFlashSeconds => _hitFlashSeconds;
        public Color HitColor => _hitColor;
        public Color DashColor => _dashColor;
        public float PointsPerWorldUnit => _pointsPerWorldUnit;
        public int CoinPoints => _coinPoints;
        public float FirstSpawnDelaySeconds => _firstSpawnDelaySeconds;
        public float MinimumSpawnIntervalSeconds => _minimumSpawnIntervalSeconds;
        public float MaximumSpawnIntervalSeconds => _maximumSpawnIntervalSeconds;
        public float BoundaryMarginWorldUnits => _boundaryMarginWorldUnits;
        public float ReactionSeconds => _reactionSeconds;
        public float LandingRecoverySeconds => _landingRecoverySeconds;
        public float MinimumClearGapWorldUnits => _minimumClearGapWorldUnits;
        public int MaximumActiveInteractables => _maximumActiveInteractables;
        public float SingleCoinWeight => _singleCoinWeight;
        public float CoinRowWeight => _coinRowWeight;
        public float CoinArcWeight => _coinArcWeight;
        public float RepeatedJumpCoinWeight => _repeatedJumpCoinWeight;
        public float CoinRowSpacingWorldUnits => _coinRowSpacingWorldUnits;
        public float CoinArcHalfDurationFraction => _coinArcHalfDurationFraction;
        public float RepeatedJumpReactionSeconds => _repeatedJumpReactionSeconds;

        public void ValidateValues()
        {
            _baseSpeed = Mathf.Max(0.01f, _baseSpeed);
            _jumpVelocity = Mathf.Max(0.01f, _jumpVelocity);
            _gravityScale = Mathf.Max(0.01f, _gravityScale);
            _dashSpeedMultiplier = Mathf.Max(1f, _dashSpeedMultiplier);
            _dashDurationSeconds = Mathf.Max(0.01f, _dashDurationSeconds);
            _dashCooldownSeconds = Mathf.Max(0.01f, _dashCooldownSeconds);
            _difficultyScoreThreshold = Mathf.Max(1, _difficultyScoreThreshold);
            _harderSpeedMultiplier = float.IsNaN(_harderSpeedMultiplier) || float.IsInfinity(_harderSpeedMultiplier)
                ? 1.25f : Mathf.Max(1f, _harderSpeedMultiplier);
            _invulnerabilitySeconds = Mathf.Max(0.03f, _invulnerabilitySeconds);
            _hitRecoverySeconds = Mathf.Clamp(_hitRecoverySeconds, 0.01f, _invulnerabilitySeconds);
            _hitFlashSeconds = Mathf.Clamp(_hitFlashSeconds, 0.01f, _invulnerabilitySeconds - 0.01f);
            _pointsPerWorldUnit = Mathf.Max(0f, _pointsPerWorldUnit);
            _coinPoints = Mathf.Max(0, _coinPoints);
            _firstSpawnDelaySeconds = Mathf.Max(2f, _firstSpawnDelaySeconds);
            _minimumSpawnIntervalSeconds = Mathf.Max(0.01f, _minimumSpawnIntervalSeconds);
            _maximumSpawnIntervalSeconds = Mathf.Max(_minimumSpawnIntervalSeconds, _maximumSpawnIntervalSeconds);
            _boundaryMarginWorldUnits = Mathf.Max(0.1f, _boundaryMarginWorldUnits);
            _reactionSeconds = Mathf.Max(0.1f, _reactionSeconds);
            _landingRecoverySeconds = Mathf.Max(0.01f, _landingRecoverySeconds);
            _minimumClearGapWorldUnits = Mathf.Max(0.1f, _minimumClearGapWorldUnits);
            _maximumActiveInteractables = Mathf.Clamp(_maximumActiveInteractables, 1, 128);
            _singleCoinWeight = ValidWeight(_singleCoinWeight);
            _coinRowWeight = ValidWeight(_coinRowWeight);
            _coinArcWeight = ValidWeight(_coinArcWeight);
            _repeatedJumpCoinWeight = ValidWeight(_repeatedJumpCoinWeight);
            _coinRowSpacingWorldUnits = float.IsNaN(_coinRowSpacingWorldUnits) || float.IsInfinity(_coinRowSpacingWorldUnits)
                ? 1f : Mathf.Max(0.1f, _coinRowSpacingWorldUnits);
            _coinArcHalfDurationFraction = float.IsNaN(_coinArcHalfDurationFraction) || float.IsInfinity(_coinArcHalfDurationFraction)
                ? 0.4f : Mathf.Clamp(_coinArcHalfDurationFraction, 0.2f, 0.5f);
            _repeatedJumpReactionSeconds = float.IsNaN(_repeatedJumpReactionSeconds) || float.IsInfinity(_repeatedJumpReactionSeconds)
                ? 0.15f : Mathf.Max(0.1f, _repeatedJumpReactionSeconds);
        }

        private static float ValidWeight(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Max(0f, value);

        private void OnValidate() => ValidateValues();
    }
}
