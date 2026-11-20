using System;
using WaveSurvival.Combat;
using WaveSurvival.Core;
using WaveSurvival.Persistence;
using WaveSurvival.Progression;
using WaveSurvival.UI;
using WaveSurvival.Waves;

namespace WaveSurvival.Bootstrap
{
    public readonly struct RunResult
    {
        public readonly int Score;
        public readonly int Wave;
        public readonly int Kills;
        public readonly int HighScore;
        public readonly bool IsNewHighScore;

        public RunResult(int score, int wave, int kills, int highScore, bool isNewHighScore)
        {
            Score = score;
            Wave = wave;
            Kills = kills;
            HighScore = highScore;
            IsNewHighScore = isNewHighScore;
        }
    }

    /// <summary>Owns the lifetime of one run: detects the end, records records and saves.</summary>
    public sealed class GameSession : IDisposable
    {
        private readonly ProfileSession _profile;
        private readonly ScoreTracker _score;
        private readonly WaveDirector _waves;
        private readonly Health _playerHealth;
        private readonly UiState _ui;

        public event Action<RunResult> Ended;

        public GameSession(ProfileSession profile, ScoreTracker score, WaveDirector waves, Health playerHealth, UiState ui)
        {
            _profile = profile;
            _score = score;
            _waves = waves;
            _playerHealth = playerHealth;
            _ui = ui;
            _playerHealth.Died += OnPlayerDied;
        }

        public void Dispose() => _playerHealth.Died -= OnPlayerDied;

        private void OnPlayerDied(DamageInfo _)
        {
            _waves.Stop();
            _ui.GameOver = true;
            _ui.ShopOpen = false;

            PlayerProfile profile = _profile.Profile;
            bool isNewHighScore = _score.Score > profile.highScore;
            if (isNewHighScore) profile.highScore = _score.Score;
            if (_waves.CurrentWave > profile.bestWave) profile.bestWave = _waves.CurrentWave;

            _profile.SaveInBackground();
            Ended?.Invoke(new RunResult(_score.Score, _waves.CurrentWave, _score.Kills, profile.highScore, isNewHighScore));
        }
    }
}
