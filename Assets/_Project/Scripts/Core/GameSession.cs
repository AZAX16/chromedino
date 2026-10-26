using System;
using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class GameSession : MonoBehaviour
    {
        [SerializeField] private RunnerSettings _settings;
        [SerializeField] private RunnerInputReader _input;
        [SerializeField] private PlayerController _player;
        [SerializeField] private PlayerAnimationController _animation;
        [SerializeField] private PlayerHitFeedback _feedback;
        [SerializeField] private WorldScrollController _world;
        [SerializeField] private SpawnController _spawner;
        [SerializeField] private ParallaxLayer[] _parallaxLayers;
        [SerializeField] private Camera _camera;
        [SerializeField] private ScoreTracker _score;
        [SerializeField] private HudController _hud;
        [SerializeField] private MenuController _menus;

        private bool _ready;
        public GameState State { get; private set; } = GameState.MainMenu;
        public RunnerSettings Settings => _settings;
        public PlayerController Player => _player;
        public ScoreTracker Score => _score;
        public event Action<GameState> StateChanged;

        private void Awake() => Time.timeScale = 1f;

        private void Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            _settings.ValidateValues();
            _score.Initialize(_settings.PointsPerWorldUnit, new PlayerPrefsHighScoreStore());
            // Cross-object initialization is explicit; it never relies on Awake order.
            if (!_player.Initialize(this, _settings)) return;
            _feedback.Initialize(_player, _settings);
            if (!_animation.Initialize(_player)) return;
            _world.Initialize(_settings, _player, _score);
            if (!_spawner.Initialize(this)) return;
            foreach (ParallaxLayer layer in _parallaxLayers)
            {
                if (!layer.Initialize(_camera)) return;
            }

            if (!_hud.Initialize(this) || !_menus.Initialize(this) || !_input.Initialize()) return;
            _ready = true;
            _player.Health.Died += EndRun;
            _input.SetGameplayEnabled(false);
            _animation.ResetRun();
            _animation.SetVisibleAnimationEnabled(false);
        }

        private void Update()
        {
            if (!_ready) return;
            // The System request wins even when Jump/Dash performed in this input update.
            if (_input.ConsumePause())
            {
                if (State == GameState.Playing) Pause();
                else if (State == GameState.Paused) Resume();
                _input.ClearGameplayRequests();
                return;
            }

            _input.ConsumeGameplay(out bool jump, out bool dash);
            if (State == GameState.Playing) _player.QueueCommands(jump, dash);
        }

        private void FixedUpdate()
        {
            if (!_ready || State != GameState.Playing) return;
            float deltaSeconds = Time.fixedDeltaTime;
            _player.Health.Tick(deltaSeconds);
            _player.PhysicsTick();
            _world.Tick(deltaSeconds);
            _score.SetDistance(_world.TotalDistance);
            _spawner.Tick(deltaSeconds, _world.StepDistance, _world.TotalDistance);
            if (State != GameState.Playing) return;
            foreach (ParallaxLayer layer in _parallaxLayers) layer.Scroll(_world.StepDistance);
            _feedback.Tick(deltaSeconds);
        }

        private void LateUpdate()
        {
            if (_ready && (State == GameState.Playing || State == GameState.GameOver)) _animation.Refresh();
        }

        public void StartRun()
        {
            if (_ready && State == GameState.MainMenu) ResetAndStartRun();
        }

        public void Pause()
        {
            if (!_ready || State != GameState.Playing) return;
            _input.SetGameplayEnabled(false);
            _player.ClearCommands();
            _player.Health.SetDamageEnabled(false);
            _score.SetScoringEnabled(false);
            _world.SetRunning(false);
            Time.timeScale = 0f;
            _score.FlushHighScore();
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (!_ready || State != GameState.Paused) return;
            _player.ClearCommands();
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            EnableRunCommands();
        }

        public void Restart()
        {
            if (_ready && (State == GameState.Paused || State == GameState.GameOver)) ResetAndStartRun();
        }

        public void ReturnToMainMenu()
        {
            if (!_ready || (State != GameState.Paused && State != GameState.GameOver)) return;
            StopRun();
            _world.ResetRun();
            _spawner.Clear();
            _feedback.ResetVisuals();
            _animation.SetVisibleAnimationEnabled(false);
            SetState(GameState.MainMenu);
        }

        public void Quit()
        {
            if (!_ready || State != GameState.MainMenu) return;
            _score.FlushHighScore();
            Application.Quit();
        }

        private void ResetAndStartRun()
        {
            StopRun();
            _spawner.ResetRun();
            foreach (ParallaxLayer layer in _parallaxLayers) layer.ResetRun();
            _player.ResetRun();
            _player.Health.ResetRun();
            _score.ResetRun();
            _world.ResetRun();
            _feedback.ResetVisuals();
            _player.BeginRun();
            _animation.ResetRun();
            SetState(GameState.Playing);
            EnableRunCommands();
        }

        private void EnableRunCommands()
        {
            _player.Health.SetDamageEnabled(true);
            _score.SetScoringEnabled(true);
            _world.SetRunning(true);
            _input.SetGameplayEnabled(true);
        }

        private void StopRun()
        {
            _input.SetGameplayEnabled(false);
            _player.StopRun();
            _spawner.StopMotion();
            _player.Health.SetDamageEnabled(false);
            _score.SetScoringEnabled(false);
            _world.SetRunning(false);
            Time.timeScale = 1f;
            _score.FlushHighScore();
        }

        private void EndRun()
        {
            if (State != GameState.Playing) return;
            StopRun();
            _feedback.ResetVisuals();
            _animation.Refresh();
            SetState(GameState.GameOver);
        }

        private void SetState(GameState state)
        {
            if (State == state) return;
            State = state;
            StateChanged?.Invoke(state);
        }

        private bool ValidateReferences()
        {
            bool valid = SetupValidation.Require(_settings, this, "Settings")
                & SetupValidation.Require(_input, this, "Input")
                & SetupValidation.Require(_player, this, "Player")
                & SetupValidation.Require(_animation, this, "Animation")
                & SetupValidation.Require(_feedback, this, "Feedback")
                & SetupValidation.Require(_world, this, "World")
                & SetupValidation.Require(_spawner, this, "Spawner")
                & SetupValidation.Require(_camera, this, "Camera")
                & SetupValidation.Require(_score, this, "Score")
                & SetupValidation.Require(_hud, this, "HUD")
                & SetupValidation.Require(_menus, this, "Menus");
            if (!SetupValidation.Check(_parallaxLayers != null && _parallaxLayers.Length >= 2,
                this, "Assign at least two Parallax Layers with different Speed Factors.")) return false;
            float firstFactor = _parallaxLayers[0] != null ? _parallaxLayers[0].SpeedFactor : 0f;
            bool hasDifferentFactor = false;
            foreach (ParallaxLayer layer in _parallaxLayers)
            {
                valid &= SetupValidation.Require(layer, this, "Parallax Layers entry");
                if (layer != null && !Mathf.Approximately(layer.SpeedFactor, firstFactor)) hasDifferentFactor = true;
            }

            valid &= SetupValidation.Check(hasDifferentFactor, this, "Parallax requires different Speed Factors.");
            return valid;
        }

        private void OnEnable()
        {
            if (_ready) _player.Health.Died += EndRun;
        }

        private void OnDisable()
        {
            if (_ready)
            {
                _player.Health.Died -= EndRun;
                StopRun();
                _world.ResetRun();
                _spawner.Clear();
                _feedback.ResetVisuals();
                _animation.SetVisibleAnimationEnabled(false);
                SetState(GameState.MainMenu);
            }

            Time.timeScale = 1f;
        }

        private void OnApplicationPause(bool paused)
        {
            if (_ready && paused)
            {
                if (State == GameState.Playing) Pause();
                _score.FlushHighScore();
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (_ready && !focused)
            {
                if (State == GameState.Playing) Pause();
                _score.FlushHighScore();
            }
        }

        private void OnApplicationQuit()
        {
            if (_score != null) _score.FlushHighScore();
        }
    }
}
