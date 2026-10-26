using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChromeDino.Tests
{
    public sealed class GameplayInvariantTests
    {
        private GameObject _root;
        private RunnerSettings _settings;
        private PlayerHealth _health;
        private ScoreTracker _score;
        private MemoryStore _store;
        private string _preferenceKey;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Invariant test");
            _settings = ScriptableObject.CreateInstance<RunnerSettings>();
            _health = _root.AddComponent<PlayerHealth>();
            _health.Initialize(_settings);
            _health.SetDamageEnabled(true);
            _store = new MemoryStore { Record = 5 };
            _score = _root.AddComponent<ScoreTracker>();
            _score.Initialize(1f, _store);
            _score.SetScoringEnabled(true);
            _preferenceKey = "ChromeDino.Tests." + Guid.NewGuid();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(_preferenceKey);
            PlayerPrefs.Save();
            UnityEngine.Object.DestroyImmediate(_root);
            UnityEngine.Object.DestroyImmediate(_settings);
        }

        [Test]
        public void EligibleHitsReachZeroOnceAndCannotBecomeNegative()
        {
            int deaths = 0;
            int nonfatalHits = 0;
            _health.Died += () => deaths++;
            _health.Damaged += () => nonfatalHits++;
            Assert.That(_health.CurrentLives, Is.EqualTo(3));
            for (int lives = 2; lives >= 0; lives--)
            {
                Assert.That(_health.TryTakeDamage(), Is.True);
                Assert.That(_health.CurrentLives, Is.EqualTo(lives));
                Assert.That(_health.TryTakeDamage(), Is.False);
                _health.Tick(_settings.InvulnerabilitySeconds + 0.01f);
            }

            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(nonfatalHits, Is.EqualTo(2));
            Assert.That(_health.IsRecovering, Is.False);
        }

        [Test]
        public void PausedHealthPreservesRemainingRecoveryAndInvulnerability()
        {
            _health.TryTakeDamage();
            _health.SetDamageEnabled(false);
            _health.Tick(100f);
            Assert.That(_health.IsRecovering, Is.True);
            Assert.That(_health.TryTakeDamage(), Is.False);
            _health.SetDamageEnabled(true);
            _health.Tick(_settings.HitRecoverySeconds);
            Assert.That(_health.IsRecovering, Is.False);
            Assert.That(_health.IsInvulnerable, Is.True);
            _health.Tick(_settings.InvulnerabilitySeconds);
            Assert.That(_health.TryTakeDamage(), Is.True);
        }

        [Test]
        public void ReentrantDamageCannotRemoveAnotherLife()
        {
            _health.Damaged += () => Assert.That(_health.TryTakeDamage(), Is.False);
            _health.TryTakeDamage();
            Assert.That(_health.CurrentLives, Is.EqualTo(2));
        }

        [Test]
        public void ResetRestoresThreeLivesAndRetainsRecord()
        {
            _health.TryTakeDamage();
            _score.AddItemPoints(_settings.CoinPoints);
            _health.ResetRun();
            _score.ResetRun();
            Assert.That(_health.CurrentLives, Is.EqualTo(3));
            Assert.That(_health.IsInvulnerable, Is.False);
            Assert.That(_score.CurrentScore, Is.Zero);
            Assert.That(_score.HighScore, Is.EqualTo(50));
            Assert.That(_store.Record, Is.EqualTo(50));
        }

        [Test]
        public void FractionalDistanceAccumulatesAndCoinAddsExactlyItsPoints()
        {
            _score.SetDistance(0.4d);
            _score.SetDistance(0.8d);
            _score.SetDistance(1.2d);
            _score.AddItemPoints(_settings.CoinPoints);
            Assert.That(_score.CurrentScore, Is.EqualTo(51));
            Assert.That(_score.HighScore, Is.EqualTo(51));
        }

        [Test]
        public void StoppedScoreRejectsDistanceAndItems()
        {
            _score.SetDistance(2.5d);
            _score.SetScoringEnabled(false);
            _score.SetDistance(100d);
            _score.AddItemPoints(10);
            Assert.That(_score.CurrentScore, Is.EqualTo(2));
        }

        [Test]
        public void ScoreSaturatesAndRejectsInvalidRequests()
        {
            _score.SetDistance(double.NaN);
            _score.SetDistance(-1d);
            _score.AddItemPoints(-10);
            Assert.That(_score.CurrentScore, Is.Zero);
            _score.AddItemPoints(int.MaxValue);
            _score.AddItemPoints(int.MaxValue);
            _score.SetDistance(double.MaxValue);
            Assert.That(_score.CurrentScore, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void RecordIsDirtyUntilCheckpointAndLowerRunCannotOverwriteIt()
        {
            _score.AddItemPoints(20);
            Assert.That(_store.Saves, Is.Zero);
            _score.FlushHighScore();
            _score.FlushHighScore();
            Assert.That(_store.Saves, Is.EqualTo(1));
            _score.ResetRun();
            _score.SetScoringEnabled(true);
            _score.AddItemPoints(2);
            _score.FlushHighScore();
            Assert.That(_store.Record, Is.EqualTo(20));
            Assert.That(_store.Saves, Is.EqualTo(1));
        }

        [Test]
        public void PlayerPrefsStoreHandlesMissingNegativeAndFreshInstance()
        {
            PlayerPrefsHighScoreStore store = new PlayerPrefsHighScoreStore(_preferenceKey);
            Assert.That(store.Load(), Is.Zero);
            PlayerPrefs.SetInt(_preferenceKey, -7);
            Assert.That(store.Load(), Is.Zero);
            store.Save(42);
            PlayerPrefsHighScoreStore reopened = new PlayerPrefsHighScoreStore(_preferenceKey);
            Assert.That(reopened.Load(), Is.EqualTo(42));
            reopened.Save(2);
            Assert.That(store.Load(), Is.EqualTo(42));
        }

        [Test]
        public void GameSceneReferencesSettingsWithFiftyPointCoinsAndConfiguredDifficulty()
        {
            RunnerSettings activeSettings = AssetDatabase.LoadAssetAtPath<RunnerSettings>("Assets/Settings/DefaultRunnerSettings.asset");
            Assert.That(activeSettings, Is.Not.Null);
            Assert.That(activeSettings.CoinPoints, Is.EqualTo(50));
            Assert.That(activeSettings.DifficultyScoreThreshold, Is.EqualTo(500));
            Assert.That(activeSettings.HarderSpeedMultiplier, Is.EqualTo(1.25f));
            var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
            try
            {
                GameSession session = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    session = root.GetComponentInChildren<GameSession>(true);
                    if (session != null) break;
                }

                Assert.That(session, Is.Not.Null);
                Assert.That(session.Settings, Is.SameAs(activeSettings));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void DefaultsUseFiftyPointCoinsAndHardestStageDashMaximum()
        {
            Assert.That(_settings.CoinPoints, Is.EqualTo(50));
            Assert.That(_settings.DifficultyScoreThreshold, Is.EqualTo(500));
            Assert.That(_settings.HarderSpeedMultiplier, Is.EqualTo(1.25f));
            Assert.That(_settings.MaximumSpeed, Is.EqualTo(15f));
        }

        [TestCase(0.5f, 1f)]
        [TestCase(float.NaN, 1.25f)]
        [TestCase(float.PositiveInfinity, 1.25f)]
        [TestCase(float.NegativeInfinity, 1.25f)]
        public void DifficultyValidationRequiresPositiveThresholdAndFiniteNonSlowingMultiplier(float value, float expected)
        {
            typeof(RunnerSettings).GetField("_difficultyScoreThreshold", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_settings, -1);
            typeof(RunnerSettings).GetField("_harderSpeedMultiplier", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_settings, value);
            _settings.ValidateValues();
            Assert.That(_settings.DifficultyScoreThreshold, Is.EqualTo(1));
            Assert.That(_settings.HarderSpeedMultiplier, Is.EqualTo(expected));
        }

        [Test]
        public void SpacingIncludesMaximumSpeedFlightReactionAndBothWidths()
        {
            Assert.That(_settings.MaximumSpeed, Is.EqualTo(15f), "Safety uses the hardest dash, even before score 500.");
            float spacing = SpawnSpacing.MinimumCenterDistance(_settings, 9.81f, 1f, 1f, 2f);
            float flight = 2f * _settings.JumpVelocity / (9.81f * _settings.GravityScale);
            float requiredGap = _settings.MaximumSpeed
                * (flight + _settings.LandingRecoverySeconds + _settings.ReactionSeconds);
            Assert.That(spacing - 1.5f - 1f, Is.GreaterThanOrEqualTo(requiredGap - 0.0001f));
        }

        private sealed class MemoryStore : IHighScoreStore
        {
            public int Record;
            public int Saves;
            public int Load() => Record;
            public void Save(int highScore)
            {
                Record = highScore;
                Saves++;
            }
        }
    }
}
