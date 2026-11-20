using System;
using System.Collections.Generic;
using UnityEngine;
using WaveSurvival.Core;
using WaveSurvival.Enemies;

namespace WaveSurvival.Waves
{
    public enum WavePhase { Idle, Break, Combat, Stopped }

    /// <summary>
    /// The wave state machine: Break -> Combat -> Break ...
    /// Asks WaveComposer what to spawn and IEnemySpawner to spawn it. Raises events for UI and rewards.
    /// </summary>
    public class WaveDirector : MonoBehaviour
    {
        [SerializeField] private WaveSettings settings;

        private IEnemySpawner _spawner;
        private WaveComposer _composer;
        private Queue<EnemyDefinition> _toSpawn = new();
        private float _nextSpawnTime;
        private float _breakEndsAt;

        public WavePhase Phase { get; private set; } = WavePhase.Idle;
        public int CurrentWave { get; private set; }
        public int AliveEnemies { get; private set; }
        public int EnemiesRemaining => AliveEnemies + _toSpawn.Count;
        public float BreakTimeRemaining => Phase == WavePhase.Break ? Mathf.Max(0f, _breakEndsAt - Time.time) : 0f;
        public WaveSettings Settings => settings;

        public event Action<int> WaveStarted;
        /// <summary>(upcoming wave, duration)</summary>
        public event Action<int, float> BreakStarted;
        public event Action<int> WaveCleared;
        public event Action<int> EnemiesRemainingChanged;

        public void Construct(IEnemySpawner spawner, WaveComposer composer)
        {
            _spawner = spawner;
            _composer = composer;
            _spawner.EnemyKilled += OnEnemyKilled;
        }

        private void OnDestroy()
        {
            if (_spawner != null) _spawner.EnemyKilled -= OnEnemyKilled;
        }

        public void Begin() => StartBreak(settings.firstBreakDuration);

        public void Stop() => Phase = WavePhase.Stopped;

        /// <summary>Lets the player skip the remaining break time.</summary>
        public void SkipBreak()
        {
            if (Phase == WavePhase.Break) _breakEndsAt = Time.time;
        }

        private void Update()
        {
            switch (Phase)
            {
                case WavePhase.Break when Time.time >= _breakEndsAt:
                    StartWave();
                    break;
                case WavePhase.Combat:
                    TickCombat();
                    break;
            }
        }

        private void StartBreak(float duration)
        {
            Phase = WavePhase.Break;
            _breakEndsAt = Time.time + duration;
            BreakStarted?.Invoke(CurrentWave + 1, duration);
        }

        private void StartWave()
        {
            CurrentWave++;
            Phase = WavePhase.Combat;
            _toSpawn = _composer.Compose(CurrentWave);
            _nextSpawnTime = Time.time;
            WaveStarted?.Invoke(CurrentWave);
            EnemiesRemainingChanged?.Invoke(EnemiesRemaining);
        }

        private void TickCombat()
        {
            if (_toSpawn.Count > 0 && Time.time >= _nextSpawnTime)
            {
                _nextSpawnTime = Time.time + settings.spawnInterval;
                _spawner.Spawn(_toSpawn.Dequeue(), settings.HealthMultiplierFor(CurrentWave));
                AliveEnemies++;
            }

            if (_toSpawn.Count == 0 && AliveEnemies == 0)
            {
                WaveCleared?.Invoke(CurrentWave);
                StartBreak(settings.breakDuration);
            }
        }

        private void OnEnemyKilled(Enemy enemy, DamageInfo _)
        {
            AliveEnemies = Mathf.Max(0, AliveEnemies - 1);
            EnemiesRemainingChanged?.Invoke(EnemiesRemaining);
        }
    }
}
