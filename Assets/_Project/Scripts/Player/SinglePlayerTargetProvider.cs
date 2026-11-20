using UnityEngine;
using WaveSurvival.Combat;
using WaveSurvival.Core;

namespace WaveSurvival.Player
{
    /// <summary>Singleplayer answer to "who do enemies chase": the one player, while alive.</summary>
    public sealed class SinglePlayerTargetProvider : ITargetProvider
    {
        private readonly Transform _player;
        private readonly Health _health;

        public SinglePlayerTargetProvider(Transform player, Health health)
        {
            _player = player;
            _health = health;
        }

        public bool TryGetClosestTarget(Vector3 from, out Transform target)
        {
            target = _player != null && _health.IsAlive ? _player : null;
            return target != null;
        }
    }
}
