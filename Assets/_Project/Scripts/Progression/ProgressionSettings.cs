using UnityEngine;

namespace WaveSurvival.Progression
{
    /// <summary>Tuning numbers for the RPG side: XP curve and stats per level.</summary>
    [CreateAssetMenu(menuName = "Wave Survival/Progression Settings")]
    public class ProgressionSettings : ScriptableObject
    {
        [Header("XP curve: xpToNext = baseXp * level ^ exponent")]
        public int baseXp = 100;
        public float exponent = 1.4f;

        [Header("Stats")]
        public float baseHealth = 100f;
        public float healthPerLevel = 10f;
        public float damageBonusPerLevel = 0.05f;
        public float moveSpeed = 6f;

        [Header("Shop")]
        public int medkitPrice = 40;
    }
}
