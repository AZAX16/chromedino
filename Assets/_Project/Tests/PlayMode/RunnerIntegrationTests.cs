#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ChromeDino.Tests
{
    // This disposable fixture uses Unity APIs, never scene/prefab YAML. Its inactive
    // Canvas lets model/HUD binding run without importing production font/art assets.
    public sealed partial class RunnerIntegrationTests
    {
        private GameObject _root;
        private GameSession _session;
        private PlayerController _player;
        private WorldScrollController _world;
        private SpawnController _spawner;
        private RunnerInputReader _input;
        private RunnerSettings _settings;
        private Animator _animator;
        private SpriteRenderer _sprite;
        private InputActionAsset _actions;
        private Keyboard _keyboard;
        private InputSettings.UpdateMode _previousInputMode;
        private InputTestFixture _inputFixture;
        private float _previousTimeScale;
        private readonly List<Object> _ownedAssets = new List<Object>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousTimeScale = Time.timeScale;
            _inputFixture = new InputTestFixture();
            _inputFixture.Setup();
            _previousInputMode = InputSystem.settings.updateMode;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _actions = InputActionAsset.FromJson(File.ReadAllText(Path.Combine(Application.dataPath,
                "_Project/Input/RunnerControls.inputactions")));
            _actions.devices = new InputDevice[] { _keyboard };
            _ownedAssets.Add(_actions);
            _settings = ScriptableObject.CreateInstance<RunnerSettings>();
            _ownedAssets.Add(_settings);
            _root = new GameObject("Disposable runner integration fixture");
            _root.SetActive(false);
            _session = _root.AddComponent<GameSession>();
            _world = _root.AddComponent<WorldScrollController>();
            _spawner = _root.AddComponent<SpawnController>();
            _input = _root.AddComponent<RunnerInputReader>();
            ScoreTracker score = _root.AddComponent<ScoreTracker>();
            Camera camera = Child("Camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.aspect = 16f / 9f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            GameObject ground = Child("Ground");
            ground.layer = 8;
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(100f, 1f);
            GameObject playerObject = Child("Player");
            playerObject.layer = 9;
            playerObject.transform.position = new Vector3(-5f, 0.5f, 0f);
            Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            BoxCollider2D playerCollider = playerObject.AddComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(0.7f, 1f);
            _player = playerObject.AddComponent<PlayerController>();
            Transform feet = Child("Feet", playerObject.transform).transform;
            feet.localPosition = new Vector3(0f, -0.5f, 0f);
            Set(_player, "_bodyCollider", playerCollider);
            Set(_player, "_feetCheck", feet);
            Set(_player, "_groundLayers", (LayerMask)(1 << 8));
            GameObject visual = Child("Visual", playerObject.transform);
            _sprite = visual.AddComponent<SpriteRenderer>();
            _sprite.color = new Color(0.4f, 0.7f, 0.2f, 1f);
            _animator = visual.AddComponent<Animator>();
            _animator.runtimeAnimatorController = CreateController();
            PlayerAnimationController animation = visual.AddComponent<PlayerAnimationController>();
            PlayerHitFeedback feedback = visual.AddComponent<PlayerHitFeedback>();

            Set(_input, "_jump", ActionReference("Gameplay/Jump"));
            Set(_input, "_dash", ActionReference("Gameplay/Dash"));
            Set(_input, "_pause", ActionReference("System/Pause"));
            Set(_spawner, "_camera", camera);
            Set(_spawner, "_groundObstaclePrefab", Prefab<GroundObstacle>(new Vector2(0.5f, 0.8f)));
            Set(_spawner, "_aerialObstaclePrefab", Prefab<AerialObstacle>(new Vector2(0.7f, 0.5f)));
            Set(_spawner, "_coinPrefab", Prefab<CoinCollectible>(new Vector2(0.3f, 0.3f)));
            Set(_spawner, "_groundSpawnHeight", Height("Ground height", 0.4f));
            Set(_spawner, "_aerialSpawnHeight", Height("Air height", 2f));
            Set(_spawner, "_coinSpawnHeight", Height("Coin height", 1.2f));
            Set(_spawner, "_spawnedObjectsRoot", Child("Spawned").transform);

            ParallaxLayer[] layers = new ParallaxLayer[2];
            for (int i = 0; i < layers.Length; i++)
            {
                GameObject layerObject = Child("Parallax " + i);
                layers[i] = layerObject.AddComponent<ParallaxLayer>();
                Transform first = Child("Tile 0", layerObject.transform).transform;
                Transform second = Child("Tile 1", layerObject.transform).transform;
                first.position = new Vector3(-10f, 0f, 0f);
                second.position = new Vector3(10f, 0f, 0f);
                Set(layers[i], "_tiles", new[] { first, second });
                Set(layers[i], "_speedFactor", i == 0 ? 0.2f : 0.5f);
            }

            GameObject canvas = Child("Inactive test Canvas");
            canvas.SetActive(false);
            HudController hud = _root.AddComponent<HudController>();
            Set(hud, "_scoreText", Text("Score", canvas.transform));
            Set(hud, "_highScoreText", Text("High score", canvas.transform));
            Set(hud, "_finalScoreText", Text("Final score", canvas.transform));
            Set(hud, "_gameOverHighScoreText", Text("Final high score", canvas.transform));
            Set(hud, "_healthBar", Child("Health bar", canvas.transform).AddComponent<Slider>());
            EventSystem eventSystem = Child("EventSystem").AddComponent<EventSystem>();
            eventSystem.enabled = false;
            MenuController menus = _root.AddComponent<MenuController>();
            GameObject main = Child("Main", canvas.transform);
            GameObject pause = Child("Pause", canvas.transform);
            GameObject gameOver = Child("GameOver", canvas.transform);
            Set(menus, "_mainMenuPanel", main);
            Set(menus, "_pauseMenuPanel", pause);
            Set(menus, "_gameOverPanel", gameOver);
            Set(menus, "_hudPanel", Child("HUD", canvas.transform));
            Set(menus, "_eventSystem", eventSystem);
            Set(menus, "_mainMenuFirstSelected", Child("Play", main.transform));
            Set(menus, "_pauseMenuFirstSelected", Child("Resume", pause.transform));
            Set(menus, "_gameOverFirstSelected", Child("Restart", gameOver.transform));

            Set(_session, "_settings", _settings);
            Set(_session, "_input", _input);
            Set(_session, "_player", _player);
            Set(_session, "_animation", animation);
            Set(_session, "_feedback", feedback);
            Set(_session, "_world", _world);
            Set(_session, "_spawner", _spawner);
            Set(_session, "_parallaxLayers", layers);
            Set(_session, "_camera", camera);
            Set(_session, "_score", score);
            Set(_session, "_hud", hud);
            Set(_session, "_menus", menus);
            _root.SetActive(true);
            Physics2D.SyncTransforms();
            yield return null;
            InputSystem.Update();
            Assert.That(_session.State, Is.EqualTo(GameState.MainMenu));
            _session.Score.Initialize(_settings.PointsPerWorldUnit, new TestHighScoreStore());
            _session.StartRun();
            Assert.That(_session.State, Is.EqualTo(GameState.Playing), "Session bootstrap must succeed.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_root);
            yield return null;
            foreach (Object asset in _ownedAssets) Object.DestroyImmediate(asset);
            _ownedAssets.Clear();
            InputSystem.RemoveDevice(_keyboard);
            InputSystem.settings.updateMode = _previousInputMode;
            Time.timeScale = _previousTimeScale;
            _inputFixture.TearDown();
        }

        [UnityTest]
        public IEnumerator SpaceJumpsOnceAndDashReturnsToBaseSpeed()
        {
            Send(Key.Space, Key.LeftShift);
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.That(_player.VerticalVelocity, Is.GreaterThan(0f));
            Assert.That(_player.IsGrounded, Is.False);
            Assert.That(_world.EffectiveSpeed, Is.EqualTo(_world.DashSpeed));
            Assert.That(_world.DashSpeed, Is.EqualTo(12f));
            Assert.That(_settings.MaximumSpeed, Is.EqualTo(15f));
            float originalVelocity = _player.VerticalVelocity;
            yield return new WaitForFixedUpdate();
            Assert.That(_player.VerticalVelocity, Is.LessThan(originalVelocity));
            yield return new WaitForSeconds(_settings.DashDurationSeconds + 0.05f);
            Assert.That(_player.IsDashing, Is.False);
            Assert.That(_world.EffectiveSpeed, Is.EqualTo(_settings.BaseSpeed));
            Send();
            Send(Key.LeftShift);
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.That(_player.IsDashing, Is.False, "Cooldown must reject a fresh dash press.");
        }

        [UnityTest]
        public IEnumerator PauseFreezesDistanceTimersPhysicsAndTintThenRestartRestoresRun()
        {
            _session.Score.AddItemPoints(500);
            TickAndAssertSpeed(7.5f, Time.fixedDeltaTime);
            Color originalColor = _sprite.color;
            _player.Health.TryTakeDamage();
            _session.Pause();
            double distance = _world.TotalDistance;
            float countdown = _spawner.CountdownSeconds;
            Vector3 position = _player.transform.position;
            Color hitColor = _sprite.color;
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(_world.TotalDistance, Is.EqualTo(distance));
            Assert.That(_spawner.CountdownSeconds, Is.EqualTo(countdown));
            Assert.That(_player.transform.position, Is.EqualTo(position));
            Assert.That(_sprite.color, Is.EqualTo(hitColor));
            Assert.That(_player.Health.IsRecovering, Is.True);
            Assert.That(_world.DifficultyMultiplier, Is.EqualTo(1.25f));
            Assert.That(_world.EffectiveSpeed, Is.Zero);
            _session.Resume();
            yield return new WaitForFixedUpdate();
            _session.Pause();
            _session.Restart();
            Assert.That(_player.Health.CurrentLives, Is.EqualTo(3));
            Assert.That(_sprite.color, Is.EqualTo(originalColor));
            Assert.That(_session.Score.CurrentScore, Is.Zero);
            Assert.That(_world.DifficultyMultiplier, Is.EqualTo(1f));
            Assert.That(_world.EffectiveSpeed, Is.EqualTo(6f));
            Assert.That(_session.Score.HighScore, Is.GreaterThanOrEqualTo(500));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator EncountersAndCoinAreConsumedOnceEvenDuringInvulnerability()
        {
            GroundObstacle first = Live<GroundObstacle>();
            AerialObstacle ignored = Live<AerialObstacle>();
            CoinCollectible coin = Live<CoinCollectible>();
            Assert.That(first.TryInteract(_player), Is.True);
            Assert.That(first.TryInteract(_player), Is.False);
            Assert.That(_player.Health.CurrentLives, Is.EqualTo(2));
            Assert.That(ignored.TryInteract(_player), Is.True);
            int before = _session.Score.CurrentScore;
            Assert.That(coin.TryInteract(_player), Is.True);
            Assert.That(coin.gameObject.activeSelf, Is.False);
            Assert.That(coin.TryInteract(_player), Is.False);
            Assert.That(_session.Score.CurrentScore, Is.EqualTo(before + 50));
            _player.Health.Tick(_settings.InvulnerabilitySeconds + 0.01f);
            Assert.That(ignored.TryInteract(_player), Is.False);
            Assert.That(_player.Health.CurrentLives, Is.EqualTo(2));
            yield return null;
        }

        [UnityTest]
        public IEnumerator DifficultyChangesAt499To500AndDoesNotMultiplyAgain()
        {
            _session.Score.SetDistance(499d);
            TickAndAssertSpeed(6f, 0.02f);
            Assert.That(_world.DifficultyMultiplier, Is.EqualTo(1f));
            _session.Score.SetDistance(500d);
            TickAndAssertSpeed(7.5f, 0.02f);
            Assert.That(_world.DifficultyMultiplier, Is.EqualTo(1.25f));
            for (int i = 0; i < 5; i++)
            {
                _session.Score.AddItemPoints(500);
                TickAndAssertSpeed(7.5f, 0.02f);
            }

            Assert.That(_settings.BaseSpeed, Is.EqualTo(6f));
            Assert.That(_settings.DashSpeedMultiplier, Is.EqualTo(2f));
            Assert.That(_settings.HarderSpeedMultiplier, Is.EqualTo(1.25f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator FiftyPointCoinCrossesThresholdByNextPhysicsStep()
        {
            _session.Score.SetDistance(475d);
            CoinCollectible coin = Live<CoinCollectible>();
            Assert.That(coin.TryInteract(_player), Is.True);
            Assert.That(coin.TryInteract(_player), Is.False);
            Assert.That(_session.Score.CurrentScore, Is.EqualTo(525));
            Assert.That(_session.Score.Distance, Is.LessThan(500d));
            yield return new WaitForFixedUpdate();
            Assert.That(_world.DifficultyMultiplier, Is.EqualTo(1.25f));
            Assert.That(_world.EffectiveSpeed, Is.EqualTo(7.5f));
            Assert.That(_world.StepDistance, Is.EqualTo(7.5f * Time.fixedDeltaTime).Within(0.00001f));
        }

        [UnityTest]
        public IEnumerator DashChangesFrom12To15WhenScoreReachesThreshold()
        {
            _player.QueueCommands(false, true);
            _player.PhysicsTick();
            TickAndAssertSpeed(12f, 0.02f);
            _session.Score.AddItemPoints(500);
            TickAndAssertSpeed(15f, 0.02f);
            Assert.That(_player.IsDashing, Is.True);
            Assert.That(_world.NormalSpeed, Is.EqualTo(7.5f));
            Assert.That(_world.DashSpeed, Is.EqualTo(_settings.MaximumSpeed));
            yield return null;
        }

        [UnityTest]
        public IEnumerator PartialDashIntegratesOnceInBothStagesAndCarriesCooldownOvershoot()
        {
            for (int stage = 0; stage < 2; stage++)
            {
                if (stage == 1) _session.Score.AddItemPoints(500);
                _player.QueueCommands(false, true);
                _player.PhysicsTick();
                Assert.That(_player.IsDashing, Is.True);
                float stepSeconds = _settings.DashDurationSeconds + 0.05f;
                float expectedSpeed = (stage == 0 ? 6f : 7.5f)
                    * (1f + (_settings.DashSpeedMultiplier - 1f) * _settings.DashDurationSeconds / stepSeconds);
                TickAndAssertSpeed(expectedSpeed, stepSeconds);
                Assert.That(_player.IsDashing, Is.False);
                // EffectiveSpeed can be read repeatedly without advancing any timer.
                Assert.That(_world.EffectiveSpeed, Is.EqualTo(expectedSpeed).Within(0.00001f));
                Assert.That(_player.DashCooldownRemaining,
                    Is.EqualTo(_settings.DashCooldownSeconds - 0.05f).Within(0.00001f));
                TickAndAssertSpeed(stage == 0 ? 6f : 7.5f, _settings.DashCooldownSeconds);
                Assert.That(_player.DashCooldownRemaining, Is.Zero);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator MenuAndNewRunResetDifficultyAndKeepHighScore()
        {
            _session.Score.AddItemPoints(600);
            TickAndAssertSpeed(7.5f, 0.02f);
            _session.Pause();
            _session.ReturnToMainMenu();
            Assert.That(_world.DifficultyMultiplier, Is.EqualTo(1f));
            Assert.That(_world.EffectiveSpeed, Is.Zero);
            Assert.That(_session.Score.HighScore, Is.EqualTo(600));
            _session.StartRun();
            Assert.That(_session.Score.CurrentScore, Is.Zero);
            Assert.That(_session.Score.HighScore, Is.EqualTo(600));
            TickAndAssertSpeed(6f, 0.02f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StoredHighScoreAboveThresholdStartsNormalSpeedRun()
        {
            _session.Pause();
            _session.ReturnToMainMenu();
            TestHighScoreStore store = new TestHighScoreStore { Record = 9000 };
            _session.Score.Initialize(_settings.PointsPerWorldUnit, store);
            _session.StartRun();
            Assert.That(_session.Score.CurrentScore, Is.Zero);
            Assert.That(_session.Score.HighScore, Is.EqualTo(9000));
            TickAndAssertSpeed(6f, 0.02f);
            Assert.That(_world.DifficultyMultiplier, Is.EqualTo(1f));
            Assert.That(_world.DashSpeed, Is.EqualTo(12f));
            Assert.That(store.Record, Is.EqualTo(9000));
            yield return null;
        }

        [UnityTest]
        public IEnumerator HarderDashSharesDistanceWithObstaclesParallaxAndScore()
        {
            _spawner.Tick(5f, 0f, _world.TotalDistance);
            Transform spawnedRoot = (Transform)typeof(SpawnController).GetField("_spawnedObjectsRoot",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_spawner);
            Rigidbody2D obstacle = spawnedRoot.GetChild(0).GetComponent<Rigidbody2D>();
            ParallaxLayer[] layers = (ParallaxLayer[])typeof(GameSession).GetField("_parallaxLayers",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_session);
            float[] tileX = new float[layers.Length];
            for (int i = 0; i < layers.Length; i++) tileX[i] = layers[i].transform.GetChild(0).position.x;
            double previousDistance = _world.TotalDistance;
            float previousX = obstacle.position.x;
            _session.Score.AddItemPoints(500);
            _player.QueueCommands(false, true);
            yield return new WaitForFixedUpdate();
            float distance = 15f * Time.fixedDeltaTime;
            Assert.That(_world.StepDistance, Is.EqualTo(distance).Within(0.00001f));
            Assert.That(_world.TotalDistance - previousDistance, Is.EqualTo(distance).Within(0.00001d));
            Assert.That(_session.Score.Distance, Is.EqualTo(_world.TotalDistance));
            Assert.That(previousX - obstacle.position.x, Is.EqualTo(distance).Within(0.00001f));
            for (int i = 0; i < layers.Length; i++)
                Assert.That(tileX[i] - layers[i].transform.GetChild(0).position.x,
                    Is.EqualTo(distance * layers[i].SpeedFactor).Within(0.00001f));
        }

        [UnityTest]
        public IEnumerator EarlySpawnReactionBoundaryReservesHardestDashSpeed()
        {
            Camera camera = (Camera)typeof(SpawnController).GetField("_camera",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_spawner);
            camera.transform.position = new Vector3(-20f, 0f, -10f);
            _spawner.Tick(5f, 0f, _world.TotalDistance);
            Transform spawnedRoot = (Transform)typeof(SpawnController).GetField("_spawnedObjectsRoot",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_spawner);
            Assert.That(spawnedRoot.childCount, Is.EqualTo(1));
            float warningDistance = spawnedRoot.GetChild(0).position.x - _player.transform.position.x;
            Assert.That(_world.DifficultyMultiplier, Is.EqualTo(1f));
            Assert.That(_world.DashSpeed, Is.EqualTo(12f));
            Assert.That(warningDistance, Is.GreaterThanOrEqualTo(15f * _settings.ReactionSeconds + 0.7f - 0.00001f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator FatalHitFreezesGameplayButAllowsDeathAndRepeatedRestart()
        {
            int gameOvers = 0;
            _session.StateChanged += state => { if (state == GameState.GameOver) gameOvers++; };
            for (int cycle = 0; cycle < 2; cycle++)
            {
                for (int hit = 0; hit < 3; hit++)
                {
                    _player.Health.Tick(_settings.InvulnerabilitySeconds + 0.01f);
                    Live<GroundObstacle>().TryInteract(_player);
                }

                Assert.That(_session.State, Is.EqualTo(GameState.GameOver));
                Assert.That(_player.Health.CurrentLives, Is.Zero);
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                double distance = _world.TotalDistance;
                int score = _session.Score.CurrentScore;
                _session.Score.AddItemPoints(_settings.CoinPoints);
                _session.Resume();
                yield return new WaitForSeconds(0.05f);
                Assert.That(_world.TotalDistance, Is.EqualTo(distance));
                Assert.That(_session.Score.CurrentScore, Is.EqualTo(score));
                Assert.That(_animator.GetCurrentAnimatorStateInfo(0).IsName("Death"), Is.True);
                _session.Restart();
                Assert.That(_player.Health.CurrentLives, Is.EqualTo(3));
                Assert.That(_player.IsDashing, Is.False);
            }

            Assert.That(gameOvers, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator ShapeSweepCatchesHazardCrossingEntirePlayerInOneStep()
        {
            GroundObstacle obstacle = Live<GroundObstacle>();
            obstacle.transform.position = _player.transform.position + Vector3.right * 2f;
            Physics2D.SyncTransforms();
            obstacle.Mover.Step(4f, -100f);
            Assert.That(_player.Health.CurrentLives, Is.EqualTo(2));
            obstacle.Mover.Step(4f, -100f);
            Assert.That(_player.Health.CurrentLives, Is.EqualTo(2));
            yield return null;
        }

        [UnityTest]
        public IEnumerator PauseWinsOverGameplayInSameInputUpdateAndMenuTransitionsAreGuarded()
        {
            Send(Key.Space, Key.LeftShift, Key.Escape);
            yield return null;
            Assert.That(_session.State, Is.EqualTo(GameState.Paused));
            Assert.That(_player.IsDashing, Is.False);
            Assert.That(_player.VerticalVelocity, Is.LessThanOrEqualTo(0f));
            _session.StartRun();
            Assert.That(_session.State, Is.EqualTo(GameState.Paused));
            _session.ReturnToMainMenu();
            _session.Resume();
            _session.Restart();
            Assert.That(_session.State, Is.EqualTo(GameState.MainMenu));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            _session.StartRun();
            _session.StartRun();
            yield return new WaitForFixedUpdate();
            Assert.That(_player.IsDashing, Is.False, "Held gameplay actions must not fire after starting again.");
        }

        [UnityTest]
        public IEnumerator SpawnCapAndGameOverFreezeExistingKinematicObjects()
        {
            Set(_settings, "_maximumActiveInteractables", 1);
            _spawner.Tick(5f, 0f, 100d);
            _spawner.Tick(100f, 0f, 1000d);
            Assert.That(_spawner.ActiveCount, Is.EqualTo(1), "Long frames cannot bypass the active cap.");
            Transform spawnedRoot = (Transform)typeof(SpawnController).GetField("_spawnedObjectsRoot",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_spawner);
            Rigidbody2D hazard = spawnedRoot.GetChild(0).GetComponent<Rigidbody2D>();
            yield return new WaitForFixedUpdate();
            for (int i = 0; i < 3; i++)
            {
                _player.Health.Tick(_settings.InvulnerabilitySeconds + 0.01f);
                _player.Health.TryTakeDamage();
            }

            Vector2 position = hazard.position;
            Assert.That(hazard.simulated, Is.False);
            yield return new WaitForSeconds(0.05f);
            Assert.That(hazard.position, Is.EqualTo(position));
            _session.Restart();
            Assert.That(_spawner.ActiveCount, Is.Zero);
        }

        private AnimatorController CreateController()
        {
            AnimatorController controller = new AnimatorController();
            controller.AddLayer("Base Layer");
            _ownedAssets.Add(controller);
            foreach (string parameter in new[] { "IsGrounded", "IsHit", "IsDead", "IsDashing" })
                controller.AddParameter(parameter, AnimatorControllerParameterType.Bool);
            controller.AddParameter("VerticalVelocity", AnimatorControllerParameterType.Float);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            _ownedAssets.Add(machine);
            foreach (string name in new[] { "Run", "Jump", "Hit", "Death" })
            {
                AnimationClip clip = new AnimationClip { name = name };
                clip.SetCurve("", typeof(Transform), "localScale.x", AnimationCurve.Constant(0f, 0.5f, 1f));
                _ownedAssets.Add(clip);
                AnimatorState state = machine.AddState(name);
                _ownedAssets.Add(state);
                state.motion = clip;
            }

            AnimatorStateTransition death = machine.AddAnyStateTransition(machine.states[3].state);
            death.hasExitTime = false;
            death.duration = 0f;
            death.canTransitionToSelf = false;
            death.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");
            _ownedAssets.Add(death);
            return controller;
        }

        private T Prefab<T>(Vector2 size) where T : Interactable
        {
            GameObject item = Child(typeof(T).Name);
            item.layer = 10;
            item.transform.position = new Vector3(1000f, 0f, 0f);
            item.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
            return item.AddComponent<T>();
        }

        private T Live<T>() where T : Interactable
        {
            T item = Prefab<T>(new Vector2(0.5f, 0.8f));
            item.Initialize(_session);
            return item;
        }

        private GameObject Child(string name, Transform parent = null)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent != null ? parent : _root.transform, false);
            return child;
        }

        private Transform Height(string name, float y)
        {
            Transform height = Child(name).transform;
            height.position = new Vector3(0f, y, 0f);
            return height;
        }

        private TMP_Text Text(string name, Transform parent) => Child(name, parent).AddComponent<TextMeshProUGUI>();

        private InputActionReference ActionReference(string path)
        {
            InputActionReference reference = InputActionReference.Create(_actions.FindAction(path, true));
            _ownedAssets.Add(reference);
            return reference;
        }

        private void Send(params Key[] keys)
        {
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(keys));
            InputSystem.Update();
        }

        private static void Set(object target, string field, object value)
        {
            FieldInfo info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, field);
            info.SetValue(target, value);
        }

        private void TickAndAssertSpeed(float expectedSpeed, float deltaSeconds)
        {
            double previousDistance = _world.TotalDistance;
            _world.Tick(deltaSeconds);
            Assert.That(_world.EffectiveSpeed, Is.EqualTo(expectedSpeed).Within(0.00001f));
            Assert.That(_world.StepDistance, Is.EqualTo(_world.EffectiveSpeed * deltaSeconds).Within(0.00001f));
            Assert.That(_world.TotalDistance - previousDistance,
                Is.EqualTo(expectedSpeed * deltaSeconds).Within(0.00001d));
        }

        private sealed class TestHighScoreStore : IHighScoreStore
        {
            public int Record;
            public int Load() => Record;
            public void Save(int highScore) => Record = highScore;
        }
    }
}
#endif
