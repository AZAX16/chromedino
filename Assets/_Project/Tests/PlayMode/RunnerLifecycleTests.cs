#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ChromeDino.Tests
{
    public sealed partial class RunnerIntegrationTests
    {
        [UnityTest]
        public IEnumerator DestroyedPlayerBeforeSessionDisableStillCleansUp()
        {
            _spawner.Tick(5f, 0f, _world.TotalDistance);
            Assert.That(_spawner.ActiveCount, Is.GreaterThan(0));
            _session.Pause();

            // Scene unload does not promise that the session outlives its
            // referenced GameObjects. Reproduce the player-first order.
            Object.Destroy(_player.gameObject);
            yield return null;
            Assert.That(_player == null, Is.True);

            _session.enabled = false;
            yield return null;

            LogAssert.NoUnexpectedReceived();
            Assert.That(_session.State, Is.EqualTo(GameState.MainMenu));
            Assert.That(_spawner.ActiveCount, Is.Zero);
            Assert.That(_world.EffectiveSpeed, Is.Zero);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator SessionDisableAndReenablePreservesFreshRun()
        {
            Color originalColor = _sprite.color;
            _spawner.Tick(5f, 0f, _world.TotalDistance);
            _player.QueueCommands(false, true);
            yield return new WaitForFixedUpdate();
            Assert.That(_player.IsDashing, Is.True);

            _root.SetActive(false);
            Assert.That(_session.State, Is.EqualTo(GameState.MainMenu));
            Assert.That(_spawner.ActiveCount, Is.Zero);
            Assert.That(_player.GetComponent<Rigidbody2D>().simulated, Is.False);
            Assert.That(_player.IsDashing, Is.False);
            Assert.That(_sprite.color, Is.EqualTo(originalColor));

            _root.SetActive(true);
            _session.StartRun();
            yield return new WaitForFixedUpdate();

            Assert.That(_session.State, Is.EqualTo(GameState.Playing));
            Assert.That(_player.Health.CurrentLives, Is.EqualTo(3));
            Assert.That(_player.GetComponent<Rigidbody2D>().simulated, Is.True);
            Assert.That(_world.EffectiveSpeed, Is.EqualTo(_settings.BaseSpeed));
            Assert.That(_animator.enabled, Is.True);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
