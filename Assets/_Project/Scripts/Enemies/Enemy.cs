using System;
using UnityEngine;
using WaveSurvival.Combat;
using WaveSurvival.Core;

namespace WaveSurvival.Enemies
{
    /// <summary>
    /// Composes Health + EnemyMovement + EnemyAttack into a simple "chase and hit" brain.
    /// Dependencies (definition, target provider) are passed in through Init, not searched for.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(EnemyMovement), typeof(EnemyAttack))]
    public class Enemy : MonoBehaviour
    {
        private Health _health;
        private EnemyMovement _movement;
        private EnemyAttack _attack;
        private ITargetProvider _targets;

        public EnemyDefinition Definition { get; private set; }
        public Health Health => _health;

        /// <summary>(enemy, killing blow)</summary>
        public event Action<Enemy, DamageInfo> Killed;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _movement = GetComponent<EnemyMovement>();
            _attack = GetComponent<EnemyAttack>();
        }

        public void Init(EnemyDefinition definition, ITargetProvider targets, float healthMultiplier)
        {
            Definition = definition;
            _targets = targets;

            _health.SetMax(definition.maxHealth * healthMultiplier, refill: true);
            _movement.Configure(definition.moveSpeed, definition.attackRange);
            _attack.Configure(definition.damage, definition.attackRange, definition.attackCooldown);

            _health.Died += OnDied;
        }

        private void Update()
        {
            if (_targets == null || !_health.IsAlive) return;

            if (!_targets.TryGetClosestTarget(transform.position, out Transform target))
            {
                _movement.Stop();
                return;
            }

            if (_attack.Tick(target)) _movement.Stop();
            else _movement.Chase(target.position);
        }

        private void OnDied(DamageInfo killingBlow)
        {
            _health.Died -= OnDied;
            _movement.Stop();
            Killed?.Invoke(this, killingBlow);
            Destroy(gameObject);
        }
    }
}
