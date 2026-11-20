using UnityEngine;

namespace WaveSurvival.Progression
{
    /// <summary>Derived numbers for the player character at a given level.</summary>
    public readonly struct PlayerStats
    {
        public readonly float MaxHealth;
        public readonly float DamageMultiplier;
        public readonly float MoveSpeed;

        public PlayerStats(float maxHealth, float damageMultiplier, float moveSpeed)
        {
            MaxHealth = maxHealth;
            DamageMultiplier = damageMultiplier;
            MoveSpeed = moveSpeed;
        }
    }

    /// <summary>Only job: level in, stats out. Easy to unit test.</summary>
    public sealed class PlayerStatsCalculator
    {
        private readonly ProgressionSettings _settings;

        public PlayerStatsCalculator(ProgressionSettings settings) => _settings = settings;

        public PlayerStats Calculate(int level)
        {
            int bonusLevels = Mathf.Max(0, level - 1);
            return new PlayerStats(
                _settings.baseHealth + _settings.healthPerLevel * bonusLevels,
                1f + _settings.damageBonusPerLevel * bonusLevels,
                _settings.moveSpeed);
        }
    }
}
