using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ChromeDino.Tests
{
    public sealed class CoinPatternTests
    {
        private RunnerSettings _settings;
        [SetUp] public void SetUp() => _settings = ScriptableObject.CreateInstance<RunnerSettings>();
        [TearDown] public void TearDown() => UnityEngine.Object.DestroyImmediate(_settings);

        private CoinPatternLayout Layout(CoinPatternKind kind, float speed = 6f) =>
            CoinPatterns.Create(kind, _settings, 9.81f, -1.9f, 0.8f, Vector2.one * 0.35f, -0.8f, speed);

        [TestCase(CoinPatternKind.Single, 1)]
        [TestCase(CoinPatternKind.Row, 3)]
        [TestCase(CoinPatternKind.Arc, 3)]
        [TestCase(CoinPatternKind.RepeatedJumps, 3)]
        public void LayoutCountsAndFullWidthsIncludeEveryCoin(CoinPatternKind kind, int count)
        {
            CoinPatternLayout layout = Layout(kind);
            Assert.That(layout.Count, Is.EqualTo(count));
            Assert.That(layout.Position(0).x, Is.Zero);
            Assert.That(layout.Span, Is.EqualTo(layout.Position(count - 1).x));
            for (int i = 1; i < count; i++)
                Assert.That(layout.Position(i).x - layout.Position(i - 1).x, Is.GreaterThan(0.35f));
            Assert.Throws<ArgumentOutOfRangeException>(() => layout.Position(count));
        }

        [Test]
        public void SingleRetainsAnchorAndRowIsDistinctAtStandingColliderCenter()
        {
            Assert.That(Layout(CoinPatternKind.Single).Position(0).y, Is.EqualTo(-0.8f));
            CoinPatternLayout row = Layout(CoinPatternKind.Row);
            for (int i = 0; i < 3; i++)
            {
                Assert.That(row.Position(i), Is.EqualTo(new Vector2(i, -1.9f)));
                Assert.That(row.Position(i).y + 0.175f, Is.LessThan(-1.9f + 0.6f));
            }
            Assert.That(row.Span, Is.LessThan(_settings.MinimumClearGapWorldUnits));
        }

        [TestCase(6f, 6f, 6f)]
        [TestCase(7.5f, 7.5f, 7.5f)]
        [TestCase(6f, 7.5f, 7.5f)] // Stage changes on approach.
        [TestCase(6f, 6f, 7.5f)]   // Stage changes during collection.
        [TestCase(7.5f, 6f, 6f)]   // Conservative check of either geometry at either speed.
        public void ShallowArcFitsOneDiscreteJumpAcrossBothNormalStages(float layoutSpeed, float firstSpeed, float lastSpeed)
        {
            CoinPatternLayout arc = Layout(CoinPatternKind.Arc, layoutSpeed);
            float gravity = 9.81f * _settings.GravityScale;
            float apex = _settings.JumpVelocity / gravity;
            float toMiddle = arc.Position(1).x / firstSpeed;
            float toLast = (arc.Position(2).x - arc.Position(1).x) / lastSpeed;
            Assert.That(arc.Position(1).y, Is.GreaterThan(arc.Position(0).y));
            Assert.That(arc.Position(2).y, Is.EqualTo(arc.Position(0).y));
            Assert.That(arc.Position(1).y - arc.Position(0).y, Is.LessThan(0.4f));
            for (int i = 0; i < 3; i++)
            {
                float time = i == 0 ? apex - toMiddle : i == 1 ? apex : apex + toLast;
                // Unity's semi-implicit integration at the actual fixed timestep.
                float rise = _settings.JumpVelocity * time - gravity * time * (time + Time.fixedDeltaTime) * 0.5f;
                Assert.That(time, Is.InRange(0f, 2f * apex));
                Assert.That(Mathf.Abs((-1.9f + rise) - arc.Position(i).y), Is.LessThan(0.3f),
                    "Coin center lies comfortably inside the actual capsule, before considering coin extents.");
                Assert.That(arc.Position(i).y - 0.175f, Is.GreaterThan(-1.9f + 0.6f));
            }
        }

        [TestCase(6f)]
        [TestCase(7.5f)]
        public void RepeatedJumpGapAllowsFlightAndGroundedRepressAtBothNormalStagesWithoutLongWait(float speed)
        {
            CoinPatternLayout layout = Layout(CoinPatternKind.RepeatedJumps, speed);
            float flight = 2f * _settings.JumpVelocity / (9.81f * _settings.GravityScale);
            float centerSeconds = layout.Position(1).x / speed;
            Assert.That(centerSeconds - flight, Is.GreaterThanOrEqualTo(_settings.LandingRecoverySeconds
                + _settings.RepeatedJumpReactionSeconds - 0.0001f));
            Assert.That(centerSeconds, Is.LessThan(1.4f), "The default sequence should have a compact jump cadence.");
            Assert.That(layout.Position(1).x, Is.InRange(8f, 8.2f));
            Assert.That(layout.Span, Is.LessThan(17f));
            Assert.That(layout.Position(0).y, Is.EqualTo(layout.Position(2).y));
        }

        [Test]
        public void InternalCoinCadenceIsIndependentOfHazardGapAndDashSpeed()
        {
            float spacing = Layout(CoinPatternKind.RepeatedJumps).Position(1).x;
            Set("_minimumClearGapWorldUnits", 100f);
            Set("_reactionSeconds", 2f);
            Set("_dashSpeedMultiplier", 3f);
            Assert.That(Layout(CoinPatternKind.RepeatedJumps).Position(1).x, Is.EqualTo(spacing));
            Assert.That(SpawnSpacing.MinimumEdgeGap(_settings, 9.81f, 0.8f), Is.GreaterThanOrEqualTo(100f),
                "Tightening optional coins must not tighten the surrounding hazard clearance.");
            Set("_repeatedJumpReactionSeconds", 0.3f);
            Assert.That(Layout(CoinPatternKind.RepeatedJumps).Position(1).x, Is.GreaterThan(spacing));
        }

        [Test]
        public void WeightedSelectionIsSeededAndZeroWeightsDisablePatterns()
        {
            System.Random first = new System.Random(73), second = new System.Random(73);
            int[] counts = new int[4];
            for (int i = 0; i < 6000; i++)
            {
                CoinPatternKind chosen = CoinPatterns.Select(_settings, first);
                Assert.That(CoinPatterns.Select(_settings, second), Is.EqualTo(chosen));
                counts[(int)chosen]++;
            }
            Assert.That(counts[0], Is.InRange(850, 1150));
            Assert.That(counts[1], Is.InRange(1800, 2200));
            Assert.That(counts[2], Is.InRange(1800, 2200));
            Assert.That(counts[3], Is.InRange(850, 1150));
            Set("_singleCoinWeight", 0f); Set("_coinRowWeight", 0f); Set("_coinArcWeight", 0f);
            for (int i = 0; i < 20; i++)
                Assert.That(CoinPatterns.Select(_settings, first), Is.EqualTo(CoinPatternKind.RepeatedJumps));
            Set("_repeatedJumpCoinWeight", 0f);
            Assert.That(CoinPatterns.Select(_settings, first), Is.EqualTo(CoinPatternKind.Single));
        }

        [Test]
        public void InvalidPatternSettingsAreSanitized()
        {
            Set("_singleCoinWeight", float.NaN); Set("_coinRowWeight", -1f);
            Set("_coinArcWeight", float.PositiveInfinity); Set("_repeatedJumpCoinWeight", float.NegativeInfinity);
            Set("_coinRowSpacingWorldUnits", float.NaN); Set("_coinArcHalfDurationFraction", 100f);
            Set("_repeatedJumpReactionSeconds", float.NaN);
            _settings.ValidateValues();
            Assert.That(_settings.SingleCoinWeight + _settings.CoinRowWeight + _settings.CoinArcWeight
                + _settings.RepeatedJumpCoinWeight, Is.Zero);
            Assert.That(_settings.CoinRowSpacingWorldUnits, Is.EqualTo(1f));
            Assert.That(_settings.CoinArcHalfDurationFraction, Is.EqualTo(0.5f));
            Assert.That(_settings.RepeatedJumpReactionSeconds, Is.EqualTo(0.15f));
        }

        private void Set(string field, object value) => typeof(RunnerSettings)
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_settings, value);
    }
}
