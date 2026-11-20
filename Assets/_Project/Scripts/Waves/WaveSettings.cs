using System;
using System.Collections.Generic;
using UnityEngine;
using WaveSurvival.Enemies;

namespace WaveSurvival.Waves
{
    [Serializable]
    public class WaveEnemyEntry
    {
        public EnemyDefinition enemy;
        [Min(0f)] public float weight = 1f;
        [Min(1)] public int firstWave = 1;
    }

    /// <summary>Tuning for wave pacing and difficulty growth.</summary>
    [CreateAssetMenu(menuName = "Wave Survival/Wave Settings")]
    public class WaveSettings : ScriptableObject
    {
        [Header("Timing")]
        public float firstBreakDuration = 5f;
        public float breakDuration = 15f;
        public float spawnInterval = 0.6f;

        [Header("Difficulty")]
        public int baseEnemyCount = 5;
        public int extraEnemiesPerWave = 3;
        public float healthGrowthPerWave = 0.12f;
        public int waveClearScoreBonus = 50;

        [Header("Enemy mix")]
        public List<WaveEnemyEntry> enemies = new();

        public int EnemyCountFor(int wave) => baseEnemyCount + extraEnemiesPerWave * (wave - 1);
        public float HealthMultiplierFor(int wave) => 1f + healthGrowthPerWave * (wave - 1);
    }
}
