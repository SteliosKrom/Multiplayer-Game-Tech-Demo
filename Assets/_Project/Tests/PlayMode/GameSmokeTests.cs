using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using WaveSurvival.Enemies;
using WaveSurvival.Player;
using WaveSurvival.UI;
using WaveSurvival.Waves;

namespace WaveSurvival.Tests
{
    /// <summary>
    /// End-to-end smoke tests: load the real scenes and check that the core loop works.
    /// They also save screenshots to "Captures/" in the project folder for a quick visual check.
    /// </summary>
    public class GameSmokeTests
    {
        private string _profilePath;
        private string _backupPath;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _profilePath = Path.Combine(Application.persistentDataPath, "profile.json");
            _backupPath = _profilePath + ".testbackup";
            if (File.Exists(_profilePath)) File.Copy(_profilePath, _backupPath, true);
            if (File.Exists(_profilePath)) File.Delete(_profilePath);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            if (File.Exists(_backupPath)) File.Copy(_backupPath, _profilePath, true);
            else if (File.Exists(_profilePath)) File.Delete(_profilePath);
            if (File.Exists(_backupPath)) File.Delete(_backupPath);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MainMenu_ShowsDisabledMultiplayerWithComingSoon()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return new WaitForSeconds(0.5f);

            var multiplayer = GameObject.Find("MultiplayerButton").GetComponent<Button>();
            Assert.IsFalse(multiplayer.interactable, "Multiplayer must be disabled in the singleplayer build");
            Assert.IsTrue(GameObject.Find("ComingSoonBadge").activeInHierarchy, "COMING SOON label must be visible");

            Capture("main_menu");
        }

        [UnityTest]
        public IEnumerator Game_WaveSpawnsEnemies_PlayerCanKillThem()
        {
            SceneManager.LoadScene(SceneNames.Game);

            PlayerController player = null;
            for (float t = 0; t < 5f && player == null; t += Time.deltaTime)
            {
                player = Object.FindAnyObjectByType<PlayerController>();
                yield return null;
            }
            Assert.IsNotNull(player, "Player was not spawned by the GameInstaller");
            Assert.IsNotNull(player.Weapons.Current, "Player should start with the default pistol");

            var director = Object.FindAnyObjectByType<WaveDirector>();
            Assert.AreEqual(WavePhase.Break, director.Phase);
            Capture("game_break");

            director.SkipBreak();
            yield return new WaitForSeconds(2.5f);
            Assert.AreEqual(WavePhase.Combat, director.Phase);
            Assert.Greater(Object.FindObjectsByType<Enemy>().Length, 0, "Enemies should spawn in wave 1");

            // Let enemies approach, then turn to the nearest enemy and shoot until it dies.
            yield return new WaitForSeconds(3f);
            Capture("game_combat");

            int remainingBefore = director.EnemiesRemaining;
            for (float t = 0; t < 12f && director.EnemiesRemaining >= remainingBefore; t += Time.deltaTime)
            {
                Enemy target = Closest(player.transform.position);
                if (target != null)
                {
                    Vector3 look = target.transform.position - player.transform.position;
                    look.y = 0f;
                    player.transform.rotation = Quaternion.LookRotation(look);
                    player.Weapons.TryFire(10f);
                }
                yield return null;
            }

            Assert.Less(director.EnemiesRemaining, remainingBefore, "Shooting should kill at least one enemy");
            Capture("game_after_kill");
        }

        [UnityTest]
        public IEnumerator Shop_BuyWeaponDuringBreak_IsSavedToProfile()
        {
            File.WriteAllText(_profilePath, "{\"level\":2,\"xp\":0,\"gold\":1000,\"ownedWeapons\":[\"pistol\"],\"highScore\":0,\"bestWave\":0}");
            // Batch mode has no focused Game view: make the Input System accept our fake key presses anyway.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                SceneManager.LoadScene(SceneNames.Game);
                PlayerController player = null;
                for (float t = 0; t < 5f && player == null; t += Time.deltaTime)
                {
                    player = Object.FindAnyObjectByType<PlayerController>();
                    yield return null;
                }
                Assert.IsNotNull(player);
                Assert.AreEqual(110f, player.Health.Max, 0.01f, "Level 2 should give +10 max health");

                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.B));
                yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;

                var shop = Object.FindAnyObjectByType<ShopView>();
                Assert.IsTrue(shop.IsVisible, "B should open the shop during the break");
                Capture("shop");

                var items = shop.GetComponentsInChildren<ShopItemView>();
                Assert.AreEqual(4, items.Length);
                items[1].GetComponentInChildren<Button>().onClick.Invoke(); // SMG
                yield return new WaitForSeconds(0.5f);

                Assert.AreEqual("smg", player.Weapons.Current.id, "Buying should equip the weapon");
                string saved = File.ReadAllText(_profilePath);
                StringAssert.Contains("smg", saved, "Purchase must be saved immediately");
                StringAssert.Contains("\"gold\": 850", saved, "SMG costs 150 gold");
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
            }
        }

        private static Enemy Closest(Vector3 from)
        {
            Enemy best = null;
            float bestDistance = float.MaxValue;
            foreach (var enemy in Object.FindObjectsByType<Enemy>())
            {
                float d = (enemy.transform.position - from).sqrMagnitude;
                if (d < bestDistance) { bestDistance = d; best = enemy; }
            }
            return best;
        }

        /// <summary>Renders the main camera (with overlay canvases temporarily moved into camera space) to a PNG.</summary>
        private static void Capture(string name)
        {
            var camera = Camera.main;
            if (camera == null) return;

            var canvases = Object.FindObjectsByType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
            }
            Canvas.ForceUpdateCanvases();

            var rt = new RenderTexture(1600, 900, 24);
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();
            camera.targetTexture = null;
            RenderTexture.active = null;

            string dir = Path.Combine(Application.dataPath, "..", "Captures");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), texture.EncodeToPNG());

            foreach (var canvas in canvases)
            {
                if (canvas.worldCamera != camera) continue;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            Object.Destroy(texture);
            rt.Release();
        }
    }
}
