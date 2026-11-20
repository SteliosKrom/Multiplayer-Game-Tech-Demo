using System.Collections.Generic;
using UnityEngine;
using WaveSurvival.Enemies;

namespace WaveSurvival.Waves
{
    /// <summary>Only job: decide which enemies make up wave N (weighted random from the allowed pool).</summary>
    public sealed class WaveComposer
    {
        private readonly WaveSettings _settings;
        private readonly List<WaveEnemyEntry> _pool = new();

        public WaveComposer(WaveSettings settings) => _settings = settings;

        public Queue<EnemyDefinition> Compose(int wave)
        {
            _pool.Clear();
            float totalWeight = 0f;
            foreach (var entry in _settings.enemies)
            {
                if (entry.enemy == null || entry.firstWave > wave || entry.weight <= 0f) continue;
                _pool.Add(entry);
                totalWeight += entry.weight;
            }

            var queue = new Queue<EnemyDefinition>();
            int count = _settings.EnemyCountFor(wave);
            for (int i = 0; i < count && _pool.Count > 0; i++)
                queue.Enqueue(PickWeighted(totalWeight));

            return queue;
        }

        private EnemyDefinition PickWeighted(float totalWeight)
        {
            float roll = Random.value * totalWeight;
            foreach (var entry in _pool)
            {
                roll -= entry.weight;
                if (roll <= 0f) return entry.enemy;
            }
            return _pool[_pool.Count - 1].enemy;
        }
    }
}
