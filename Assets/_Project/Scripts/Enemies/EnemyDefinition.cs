using UnityEngine;

namespace WaveSurvival.Enemies
{
    /// <summary>Designer data for one enemy type.</summary>
    [CreateAssetMenu(menuName = "Wave Survival/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string id = "grunt";
        public string displayName = "Grunt";
        public Enemy prefab;

        [Header("Stats")]
        [Min(1f)] public float maxHealth = 50f;
        [Min(0.5f)] public float moveSpeed = 3.5f;
        [Min(0f)] public float damage = 10f;
        [Min(0.5f)] public float attackRange = 1.6f;
        [Min(0.1f)] public float attackCooldown = 1f;

        [Header("Rewards")]
        [Min(0)] public int xpReward = 10;
        [Min(0)] public int goldReward = 5;
        [Min(0)] public int scoreReward = 10;
    }
}
