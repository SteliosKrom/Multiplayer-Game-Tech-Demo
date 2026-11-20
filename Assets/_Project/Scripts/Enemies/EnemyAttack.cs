using UnityEngine;
using WaveSurvival.Core;

namespace WaveSurvival.Enemies
{
    /// <summary>Melee attack with a cooldown. Only knows range, damage and timing.</summary>
    public class EnemyAttack : MonoBehaviour
    {
        private float _damage;
        private float _range;
        private float _cooldown;
        private float _nextAttackTime;

        public float Range => _range;

        public void Configure(float damage, float range, float cooldown)
        {
            _damage = damage;
            _range = range;
            _cooldown = cooldown;
        }

        /// <summary>Returns true while the target is inside attack range.</summary>
        public bool Tick(Transform target)
        {
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > _range * _range) return false;

            if (toTarget.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(toTarget);

            if (Time.time >= _nextAttackTime && target.TryGetComponent(out IDamageable damageable) && damageable.IsAlive)
            {
                _nextAttackTime = Time.time + _cooldown;
                damageable.TakeDamage(new DamageInfo(_damage, target.position, gameObject));
            }

            return true;
        }
    }
}
