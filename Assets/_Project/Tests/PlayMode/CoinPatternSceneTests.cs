#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ChromeDino.Tests
{
    // Runs the authored Game scene, real capsule, prefab triggers, input reader,
    // session loop, and Unity physics. Nothing is saved back to the scene/assets.
    public sealed class CoinPatternSceneTests
    {
        private Scene _scene;
        private GameSession _session;
        private SpawnController _spawner;
        private WorldScrollController _world;
        private RunnerSettings _settings;
        private Keyboard _keyboard;
        private InputTestFixture _inputFixture;
        private InputSettings.UpdateMode _previousMode;
        private float _previousTimeScale;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousTimeScale = Time.timeScale;
            _inputFixture = new InputTestFixture();
            _inputFixture.Setup();
            _previousMode = InputSystem.settings.updateMode;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            _keyboard = InputSystem.AddDevice<Keyboard>();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Game.unity",
                new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath("Assets/Scenes/Game.unity");
            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                _session = root.GetComponentInChildren<GameSession>();
                if (_session != null) break;
            }
            yield return null; // Let the production Start bootstrap finish.
            Assert.That(_session, Is.Not.Null);
            Assert.That(_session.State, Is.EqualTo(GameState.MainMenu));
            _spawner = _session.GetComponent<SpawnController>();
            _world = _session.GetComponent<WorldScrollController>();
            _settings = Object.Instantiate(_session.Settings);
            Set(_session, "_settings", _settings);
            Assert.That(_spawner.Initialize(_session), Is.True);
            _session.Score.Initialize(_settings.PointsPerWorldUnit, new MemoryStore());
            InputSystem.Update();
            _session.StartRun();
            Assert.That(_session.State, Is.EqualTo(GameState.Playing));
            Assert.That(_session.Player.GetComponent<CapsuleCollider2D>(), Is.Not.Null);
            // Let the authored player settle onto the authored floor.
            for (int i = 0; i < 5; i++) yield return new WaitForFixedUpdate();
            Assert.That(_session.Player.IsGrounded, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Stop the session while its referenced player still exists. Scene
            // unload can otherwise destroy roots in a different order.
            if (_session != null) _session.enabled = false;
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            if (_settings != null) Object.DestroyImmediate(_settings);
            InputSystem.RemoveDevice(_keyboard);
            InputSystem.settings.updateMode = _previousMode;
            Time.timeScale = _previousTimeScale;
            _inputFixture.TearDown();
        }

        [UnityTest] public IEnumerator GroundRowCollects150WithoutJump() => PlayPattern(CoinPatternKind.Row, 0);
        [UnityTest] public IEnumerator ArcCollectsWithOneGroundedJumpAtSix() => PlayPattern(CoinPatternKind.Arc, 0);
        [UnityTest] public IEnumerator ArcCollectsWithOneGroundedJumpAtSevenPointFive() => PlayPattern(CoinPatternKind.Arc, 500);
        [UnityTest] public IEnumerator ArcSurvivesDifficultyCrossingOnFirstCoin() => PlayPattern(CoinPatternKind.Arc, 450);
        [UnityTest] public IEnumerator EarlyArcSurvivesDifficultyChangeOnApproach() => PlayPattern(CoinPatternKind.Arc, 0, false, true);
        [UnityTest] public IEnumerator RepeatedCoinsRequireThreeGroundedPressesAtSix() => PlayPattern(CoinPatternKind.RepeatedJumps, 0);
        [UnityTest] public IEnumerator RepeatedCoinsRequireThreeGroundedPressesAtSevenPointFive() => PlayPattern(CoinPatternKind.RepeatedJumps, 500);
        [UnityTest] public IEnumerator RepeatedCoinsSurviveDifficultyCrossingOnFirstCoin() => PlayPattern(CoinPatternKind.RepeatedJumps, 450);
        [UnityTest] public IEnumerator EarlyRepeatedSequenceSurvivesDifficultyChangeOnApproach() => PlayPattern(CoinPatternKind.RepeatedJumps, 0, false, true);
        [UnityTest] public IEnumerator HoldingSpaceThroughRepeatedSequenceOnlyJumpsAndCollectsOnce() => PlayPattern(CoinPatternKind.RepeatedJumps, 0, true);

        [UnityTest]
        public IEnumerator HardestDashDuringArcAllowsLandingAndAvoidingNextGroundHazard()
        {
            _session.Score.AddItemPoints(500);
            Set(_settings, "_singleCoinWeight", 0f);
            Set(_settings, "_coinRowWeight", 0f);
            Set(_settings, "_coinArcWeight", 1f);
            Set(_settings, "_repeatedJumpCoinWeight", 0f);
            _spawner.ResetRun(52);
            // Begin at a coin opportunity; subsequent ground/air rotation and all
            // movement/clearance use the production session loop without offsets.
            Set(_spawner, "_nextKind", 2);
            _spawner.Tick(5f, 0f, _world.TotalDistance);
            Transform spawned = Get<Transform>(_spawner, "_spawnedObjectsRoot");
            CoinCollectible first = spawned.GetComponentInChildren<CoinCollectible>();
            GroundObstacle ground = null;
            float apex = _settings.JumpVelocity / (-Physics2D.gravity.y * _settings.GravityScale);
            bool arcPressed = false, groundPressed = false, sawFifteen = false, safeReaction = false;
            int frame;
            for (frame = 0; frame < 1000; frame++)
            {
                if (ground == null) ground = spawned.GetComponentInChildren<GroundObstacle>();
                float playerX = _session.Player.transform.position.x;
                if (!arcPressed && first != null && first.transform.position.x - playerX
                    <= _world.NormalSpeed * apex * (1f - _settings.CoinArcHalfDurationFraction))
                {
                    Send(Key.Space, Key.LeftShift);
                    arcPressed = true;
                    yield return null;
                }
                else if (!groundPressed && ground != null && ground.transform.position.x - playerX <= _world.NormalSpeed * apex)
                {
                    Assert.That(_session.Player.IsGrounded, Is.True);
                    Send(Key.Space);
                    groundPressed = true;
                    yield return null;
                }
                else Send();

                if (!safeReaction && !groundPressed && ground != null && ground.transform.position.x - playerX
                    <= _settings.MaximumSpeed * _settings.ReactionSeconds)
                {
                    Assert.That(_session.Player.IsGrounded, Is.True, "The arc/dash must leave a grounded reaction window.");
                    safeReaction = true;
                }
                yield return new WaitForFixedUpdate();
                sawFifteen |= Mathf.Approximately(_world.EffectiveSpeed, 15f);
                if (groundPressed && ground != null && ground.transform.position.x < playerX - 1f && _session.Player.IsGrounded) break;
            }
            Assert.That(frame, Is.LessThan(1000));
            Assert.That(arcPressed && groundPressed && safeReaction && sawFifteen, Is.True);
            Assert.That(_session.Player.Health.CurrentLives, Is.EqualTo(3));
        }

        private IEnumerator PlayPattern(CoinPatternKind kind, int initialPoints, bool hold = false, bool changeOnApproach = false)
        {
            _session.Score.AddItemPoints(initialPoints);
            Set(_settings, "_singleCoinWeight", 0f);
            Set(_settings, "_coinRowWeight", kind == CoinPatternKind.Row ? 1f : 0f);
            Set(_settings, "_coinArcWeight", kind == CoinPatternKind.Arc ? 1f : 0f);
            Set(_settings, "_repeatedJumpCoinWeight", kind == CoinPatternKind.RepeatedJumps ? 1f : 0f);
            _spawner.ResetRun(41);
            Transform spawned = Get<Transform>(_spawner, "_spawnedObjectsRoot");
            // Advance to one coin opportunity, removing the two earlier hazards
            // before contact. A distant reservation prevents unrelated later spawns.
            for (int i = 0; i < 3; i++)
            {
                _spawner.Tick(5f, 0f, i * 100d);
                if (i < 2)
                    foreach (Interactable item in spawned.GetComponentsInChildren<Interactable>()) item.Despawn();
            }
            CoinCollectible[] coins = spawned.GetComponentsInChildren<CoinCollectible>();
            Assert.That(coins.Length, Is.EqualTo(3));
            if (changeOnApproach)
            {
                _session.Score.AddItemPoints(500);
                initialPoints += 500;
            }

            float apexSeconds = _settings.JumpVelocity / (-Physics2D.gravity.y * _settings.GravityScale);
            float arcSpan = coins[1].transform.position.x - coins[0].transform.position.x;
            int presses = 0;
            int launches = 0;
            float firstPressTime = -1f;
            float lastPressTime = -1f;
            bool wasGrounded = _session.Player.IsGrounded;
            bool released = true;
            int frame;
            for (frame = 0; frame < 1200; frame++)
            {
                CoinCollectible next = null;
                foreach (CoinCollectible coin in coins)
                    if (coin != null && coin.gameObject.activeSelf && coin.transform.position.x > _session.Player.transform.position.x)
                    { next = coin; break; }
                bool canPress = kind != CoinPatternKind.Row && (kind != CoinPatternKind.Arc || presses == 0)
                    && (!hold || presses == 0);
                if (next != null && canPress && released && _session.Player.IsGrounded)
                {
                    float leadSeconds = kind == CoinPatternKind.Arc ? apexSeconds - arcSpan / _world.NormalSpeed : apexSeconds;
                    if (next.transform.position.x - _session.Player.transform.position.x <= _world.NormalSpeed * leadSeconds)
                    {
                        Assert.That(_session.Player.IsGrounded, Is.True, "Every launch must begin on the floor.");
                        Send(Key.Space);
                        if (presses == 0) firstPressTime = Time.fixedTime;
                        lastPressTime = Time.fixedTime;
                        presses++;
                        released = false;
                        yield return null; // The real session Update consumes the real input request.
                    }
                }
                else if (!released && !hold)
                {
                    Send();
                    released = true;
                }
                yield return new WaitForFixedUpdate();
                if (wasGrounded && !_session.Player.IsGrounded) launches++;
                wasGrounded = _session.Player.IsGrounded;
                bool allGone = true;
                foreach (CoinCollectible coin in coins)
                    if (coin != null && coin.gameObject.activeSelf) allGone = false;
                if (allGone) break;
            }
            Assert.That(frame, Is.LessThan(1200), "Every coin must collect or despawn normally.");
            int expectedJumps = kind == CoinPatternKind.Row ? 0 : kind == CoinPatternKind.Arc || hold ? 1 : 3;
            Assert.That(presses, Is.EqualTo(expectedJumps));
            Assert.That(launches, Is.EqualTo(expectedJumps), "Holding Space must never auto-jump after landing.");
            if (kind == CoinPatternKind.RepeatedJumps && !hold)
                Assert.That(lastPressTime - firstPressTime, Is.LessThan(2.9f),
                    "The three grounded jumps should form a compact sequence at either normal speed.");
            int itemPoints = _session.Score.CurrentScore - Mathf.FloorToInt((float)_session.Score.Distance * _settings.PointsPerWorldUnit);
            Assert.That(itemPoints - initialPoints, Is.EqualTo(hold ? 50 : 150));
            Assert.That(_session.Player.Health.CurrentLives, Is.EqualTo(3));
            if (initialPoints >= 450)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(_world.NormalSpeed, Is.EqualTo(7.5f));
            }
        }

        private void Send(params Key[] keys)
        {
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(keys));
            InputSystem.Update();
        }
        private static T Get<T>(object target, string field) => (T)target.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        private static void Set(object target, string field, object value) => target.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        private sealed class MemoryStore : IHighScoreStore
        {
            public int Load() => 0;
            public void Save(int highScore) { }
        }
    }
}
#endif
