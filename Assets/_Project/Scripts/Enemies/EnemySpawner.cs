using System;
using UnityEngine;
using UnityEngine.AI;
using WaveSurvival.Core;

namespace WaveSurvival.Enemies
{
    public interface IEnemySpawner
    {
        /// <summary>Raised when any spawned enemy dies, with the killing blow.</summary>
        event Action<Enemy, DamageInfo> EnemyKilled;
        Enemy Spawn(EnemyDefinition definition, float healthMultiplier);
    }

    /// <summary>Creates enemies at random spawn points and reports their deaths. Does not decide WHAT to spawn.</summary>
    public class EnemySpawner : MonoBehaviour, IEnemySpawner
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnJitter = 2f;

        private ITargetProvider _targets;

        public event Action<Enemy, DamageInfo> EnemyKilled;

        public void Construct(ITargetProvider targets) => _targets = targets;

        public Enemy Spawn(EnemyDefinition definition, float healthMultiplier)
        {
            Vector3 position = PickSpawnPosition();
            Enemy enemy = Instantiate(definition.prefab, position, Quaternion.identity, transform);
            enemy.Init(definition, _targets, healthMultiplier);
            enemy.Killed += HandleKilled;
            return enemy;
        }

        private void HandleKilled(Enemy enemy, DamageInfo killingBlow)
        {
            enemy.Killed -= HandleKilled;
            EnemyKilled?.Invoke(enemy, killingBlow);
        }

        private Vector3 PickSpawnPosition()
        {
            Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            Vector2 jitter = UnityEngine.Random.insideUnitCircle * spawnJitter;
            Vector3 candidate = point.position + new Vector3(jitter.x, 0f, jitter.y);

            return NavMesh.SamplePosition(candidate, out NavMeshHit hit, 4f, NavMesh.AllAreas) ? hit.position : point.position;
        }
    }
}
