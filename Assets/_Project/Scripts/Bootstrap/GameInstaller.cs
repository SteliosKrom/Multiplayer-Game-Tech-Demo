using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using WaveSurvival.CameraRig;
using WaveSurvival.Core;
using WaveSurvival.Enemies;
using WaveSurvival.Input;
using WaveSurvival.Persistence;
using WaveSurvival.Player;
using WaveSurvival.Progression;
using WaveSurvival.UI;
using WaveSurvival.Waves;
using WaveSurvival.Weapons;

namespace WaveSurvival.Bootstrap
{
    /// <summary>
    /// Composition root of the Game scene. The ONLY place that knows concrete types and creates objects.
    /// Everything else receives what it needs through constructors or Construct(...) methods
    /// (simple manual Dependency Injection, no framework).
    /// </summary>
    public class GameInstaller : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WeaponCatalog weaponCatalog;
        [SerializeField] private ProgressionSettings progressionSettings;

        [Header("Scene")]
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private TopDownCamera cameraRig;
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private TracerPool tracerPool;

        [Header("UI")]
        [SerializeField] private HudView hudView;
        [SerializeField] private ShopView shopView;
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private PauseView pauseView;

        private readonly List<ITickable> _tickables = new();
        private readonly List<IDisposable> _disposables = new();

        private async void Start()
        {
            // 1. Load persistent data first: everything else depends on it.
            var profileSession = new ProfileSession(new JsonFileProfileRepository());
            await profileSession.LoadAsync();
            if (this == null) return; // scene was unloaded while loading

            PlayerProfile profile = profileSession.Profile;
            navMeshSurface.BuildNavMesh();

            // 2. Pure C# services (no MonoBehaviours).
            var input = new InputSystemPlayerInput();
            var ui = new UiState();
            var progression = new ProgressionService(profile, progressionSettings);
            var wallet = new Wallet(profile);
            var inventory = new WeaponInventory(profile, weaponCatalog.DefaultOwnedIds());
            var score = new ScoreTracker();
            var stats = new PlayerStatsCalculator(progressionSettings);
            var shop = new ShopService(wallet, inventory, progressionSettings.medkitPrice);
            _disposables.Add(input);

            // 3. Player.
            PlayerController player = Instantiate(playerPrefab, playerSpawn.position, playerSpawn.rotation);
            player.Construct(input, new AimPointResolver(mainCamera), new PlayerLoadout(weaponCatalog, inventory),
                ui, new HitscanResolver(GameLayers.PlayerShotMask), tracerPool);
            player.ApplyStats(stats.Calculate(progression.Level), refillHealth: true);
            progression.LevelChanged += level => player.ApplyStats(stats.Calculate(level), refillHealth: true);
            cameraRig.SetTarget(player.transform);

            // 4. Enemies and waves.
            enemySpawner.Construct(new SinglePlayerTargetProvider(player.transform, player.Health));
            waveDirector.Construct(enemySpawner, new WaveComposer(waveDirector.Settings));

            var rewards = new RewardService(progression, wallet, score, waveDirector.Settings.waveClearScoreBonus);
            enemySpawner.EnemyKilled += rewards.OnEnemyKilled;
            waveDirector.WaveCleared += rewards.OnWaveCleared;

            // 5. Session + presenters.
            var session = new GameSession(profileSession, score, waveDirector, player.Health, ui);
            _disposables.Add(session);

            Register(new HudPresenter(hudView, player.Health, progression, wallet, score, waveDirector, player.Weapons));
            Register(new ShopPresenter(shopView, shop, weaponCatalog, inventory, wallet, player.Weapons,
                player.Health, waveDirector, ui, input, profileSession));
            Register(new PausePresenter(pauseView, input, ui, profileSession));
            Register(new GameOverPresenter(gameOverView, session));

            // 6. Go.
            waveDirector.Begin();
        }

        private void Register(object presenter)
        {
            if (presenter is ITickable tickable) _tickables.Add(tickable);
            if (presenter is IDisposable disposable) _disposables.Add(disposable);
        }

        private void Update()
        {
            for (int i = 0; i < _tickables.Count; i++) _tickables[i].Tick();
        }

        private void OnDestroy()
        {
            foreach (var disposable in _disposables) disposable.Dispose();
            _disposables.Clear();
            _tickables.Clear();
        }
    }
}
