using System;
using WaveSurvival.Combat;
using WaveSurvival.Progression;
using WaveSurvival.Waves;
using WaveSurvival.Weapons;

namespace WaveSurvival.UI
{
    /// <summary>Listens to game services and pushes values into the HudView. No game rules here.</summary>
    public sealed class HudPresenter : ITickable, IDisposable
    {
        private readonly HudView _view;
        private readonly Health _health;
        private readonly ProgressionService _progression;
        private readonly IWallet _wallet;
        private readonly ScoreTracker _score;
        private readonly WaveDirector _waves;
        private readonly WeaponHolder _weapons;

        public HudPresenter(HudView view, Health health, ProgressionService progression, IWallet wallet,
            ScoreTracker score, WaveDirector waves, WeaponHolder weapons)
        {
            _view = view;
            _health = health;
            _progression = progression;
            _wallet = wallet;
            _score = score;
            _waves = waves;
            _weapons = weapons;

            _health.Changed += _view.SetHealth;
            _progression.XpChanged += OnXpChanged;
            _progression.LevelChanged += OnLevelChanged;
            _wallet.GoldChanged += _view.SetGold;
            _score.ScoreChanged += _view.SetScore;
            _waves.WaveStarted += OnWaveStarted;
            _waves.BreakStarted += OnBreakStarted;
            _waves.WaveCleared += OnWaveCleared;
            _waves.EnemiesRemainingChanged += _view.SetEnemiesRemaining;
            _weapons.Equipped += OnEquipped;

            RefreshAll();
        }

        public void Dispose()
        {
            _health.Changed -= _view.SetHealth;
            _progression.XpChanged -= OnXpChanged;
            _progression.LevelChanged -= OnLevelChanged;
            _wallet.GoldChanged -= _view.SetGold;
            _score.ScoreChanged -= _view.SetScore;
            _waves.WaveStarted -= OnWaveStarted;
            _waves.BreakStarted -= OnBreakStarted;
            _waves.WaveCleared -= OnWaveCleared;
            _waves.EnemiesRemainingChanged -= _view.SetEnemiesRemaining;
            _weapons.Equipped -= OnEquipped;
        }

        public void Tick() => _view.SetBreakTimer(_waves.BreakTimeRemaining);

        private void RefreshAll()
        {
            _view.SetHealth(_health.Current, _health.Max);
            _view.SetXp(_progression.Level, _progression.Xp, _progression.XpToNextLevel);
            _view.SetGold(_wallet.Gold);
            _view.SetScore(_score.Score);
            _view.SetWave(_waves.CurrentWave);
            _view.SetEnemiesRemaining(_waves.EnemiesRemaining);
            _view.SetWeapon(_weapons.Current != null ? _weapons.Current.displayName : "-");
            _view.SetHint(ControlsHint);
        }

        private const string ControlsHint = "WASD move  |  Mouse aim  |  LMB shoot  |  1-4 weapons  |  B shop  |  Esc pause";

        private void OnXpChanged(int xp, int xpToNext) => _view.SetXp(_progression.Level, xp, xpToNext);
        private void OnLevelChanged(int level) => _view.ShowToast($"LEVEL UP!  Level {level}");
        private void OnEquipped(WeaponDefinition weapon) => _view.SetWeapon(weapon.displayName);

        private void OnWaveStarted(int wave)
        {
            _view.SetWave(wave);
            _view.SetHint(ControlsHint);
            _view.ShowToast($"WAVE {wave}");
        }

        private void OnBreakStarted(int nextWave, float duration) => _view.SetHint("Break! Press B to open the shop");
        private void OnWaveCleared(int wave) => _view.ShowToast($"Wave {wave} cleared!");
    }
}
