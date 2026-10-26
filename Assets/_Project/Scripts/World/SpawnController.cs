using System.Collections.Generic;
using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class SpawnController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private GroundObstacle _groundObstaclePrefab;
        [SerializeField] private AerialObstacle _aerialObstaclePrefab;
        [SerializeField] private CoinCollectible _coinPrefab;
        [SerializeField] private Transform _groundSpawnHeight;
        [SerializeField] private Transform _aerialSpawnHeight;
        [SerializeField] private Transform _coinSpawnHeight;
        [SerializeField] private Transform _spawnedObjectsRoot;

        private readonly List<Interactable> _active = new List<Interactable>();
        private readonly Interactable[] _prefabs = new Interactable[3];
        private readonly float[] _widths = new float[3];
        private GameSession _session;
        private RunnerSettings _settings;
        private System.Random _random;
        private float _countdown;
        private double _lastSpawnDistance;
        private float _lastRightEdge;
        private float _standingCenterY;
        private Vector2 _coinSize;
        private float _coinCenterOffsetY;
        private CoinPatternKind? _pendingCoinPattern;
        private bool _hasSpawned;
        private int _nextKind;
        private float _spawnX;
        private float _cleanupX;

        public int ActiveCount => _active.Count;
        public float CountdownSeconds => _countdown;

        public bool Initialize(GameSession session)
        {
            _session = session;
            _settings = session.Settings;
            bool valid = SetupValidation.Require(_camera, this, "Camera")
                & SetupValidation.Require(_groundObstaclePrefab, this, "Ground Obstacle Prefab")
                & SetupValidation.Require(_aerialObstaclePrefab, this, "Aerial Obstacle Prefab")
                & SetupValidation.Require(_coinPrefab, this, "Coin Prefab")
                & SetupValidation.Require(_groundSpawnHeight, this, "Ground Spawn Height")
                & SetupValidation.Require(_aerialSpawnHeight, this, "Aerial Spawn Height")
                & SetupValidation.Require(_coinSpawnHeight, this, "Coin Spawn Height")
                & SetupValidation.Require(_spawnedObjectsRoot, this, "Spawned Objects Root");
            if (!valid) return false;
            valid &= SetupValidation.Check(_camera.orthographic, this, "Use an orthographic Camera.");
            valid &= SetupValidation.Check(Physics2D.gravity.y < 0f && Mathf.Approximately(Physics2D.gravity.x, 0f),
                this, "Spawning assumes downward Physics2D gravity and flat ground.");
            valid &= SetupValidation.Check(_spawnedObjectsRoot.lossyScale == Vector3.one,
                this, "Spawned Objects Root must have world scale (1,1,1).");
            _prefabs[0] = _groundObstaclePrefab;
            _prefabs[1] = _aerialObstaclePrefab;
            _prefabs[2] = _coinPrefab;
            for (int i = 0; i < _prefabs.Length; i++) valid &= ValidatePrefab(i);
            if (!valid) return false;

            float playerBottom = session.Player.Bottom;
            _standingCenterY = playerBottom + session.Player.Height * 0.5f;
            BoxCollider2D coin = _coinPrefab.GetComponent<BoxCollider2D>();
            _coinSize = Vector2.Scale(coin.size, _coinPrefab.transform.localScale);
            _coinCenterOffsetY = coin.offset.y * _coinPrefab.transform.localScale.y;
            BoxCollider2D ground = _groundObstaclePrefab.GetComponent<BoxCollider2D>();
            BoxCollider2D air = _aerialObstaclePrefab.GetComponent<BoxCollider2D>();
            float groundTop = _groundSpawnHeight.position.y + (ground.offset.y + ground.size.y / 2f)
                * _groundObstaclePrefab.transform.localScale.y;
            float airBottom = _aerialSpawnHeight.position.y + (air.offset.y - air.size.y / 2f)
                * _aerialObstaclePrefab.transform.localScale.y;
            float jumpHeight = _settings.JumpVelocity * _settings.JumpVelocity
                / (2f * -Physics2D.gravity.y * _settings.GravityScale);
            valid &= SetupValidation.Check(groundTop > playerBottom && groundTop - playerBottom < jumpHeight * 0.65f,
                this, "Ground hazard must rise above the floor and fit comfortably below jump apex.");
            float clearanceHeight = Mathf.Max(0f, groundTop - playerBottom) + 0.1f;
            float gravity = -Physics2D.gravity.y * _settings.GravityScale;
            float clearanceSeconds = 2f * Mathf.Sqrt(Mathf.Max(0f,
                _settings.JumpVelocity * _settings.JumpVelocity - 2f * gravity * clearanceHeight)) / gravity;
            valid &= SetupValidation.Check((_widths[0] + session.Player.Width) / _settings.BaseSpeed < clearanceSeconds * 0.8f,
                this, "Ground hazard is too wide to clear at base speed; reduce its collider width.");
            // Aerial hazards are safe while grounded but obstruct a jump. Large spacing
            // leaves time to land before choosing how to pass the next hazard.
            valid &= SetupValidation.Check(airBottom > playerBottom + session.Player.Height + 0.15f
                && airBottom < playerBottom + session.Player.Height + jumpHeight * 0.8f,
                this, "Aerial bottom must clear the standing player but intersect the jump path.");
            _random = new System.Random();
            ResetRun();
            return valid;
        }

        public void ResetRun(int? randomSeed = null)
        {
            Clear();
            if (randomSeed.HasValue) _random = new System.Random(randomSeed.Value);
            _countdown = _settings.FirstSpawnDelaySeconds;
            UpdateBoundaries();
        }

        public void Clear()
        {
            foreach (Interactable item in _active)
            {
                if (item != null) item.Despawn();
            }

            _active.Clear();
            _lastSpawnDistance = 0d;
            _lastRightEdge = 0f;
            _hasSpawned = false;
            _pendingCoinPattern = null;
            _nextKind = 0;
            _countdown = _settings != null ? _settings.FirstSpawnDelaySeconds : 0f;
        }

        public void StopMotion()
        {
            foreach (Interactable item in _active)
            {
                if (item != null && item.gameObject.activeSelf) item.Mover.StopMotion();
            }
        }

        public void Tick(float deltaSeconds, float distance, double totalDistance)
        {
            UpdateBoundaries();
            // Creation order is stable, so simultaneous encounters have a stable order.
            for (int i = 0; i < _active.Count; i++)
            {
                Interactable item = _active[i];
                if (item != null && item.gameObject.activeSelf) item.Mover.Step(distance, _cleanupX);
                if (_session.State != GameState.Playing) return;
            }

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (IsDespawned(_active[i])) _active.RemoveAt(i);
            }
            _countdown = Mathf.Max(0f, _countdown - deltaSeconds);
            if (_countdown > 0f || _active.Count >= _settings.MaximumActiveInteractables) return;

            Transform height = _nextKind == 0 ? _groundSpawnHeight : _nextKind == 1 ? _aerialSpawnHeight : _coinSpawnHeight;
            CoinPatternLayout layout = default;
            int count = 1;
            if (_nextKind == 2)
            {
                // Select once per opportunity, including while blocked by space/cap.
                if (!_pendingCoinPattern.HasValue) _pendingCoinPattern = CoinPatterns.Select(_settings, _random);
                float normalSpeed = _settings.BaseSpeed * (_session.Score.CurrentScore >= _settings.DifficultyScoreThreshold
                    ? _settings.HarderSpeedMultiplier : 1f);
                layout = CoinPatterns.Create(_pendingCoinPattern.Value, _settings, -Physics2D.gravity.y,
                    _standingCenterY, _session.Player.Width, _coinSize,
                    _coinSpawnHeight.position.y + _coinCenterOffsetY, normalSpeed);
                count = layout.Count;
            }
            // An Inspector cap smaller than an entire pattern can never free
            // enough slots. Skip that opportunity so hazards keep rotating.
            if (count > _settings.MaximumActiveInteractables)
            {
                AdvanceOpportunity();
                return;
            }
            // Never truncate a pattern or swap to a cheaper one when capacity is low.
            if (_active.Count + count > _settings.MaximumActiveInteractables) return;

            float edgeGap = SpawnSpacing.MinimumEdgeGap(_settings, -Physics2D.gravity.y, _session.Player.Width);
            double previousRightEdge = _lastRightEdge - (totalDistance - _lastSpawnDistance);
            if (_hasSpawned && _spawnX - _widths[_nextKind] * 0.5f - previousRightEdge < edgeGap) return;

            for (int i = 0; i < count; i++)
            {
                Vector2 offset = _nextKind == 2 ? layout.Position(i) : new Vector2(0f, height.position.y);
                if (_nextKind == 2) offset.y -= _coinCenterOffsetY;
                Interactable spawned = Instantiate(_prefabs[_nextKind],
                    new Vector3(_spawnX + offset.x, offset.y, 0f), Quaternion.identity, _spawnedObjectsRoot);
                spawned.Initialize(_session);
                _active.Add(spawned);
            }
            _hasSpawned = true;
            _lastSpawnDistance = totalDistance;
            _lastRightEdge = _spawnX + (_nextKind == 2 ? layout.Span : 0f) + _widths[_nextKind] * 0.5f;
            AdvanceOpportunity();
        }

        private void AdvanceOpportunity()
        {
            _pendingCoinPattern = null;
            // Cycle all required kinds with randomized waiting times; no arbitrary
            // combinations of blocking hazards, and no coincident coins/hazards.
            _nextKind = (_nextKind + 1) % _prefabs.Length;
            _countdown = Mathf.Lerp(_settings.MinimumSpawnIntervalSeconds,
                _settings.MaximumSpawnIntervalSeconds, (float)_random.NextDouble());
        }

        private void UpdateBoundaries()
        {
            float halfWidth = _camera.orthographicSize * _camera.aspect;
            float widest = Mathf.Max(_widths[0], Mathf.Max(_widths[1], _widths[2]));
            // Existing objects must remain fair if a coin activates difficulty.
            float warningDistance = _settings.MaximumSpeed * _settings.ReactionSeconds;
            _spawnX = Mathf.Max(_camera.transform.position.x + halfWidth + _settings.BoundaryMarginWorldUnits + widest,
                _session.Player.transform.position.x + warningDistance + widest);
            _cleanupX = _camera.transform.position.x - halfWidth - _settings.BoundaryMarginWorldUnits;
        }

        private bool ValidatePrefab(int index)
        {
            Interactable prefab = _prefabs[index];
            BoxCollider2D collider = prefab.GetComponent<BoxCollider2D>();
            Rigidbody2D body = prefab.GetComponent<Rigidbody2D>();
            bool valid = SetupValidation.Require(collider, this, prefab.name + " BoxCollider2D")
                & SetupValidation.Require(body, this, prefab.name + " Rigidbody2D");
            if (!valid) return false;
            valid &= SetupValidation.Check(collider.isTrigger && collider.enabled && prefab.gameObject.activeSelf
                && prefab.enabled && body.simulated && body.bodyType == RigidbodyType2D.Kinematic,
                this, prefab.name + " needs an enabled trigger, active Interactable, and simulated Kinematic body.");
            valid &= SetupValidation.Check(!Physics2D.GetIgnoreLayerCollision(prefab.gameObject.layer,
                _session.Player.gameObject.layer), this, prefab.name + " layer must interact with Player in the Physics2D matrix.");
            valid &= SetupValidation.Check(prefab.transform.localScale.x > 0f && prefab.transform.localScale.y > 0f
                && Mathf.Abs(collider.offset.x) < 0.001f,
                this, prefab.name + " needs positive scale and centered collider X offset.");
            _widths[index] = collider.size.x * prefab.transform.localScale.x;
            return valid;
        }

        private static bool IsDespawned(Interactable item) => item == null || !item.gameObject.activeSelf;

        private void OnDisable() => Clear();
    }
}
