using UnityEngine;

namespace WaveSurvival.Core
{
    /// <summary>
    /// Answers "who should an enemy chase?". Singleplayer has one target,
    /// multiplayer can return the closest of many players without enemies knowing the difference.
    /// </summary>
    public interface ITargetProvider
    {
        bool TryGetClosestTarget(Vector3 from, out Transform target);
    }
}
