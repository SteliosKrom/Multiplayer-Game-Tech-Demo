using System;
using UnityEngine;

namespace WaveSurvival.Progression
{
    /// <summary>Owns level and XP rules. Mutates the profile, raises events, never touches UI.</summary>
    public sealed class ProgressionService
    {
        private readonly PlayerProfile _profile;
        private readonly ProgressionSettings _settings;

        public ProgressionService(PlayerProfile profile, ProgressionSettings settings)
        {
            _profile = profile;
            _settings = settings;
        }

        public int Level => _profile.level;
        public int Xp => _profile.xp;
        public int XpToNextLevel => XpRequiredFor(_profile.level);

        /// <summary>(xp, xpToNext)</summary>
        public event Action<int, int> XpChanged;
        public event Action<int> LevelChanged;

        public int XpRequiredFor(int level) =>
            Mathf.RoundToInt(_settings.baseXp * Mathf.Pow(Mathf.Max(1, level), _settings.exponent));

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            _profile.xp += amount;

            bool leveledUp = false;
            while (_profile.xp >= XpToNextLevel)
            {
                _profile.xp -= XpToNextLevel;
                _profile.level++;
                leveledUp = true;
            }

            if (leveledUp) LevelChanged?.Invoke(_profile.level);
            XpChanged?.Invoke(_profile.xp, XpToNextLevel);
        }
    }
}
