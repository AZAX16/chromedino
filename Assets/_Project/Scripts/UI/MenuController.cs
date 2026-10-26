using UnityEngine;
using UnityEngine.EventSystems;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private GameObject _mainMenuFirstSelected;
        [SerializeField] private GameObject _pauseMenuFirstSelected;
        [SerializeField] private GameObject _gameOverFirstSelected;

        private GameSession _session;
        private bool _initialized;

        public bool Initialize(GameSession session)
        {
            bool valid = SetupValidation.Require(_mainMenuPanel, this, "Main Menu Panel")
                & SetupValidation.Require(_pauseMenuPanel, this, "Pause Menu Panel")
                & SetupValidation.Require(_gameOverPanel, this, "Game Over Panel")
                & SetupValidation.Require(_hudPanel, this, "HUD Panel")
                & SetupValidation.Require(_eventSystem, this, "Event System")
                & SetupValidation.Require(_mainMenuFirstSelected, this, "Main Menu First Selected")
                & SetupValidation.Require(_pauseMenuFirstSelected, this, "Pause Menu First Selected")
                & SetupValidation.Require(_gameOverFirstSelected, this, "Game Over First Selected");
            if (!valid) return false;
            valid &= SetupValidation.Check(_mainMenuPanel != _pauseMenuPanel
                && _mainMenuPanel != _gameOverPanel && _pauseMenuPanel != _gameOverPanel,
                this, "Use three separate menu panels.");
            if (!valid) return false;
            _session = session;
            _initialized = true;
            if (isActiveAndEnabled) _session.StateChanged += ShowState;
            ShowState(session.State);
            return true;
        }

        private void ShowState(GameState state)
        {
            _eventSystem.SetSelectedGameObject(null);
            _mainMenuPanel.SetActive(state == GameState.MainMenu);
            _pauseMenuPanel.SetActive(state == GameState.Paused);
            _gameOverPanel.SetActive(state == GameState.GameOver);
            _hudPanel.SetActive(state != GameState.MainMenu);
            GameObject selected = state == GameState.MainMenu ? _mainMenuFirstSelected
                : state == GameState.Paused ? _pauseMenuFirstSelected
                : state == GameState.GameOver ? _gameOverFirstSelected : null;
            _eventSystem.SetSelectedGameObject(selected);
        }

        private void OnEnable()
        {
            if (!_initialized) return;
            _session.StateChanged += ShowState;
            ShowState(_session.State);
        }

        private void OnDisable()
        {
            if (_initialized) _session.StateChanged -= ShowState;
        }
    }
}
