using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace ChromeDino.Tests
{
    public sealed class InputRoutingTests
    {
        private GameObject _root;
        private RunnerInputReader _reader;
        private InputActionAsset _actions;
        private Keyboard _keyboard;
        private InputSettings.UpdateMode _previousMode;
        private InputTestFixture _inputFixture;
        private readonly List<InputActionReference> _references = new List<InputActionReference>();

        [SetUp]
        public void SetUp()
        {
            _inputFixture = new InputTestFixture();
            _inputFixture.Setup();
            _previousMode = InputSystem.settings.updateMode;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _actions = InputActionAsset.FromJson(File.ReadAllText(Path.Combine(Application.dataPath,
                "_Project/Input/RunnerControls.inputactions")));
            _actions.devices = new InputDevice[] { _keyboard };
            _root = new GameObject("Input routing test");
            _reader = _root.AddComponent<RunnerInputReader>();
            SetAction("_jump", "Gameplay/Jump");
            SetAction("_dash", "Gameplay/Dash");
            SetAction("_pause", "System/Pause");
            Assert.That(_reader.Initialize(), Is.True);
            InputSystem.Update();
            _reader.SetGameplayEnabled(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            foreach (InputActionReference reference in _references) Object.DestroyImmediate(reference);
            _references.Clear();
            Object.DestroyImmediate(_actions);
            InputSystem.RemoveDevice(_keyboard);
            InputSystem.settings.updateMode = _previousMode;
            _inputFixture.TearDown();
        }

        [Test]
        public void HoldGeneratesOneJumpAndRequiresNewPress()
        {
            Send(Key.Space);
            AssertRequests(true, false);
            InputSystem.Update();
            AssertRequests(false, false);
            Send();
            Send(Key.Space);
            AssertRequests(true, false);
        }

        [Test]
        public void HoldingThroughPauseOrRestartCannotLeakGameplay()
        {
            Send(Key.Space, Key.LeftShift);
            _reader.SetGameplayEnabled(false);
            AssertRequests(false, false);
            _reader.SetGameplayEnabled(true);
            InputSystem.Update();
            AssertRequests(false, false);
            Send();
            Send(Key.Space, Key.LeftShift);
            AssertRequests(true, true);
        }

        [Test]
        public void SystemPauseSurvivesGameplayGateAndDoesNotInvokeUiCancel()
        {
            _reader.SetGameplayEnabled(false);
            int cancels = 0;
            _actions.FindAction("UI/Cancel").performed += _ => cancels++;
            _actions.FindActionMap("UI").Enable();
            Send(Key.Escape);
            Assert.That(_reader.ConsumePause(), Is.True);
            Assert.That(_reader.ConsumePause(), Is.False);
            Assert.That(cancels, Is.Zero);
            Assert.That(_actions.FindAction("UI/Submit").bindings[0].path, Does.Not.Contain("space"));
        }

        [Test]
        public void DisablingReaderLeavesUiActionsEnabledAndResubscribesOnce()
        {
            _actions.FindActionMap("UI").Enable();
            _reader.enabled = false;
            Assert.That(_actions.FindAction("UI/Point").enabled, Is.True);
            _reader.enabled = true;
            _reader.SetGameplayEnabled(true);
            Send(Key.LeftShift);
            AssertRequests(false, true);
            AssertRequests(false, false);
        }

        private void SetAction(string field, string path)
        {
            InputActionReference reference = InputActionReference.Create(_actions.FindAction(path, true));
            _references.Add(reference);
            typeof(RunnerInputReader).GetField(field,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(_reader, reference);
        }

        private void Send(params Key[] keys)
        {
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(keys));
            InputSystem.Update();
        }

        private void AssertRequests(bool jump, bool dash)
        {
            _reader.ConsumeGameplay(out bool actualJump, out bool actualDash);
            Assert.That(actualJump, Is.EqualTo(jump));
            Assert.That(actualDash, Is.EqualTo(dash));
        }
    }
}
