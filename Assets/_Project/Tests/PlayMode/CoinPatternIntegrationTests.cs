#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ChromeDino.Tests
{
    public sealed partial class RunnerIntegrationTests
    {
        private Transform SpawnRoot => Field<Transform>(_spawner, "_spawnedObjectsRoot");

        private CoinCollectible[] SpawnPattern(CoinPatternKind kind)
        {
            Set(_settings, "_singleCoinWeight", kind == CoinPatternKind.Single ? 1f : 0f);
            Set(_settings, "_coinRowWeight", kind == CoinPatternKind.Row ? 1f : 0f);
            Set(_settings, "_coinArcWeight", kind == CoinPatternKind.Arc ? 1f : 0f);
            Set(_settings, "_repeatedJumpCoinWeight", kind == CoinPatternKind.RepeatedJumps ? 1f : 0f);
            _spawner.ResetRun(31);
            _spawner.Tick(5f, 0f, 0d);
            Assert.That(SpawnRoot.GetComponentsInChildren<GroundObstacle>().Length, Is.EqualTo(1));
            _spawner.Tick(5f, 0f, 100d);
            Assert.That(SpawnRoot.GetComponentsInChildren<AerialObstacle>().Length, Is.EqualTo(1));
            _spawner.Tick(5f, 0f, 200d);
            return SpawnRoot.GetComponentsInChildren<CoinCollectible>();
        }

        [UnityTest]
        public IEnumerator SpawnedRowAwards150OnceAndMissingCoinAwardsNothing()
        {
            CoinCollectible[] coins = SpawnPattern(CoinPatternKind.Row);
            Assert.That(coins.Length, Is.EqualTo(3));
            int before = _session.Score.CurrentScore;
            foreach (CoinCollectible coin in coins)
            {
                Assert.That(coin.TryInteract(_player), Is.True);
                Assert.That(coin.TryInteract(_player), Is.False, "Sweep and trigger share the consumed guard.");
            }
            Assert.That(_session.Score.CurrentScore, Is.EqualTo(before + 150));
            coins = SpawnPattern(CoinPatternKind.Row);
            before = _session.Score.CurrentScore;
            coins[0].TryInteract(_player);
            coins[1].Despawn();
            coins[2].TryInteract(_player);
            Assert.That(_session.Score.CurrentScore, Is.EqualTo(before + 100));
            yield return null;
        }

        [UnityTest]
        public IEnumerator PatternCollectionCrossesDifficultyOnceWithoutChangingFiftyPointRewards()
        {
            CoinCollectible[] coins = SpawnPattern(CoinPatternKind.Row);
            _session.Score.AddItemPoints(449);
            for (int i = 0; i < coins.Length; i++)
            {
                coins[i].TryInteract(_player);
                TickAndAssertSpeed(i == 0 ? 6f : 7.5f, 0.02f);
                Assert.That(_session.Score.CurrentScore, Is.EqualTo(449 + (i + 1) * 50));
                Assert.That(_world.DashSpeed, Is.EqualTo(i == 0 ? 12f : 15f));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator CapWaitsForWholePatternAndDoesNotAdvanceOpportunityOrReroll()
        {
            Set(_settings, "_maximumActiveInteractables", 4);
            Assert.That(SpawnPattern(CoinPatternKind.Row).Length, Is.Zero);
            Assert.That(_spawner.ActiveCount, Is.EqualTo(2));
            Assert.That(Field<CoinPatternKind?>(_spawner, "_pendingCoinPattern"), Is.EqualTo(CoinPatternKind.Row));
            _spawner.Tick(100f, 0f, 500d);
            Assert.That(_spawner.ActiveCount, Is.EqualTo(2));
            SpawnRoot.GetComponentInChildren<GroundObstacle>().Despawn();
            _spawner.Tick(0f, 0f, 500d);
            Assert.That(_spawner.ActiveCount, Is.EqualTo(4));
            Assert.That(SpawnRoot.GetComponentsInChildren<CoinCollectible>().Length, Is.EqualTo(3));
            Assert.That(Field<int>(_spawner, "_nextKind"), Is.Zero, "One pattern consumes one coin opportunity.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator CapSmallerThanAnEntirePatternSkipsItAndKeepsHazardsRotating()
        {
            Set(_settings, "_maximumActiveInteractables", 2);
            Assert.That(SpawnPattern(CoinPatternKind.Row).Length, Is.Zero);
            foreach (Interactable item in SpawnRoot.GetComponentsInChildren<Interactable>()) item.Despawn();
            _spawner.Tick(0f, 0f, 300d);
            Assert.That(_spawner.ActiveCount, Is.Zero);
            Assert.That(Field<int>(_spawner, "_nextKind"), Is.Zero);
            _spawner.Tick(5f, 0f, 400d);
            Assert.That(_spawner.ActiveCount, Is.EqualTo(1));
            Assert.That(SpawnRoot.GetComponentInChildren<GroundObstacle>(), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FullPatternTailReservesHazardManeuverSpaceEvenAfterCoinsDisappearAndCameraChanges()
        {
            CoinCollectible[] coins = SpawnPattern(CoinPatternKind.RepeatedJumps);
            float tailRight = coins[2].GetComponent<BoxCollider2D>().bounds.max.x;
            float gap = SpawnSpacing.MinimumEdgeGap(_settings, -Physics2D.gravity.y, _player.Width);
            // Earlier hazard => pattern separation includes the approaching first coin.
            float previousRight = SpawnRoot.GetComponentInChildren<AerialObstacle>().GetComponent<BoxCollider2D>().bounds.max.x;
            Assert.That(coins[0].GetComponent<BoxCollider2D>().bounds.min.x - (previousRight - 100f),
                Is.GreaterThanOrEqualTo(gap));
            foreach (Interactable item in SpawnRoot.GetComponentsInChildren<Interactable>()) item.Despawn();
            Camera camera = Field<Camera>(_spawner, "_camera");
            camera.aspect = 4f / 3f;
            camera.transform.position += Vector3.left * 1f;
            _spawner.Tick(100f, 0f, 200d);
            Assert.That(_spawner.ActiveCount, Is.Zero, "Collection/despawn must not release reserved landing space.");
            float nextLeft = Field<float>(_spawner, "_spawnX") - 0.25f;
            float requiredTravel = tailRight + gap - nextLeft;
            _spawner.Tick(0f, 0f, 200d + requiredTravel - 0.01f);
            Assert.That(_spawner.ActiveCount, Is.Zero, "One coin's width cannot stand in for the full sequence.");
            _spawner.Tick(0f, 0f, 200d + requiredTravel + 0.01f);
            GroundObstacle hazard = SpawnRoot.GetComponentInChildren<GroundObstacle>();
            Assert.That(hazard, Is.Not.Null);
            Assert.That(hazard.GetComponent<BoxCollider2D>().bounds.min.x - (tailRight - requiredTravel - 0.01f),
                Is.GreaterThanOrEqualTo(gap - 0.0001f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllPatternCoinsDespawnAndRestartMenuAndDisableClearReservationsAndPendingChoice()
        {
            CoinCollectible[] coins = SpawnPattern(CoinPatternKind.RepeatedJumps);
            foreach (Interactable hazard in SpawnRoot.GetComponentsInChildren<Interactable>().Where(x => !(x is CoinCollectible)))
                hazard.Despawn();
            foreach (CoinCollectible coin in coins)
                coin.transform.position = new Vector3(-100f, 5f, 0f);
            Physics2D.SyncTransforms();
            _spawner.Tick(0f, 0.1f, 200.1d);
            Assert.That(_spawner.ActiveCount, Is.Zero);
            Assert.That(_session.Score.CurrentScore, Is.Zero, "Despawn is not collection.");
            yield return null;
            foreach (CoinCollectible coin in coins) Assert.That(coin == null, Is.True);

            SpawnPattern(CoinPatternKind.Row);
            _session.Pause();
            _session.Restart();
            AssertPatternReset();
            Set(_settings, "_maximumActiveInteractables", 4);
            SpawnPattern(CoinPatternKind.Row); // Leaves a pending three-coin choice at cap.
            _session.Pause();
            _session.ReturnToMainMenu();
            AssertPatternReset();
            _session.StartRun();
            SpawnPattern(CoinPatternKind.Row);
            _session.enabled = false;
            AssertPatternReset();
        }

        [UnityTest]
        public IEnumerator SeededResetReplaysCoinChoicesWithoutDroppingHazards()
        {
            string first = SpawnHistory(71);
            Assert.That(SpawnHistory(71), Is.EqualTo(first));
            Assert.That(SpawnHistory(72), Is.Not.EqualTo(first));
            yield return null;
        }

        private string SpawnHistory(int seed)
        {
            _spawner.ResetRun(seed);
            string history = "";
            for (int i = 0; i < 24; i++)
            {
                foreach (Interactable item in SpawnRoot.GetComponentsInChildren<Interactable>()) item.Despawn();
                _spawner.Tick(5f, 0f, i * 200d);
                Interactable[] items = SpawnRoot.GetComponentsInChildren<Interactable>();
                Assert.That(items.Length, Is.GreaterThan(0));
                Assert.That(items[0].GetType(), Is.EqualTo(i % 3 == 0 ? typeof(GroundObstacle)
                    : i % 3 == 1 ? typeof(AerialObstacle) : typeof(CoinCollectible)));
                history += items.Length + ":" + items[items.Length - 1].transform.position + ";";
            }
            return history;
        }

        private void AssertPatternReset()
        {
            Assert.That(_spawner.ActiveCount, Is.Zero);
            Assert.That(Field<bool>(_spawner, "_hasSpawned"), Is.False);
            Assert.That(Field<float>(_spawner, "_lastRightEdge"), Is.Zero);
            Assert.That(Field<CoinPatternKind?>(_spawner, "_pendingCoinPattern"), Is.Null);
            Assert.That(Field<int>(_spawner, "_nextKind"), Is.Zero);
            Assert.That(_spawner.CountdownSeconds, Is.EqualTo(_settings.FirstSpawnDelaySeconds));
        }

        private static T Field<T>(object target, string field) => (T)target.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    }
}
#endif
