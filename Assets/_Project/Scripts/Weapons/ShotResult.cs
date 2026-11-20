using UnityEngine;

namespace WaveSurvival.Weapons
{
    /// <summary>Where a single pellet started and ended. Used by visuals (and later by the network).</summary>
    public readonly struct ShotResult
    {
        public readonly Vector3 Origin;
        public readonly Vector3 End;
        public readonly bool HitSomething;

        public ShotResult(Vector3 origin, Vector3 end, bool hitSomething)
        {
            Origin = origin;
            End = end;
            HitSomething = hitSomething;
        }
    }
}
