using System;
using UnityEngine;

namespace ChromeDino
{
    public enum CoinPatternKind { Single, Row, Arc, RepeatedJumps }

    // A small immutable layout: X is relative to the first coin, Y is world height
    // of the collider center. Each coin still uses the ordinary collectible/mover.
    public readonly struct CoinPatternLayout
    {
        public int Count { get; }
        public float Span => Count == 1 ? 0f : _third.x;
        private readonly Vector2 _first;
        private readonly Vector2 _second;
        private readonly Vector2 _third;

        public CoinPatternLayout(int count, Vector2 first, Vector2 second, Vector2 third)
        {
            Count = count;
            _first = first;
            _second = second;
            _third = third;
        }

        public Vector2 Position(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            return index == 0 ? _first : index == 1 ? _second : _third;
        }
    }

    public static class CoinPatterns
    {
        public static CoinPatternKind Select(RunnerSettings settings, System.Random random)
        {
            // Double precision keeps the sum finite for large valid Inspector weights.
            double single = settings.SingleCoinWeight;
            double row = settings.CoinRowWeight;
            double arc = settings.CoinArcWeight;
            double total = single + row + arc + settings.RepeatedJumpCoinWeight;
            if (total <= 0d) return CoinPatternKind.Single;
            double choice = random.NextDouble() * total;
            if (choice < single) return CoinPatternKind.Single;
            if (choice < single + row) return CoinPatternKind.Row;
            if (choice < single + row + arc) return CoinPatternKind.Arc;
            return CoinPatternKind.RepeatedJumps;
        }

        public static CoinPatternLayout Create(CoinPatternKind kind, RunnerSettings settings,
            float gravityMagnitude, float standingCenterY, float playerWidth, Vector2 coinSize,
            float singleCoinCenterY, float normalSpeed)
        {
            if (kind == CoinPatternKind.Single)
                return new CoinPatternLayout(1, new Vector2(0f, singleCoinCenterY), default, default);

            float gravity = gravityMagnitude * settings.GravityScale;
            // Unity integrates gravity before position. At fixed-step samples its
            // trajectory is (v - g*dt/2)*t - g*t*t/2, slightly below a continuous jump.
            float trajectoryVelocity = Mathf.Max(0f, settings.JumpVelocity - gravity * Time.fixedDeltaTime * 0.5f);
            float apexSeconds = trajectoryVelocity / gravity;
            float apexHeight = trajectoryVelocity * apexSeconds * 0.5f;
            float separation;
            float edgeY;
            float middleY;
            if (kind == CoinPatternKind.Row)
            {
                separation = Mathf.Max(settings.CoinRowSpacingWorldUnits, coinSize.x + 0.2f);
                edgeY = middleY = standingCenterY;
            }
            else if (kind == CoinPatternKind.Arc)
            {
                // Sample one jump at t_apex +/- halfDuration. Center the coins
                // on the standing collider's translated center, not its root/pivot.
                // Use normal speed, so triggering a dash need not collect all coins.
                float halfDuration = apexSeconds * settings.CoinArcHalfDurationFraction;
                separation = normalSpeed * halfDuration;
                edgeY = standingCenterY + apexHeight - 0.5f * gravity * halfDuration * halfDuration;
                middleY = standingCenterY + apexHeight;
            }
            else
            {
                // Predictable optional coins need a jump cadence, rather than the
                // hazard avoidance gap. Use the fastest normal stage so an early
                // pattern still allows landing/repressing after difficulty changes.
                // A deliberate dash may skip a coin; external hazard spacing still
                // uses MaximumSpeed independently of this internal separation.
                float flightSeconds = 2f * settings.JumpVelocity / gravity;
                float fastestNormalSpeed = settings.BaseSpeed * settings.HarderSpeedMultiplier;
                float jumpCycleSeconds = flightSeconds + settings.LandingRecoverySeconds
                    + settings.RepeatedJumpReactionSeconds;
                separation = Mathf.Max(playerWidth + coinSize.x + 0.2f, fastestNormalSpeed * jumpCycleSeconds);
                edgeY = middleY = standingCenterY + apexHeight;
            }

            return new CoinPatternLayout(3, new Vector2(0f, edgeY),
                new Vector2(separation, middleY), new Vector2(2f * separation, edgeY));
        }
    }
}
