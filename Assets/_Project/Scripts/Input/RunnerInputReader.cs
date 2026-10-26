using UnityEngine;
using UnityEngine.InputSystem;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class RunnerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference _jump;
        [SerializeField] private InputActionReference _dash;
        [SerializeField] private InputActionReference _pause;

        private bool _initialized;
        private bool _subscribed;
        private bool _acceptGameplay;
        private bool _jumpArmed;
        private bool _dashArmed;
        private bool _jumpRequested;
        private bool _dashRequested;
        private bool _pauseRequested;
        private bool _enabledJump;
        private bool _enabledDash;
        private bool _enabledPause;

        public bool Initialize()
        {
            bool valid = ValidateAction(_jump, "Gameplay", "Jump")
                & ValidateAction(_dash, "Gameplay", "Dash")
                & ValidateAction(_pause, "System", "Pause");
            if (!valid)
            {
                enabled = false;
                return false;
            }

            _initialized = true;
            Subscribe();
            return true;
        }

        public void SetGameplayEnabled(bool enabled)
        {
            _acceptGameplay = enabled;
            ClearGameplayRequests();
            _jumpArmed = enabled && !_jump.action.IsPressed();
            _dashArmed = enabled && !_dash.action.IsPressed();
        }

        public bool ConsumePause()
        {
            bool requested = _pauseRequested;
            _pauseRequested = false;
            return requested;
        }

        public void ConsumeGameplay(out bool jump, out bool dash)
        {
            jump = _acceptGameplay && _jumpRequested;
            dash = _acceptGameplay && _dashRequested;
            ClearGameplayRequests();
        }

        public void ClearGameplayRequests()
        {
            _jumpRequested = false;
            _dashRequested = false;
        }

        private bool ValidateAction(InputActionReference reference, string map, string actionName)
        {
            return SetupValidation.Require(reference, this, actionName + " Action")
                && SetupValidation.Check(reference.action != null
                    && reference.action.type == InputActionType.Button
                    && reference.action.name == actionName
                    && reference.action.actionMap.name == map,
                    this, $"Assign RunnerControls/{map}/{actionName} (Button).");
        }

        private void OnEnable()
        {
            if (_initialized)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            _acceptGameplay = false;
            _pauseRequested = false;
            ClearGameplayRequests();
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            _jump.action.performed += OnJump;
            _jump.action.canceled += OnJumpReleased;
            _dash.action.performed += OnDash;
            _dash.action.canceled += OnDashReleased;
            _pause.action.performed += OnPause;
            // Keep these actions active in menus to observe releases. Only this reader
            // interprets gameplay presses; the UI module owns the separate UI actions.
            _enabledJump = !_jump.action.enabled;
            _enabledDash = !_dash.action.enabled;
            _enabledPause = !_pause.action.enabled;
            _jump.action.wantsInitialStateCheck = true;
            _dash.action.wantsInitialStateCheck = true;
            _jump.action.Enable();
            _dash.action.Enable();
            _pause.action.Enable();
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            _jump.action.performed -= OnJump;
            _jump.action.canceled -= OnJumpReleased;
            _dash.action.performed -= OnDash;
            _dash.action.canceled -= OnDashReleased;
            _pause.action.performed -= OnPause;
            if (_enabledJump) _jump.action.Disable();
            if (_enabledDash) _dash.action.Disable();
            if (_enabledPause) _pause.action.Disable();
            _subscribed = false;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (_acceptGameplay && _jumpArmed) _jumpRequested = true;
            _jumpArmed = false;
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            if (_acceptGameplay && _dashArmed) _dashRequested = true;
            _dashArmed = false;
        }

        private void OnJumpReleased(InputAction.CallbackContext context) => _jumpArmed = true;
        private void OnDashReleased(InputAction.CallbackContext context) => _dashArmed = true;
        private void OnPause(InputAction.CallbackContext context) => _pauseRequested = true;
    }
}
