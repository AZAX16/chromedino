using UnityEngine;

namespace ChromeDino
{
    public static class SpawnSpacing
    {
        public static float MinimumCenterDistance(RunnerSettings settings, float gravityMagnitude,
            float playerWidth, float previousWidth, float nextWidth)
        {
            return (previousWidth + nextWidth) * 0.5f + MinimumEdgeGap(settings, gravityMagnitude, playerWidth);
        }

        public static float MinimumEdgeGap(RunnerSettings settings, float gravityMagnitude, float playerWidth)
        {
            float flightSeconds = 2f * settings.JumpVelocity / (gravityMagnitude * settings.GravityScale);
            // Reserve hardest-stage dash spacing even for early-run spawns.
            float recoveryGap = settings.MaximumSpeed
                * (flightSeconds + settings.LandingRecoverySeconds + settings.ReactionSeconds);
            return playerWidth + Mathf.Max(settings.MinimumClearGapWorldUnits, recoveryGap);
        }
    }
}
