using WaveSurvival.Core;
using WaveSurvival.Enemies;

namespace WaveSurvival.Progression
{
    /// <summary>Translates game events into XP, gold and score. One reason to change: reward rules.</summary>
    public sealed class RewardService
    {
        private readonly ProgressionService _progression;
        private readonly IWallet _wallet;
        private readonly ScoreTracker _score;
        private readonly int _waveClearBonus;

        public RewardService(ProgressionService progression, IWallet wallet, ScoreTracker score, int waveClearBonus)
        {
            _progression = progression;
            _wallet = wallet;
            _score = score;
            _waveClearBonus = waveClearBonus;
        }

        public void OnEnemyKilled(Enemy enemy, DamageInfo killingBlow)
        {
            EnemyDefinition definition = enemy.Definition;
            _progression.AddXp(definition.xpReward);
            _wallet.Add(definition.goldReward);
            _score.AddKill(definition.scoreReward);
        }

        public void OnWaveCleared(int wave)
        {
            _score.Add(_waveClearBonus * wave);
            _wallet.Add(10 * wave);
        }
    }
}
