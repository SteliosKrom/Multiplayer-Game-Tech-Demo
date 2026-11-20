using System;
using UnityEngine;
using WaveSurvival.Core;

namespace WaveSurvival.Combat
{
    /// <summary>Owns hit points only. It does not know who owns it or what happens on death.</summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        public float Current { get; private set; }
        public float Max => maxHealth;
        public bool IsAlive => Current > 0f;

        /// <summary>(current, max)</summary>
        public event Action<float, float> Changed;
        public event Action<DamageInfo> Damaged;
        /// <summary>Raised once, with the hit that killed it.</summary>
        public event Action<DamageInfo> Died;

        private void Awake() => Current = maxHealth;

        public void SetMax(float max, bool refill)
        {
            maxHealth = Mathf.Max(1f, max);
            Current = refill ? maxHealth : Mathf.Min(Current, maxHealth);
            Changed?.Invoke(Current, maxHealth);
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            Current = Mathf.Min(maxHealth, Current + amount);
            Changed?.Invoke(Current, maxHealth);
        }

        public void TakeDamage(DamageInfo damage)
        {
            if (!IsAlive || damage.Amount <= 0f) return;

            Current = Mathf.Max(0f, Current - damage.Amount);
            Damaged?.Invoke(damage);
            Changed?.Invoke(Current, maxHealth);

            if (!IsAlive) Died?.Invoke(damage);
        }
    }
}
