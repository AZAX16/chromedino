using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _highScoreText;
        [SerializeField] private Slider _healthBar;
        [SerializeField] private TMP_Text _finalScoreText;
        [SerializeField] private TMP_Text _gameOverHighScoreText;

        private GameSession _session;
        private bool _initialized;

        public bool Initialize(GameSession session)
        {
            bool valid = SetupValidation.Require(_scoreText, this, "Score Text")
                & SetupValidation.Require(_highScoreText, this, "High Score Text")
                & SetupValidation.Require(_healthBar, this, "Health Bar")
                & SetupValidation.Require(_finalScoreText, this, "Final Score Text")
                & SetupValidation.Require(_gameOverHighScoreText, this, "Game Over High Score Text");
            if (!valid) return false;
            _session = session;
            _healthBar.minValue = 0f;
            _healthBar.maxValue = RunnerSettings.MaximumLives;
            _healthBar.wholeNumbers = true;
            _healthBar.interactable = false;
            _initialized = true;
            if (isActiveAndEnabled) Subscribe();
            RefreshScore();
            RefreshHealth();
            return true;
        }

        private void RefreshScore()
        {
            _scoreText.text = $"Score: {_session.Score.CurrentScore}";
            _highScoreText.text = $"High Score: {_session.Score.HighScore}";
            _finalScoreText.text = $"Final Score: {_session.Score.CurrentScore}";
            _gameOverHighScoreText.text = $"High Score: {_session.Score.HighScore}";
        }

        private void RefreshHealth() => _healthBar.SetValueWithoutNotify(_session.Player.Health.CurrentLives);

        private void Subscribe()
        {
            _session.Score.ScoreChanged += RefreshScore;
            _session.Player.Health.HealthChanged += RefreshHealth;
        }

        private void OnEnable()
        {
            if (!_initialized) return;
            Subscribe();
            RefreshScore();
            RefreshHealth();
        }

        private void OnDisable()
        {
            if (!_initialized) return;
            _session.Score.ScoreChanged -= RefreshScore;
            _session.Player.Health.HealthChanged -= RefreshHealth;
        }
    }
}
