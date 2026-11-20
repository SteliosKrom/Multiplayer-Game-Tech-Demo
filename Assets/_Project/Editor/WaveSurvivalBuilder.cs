using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WaveSurvival.Bootstrap;
using WaveSurvival.CameraRig;
using WaveSurvival.Combat;
using WaveSurvival.Core;
using WaveSurvival.Enemies;
using WaveSurvival.Player;
using WaveSurvival.Progression;
using WaveSurvival.UI;
using WaveSurvival.Waves;
using WaveSurvival.Weapons;
using static WaveSurvival.EditorTools.UiFactory;
using Object = UnityEngine.Object;

namespace WaveSurvival.EditorTools
{
    /// <summary>
    /// Generates every asset the game needs (materials, data, prefabs, scenes) from code.
    /// Menu: Wave Survival/Rebuild All Content. Safe to run again: assets are overwritten in place.
    /// </summary>
    public static class WaveSurvivalBuilder
    {
        private const string Root = "Assets/_Project";
        private const string MaterialsDir = Root + "/Materials";
        private const string DataDir = Root + "/Data";
        private const string PrefabsDir = Root + "/Prefabs";
        private const string ScenesDir = Root + "/Scenes";

        private static readonly Dictionary<string, Material> Materials = new();

        // ------------------------------------------------------------------ entry points

        [MenuItem("Wave Survival/Import TMP Essentials")]
        public static void ImportTmpEssentials()
        {
            if (TMP_Settings.instance != null && TMP_Settings.defaultFontAsset != null)
            {
                Debug.Log("[Builder] TMP essentials already present.");
                return;
            }

            string[] candidates =
            {
                "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage",
                "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage",
            };
            foreach (string candidate in candidates)
            {
                string full = Path.GetFullPath(candidate);
                if (!File.Exists(full)) continue;
                AssetDatabase.ImportPackage(full, false);
                AssetDatabase.Refresh();
                Debug.Log("[Builder] Imported TMP essentials from " + candidate);
                return;
            }
            Debug.LogError("[Builder] Could not find TMP Essential Resources package.");
        }

        public static void ImportTmpEssentialsBatch() => RunBatch(ImportTmpEssentials);
        public static void BuildAllBatch() => RunBatch(BuildAll);

        [MenuItem("Wave Survival/Rebuild All Content")]
        public static void BuildAll()
        {
            EnsureFolders();
            WhiteSprite = CreateWhiteSprite();
            CreateMaterials();

            var weapons = CreateWeapons();
            CreateCatalog(weapons);
            CreateProgressionSettings();
            var enemies = CreateEnemies();
            CreateWaveSettings(enemies);

            CreateTracerPrefab();
            CreatePlayerPrefab();
            CreateShopItemPrefab();

            // Creating a new scene unloads unreferenced assets, so save everything to disk
            // now and re-load by path inside each scene builder.
            AssetDatabase.SaveAssets();

            BuildMainMenuScene();
            BuildGameScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesDir}/{SceneNames.MainMenu}.unity", true),
                new EditorBuildSettingsScene($"{ScenesDir}/{SceneNames.Game}.unity", true),
            };

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene($"{ScenesDir}/{SceneNames.MainMenu}.unity");
            Debug.Log("[Builder] Wave Survival content rebuilt.");
        }

        private static void RunBatch(Action action)
        {
            try
            {
                action();
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        // ------------------------------------------------------------------ basics

        private static void EnsureFolders()
        {
            foreach (string dir in new[] { MaterialsDir, DataDir, PrefabsDir, ScenesDir })
                Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        private static Sprite CreateWhiteSprite()
        {
            string path = MaterialsDir + "/White.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(8, 8);
                var pixels = new Color[64];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                texture.SetPixels(pixels);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = new Vector4(2, 2, 2, 2);
                importer.SaveAndReimport();
            }
            return Load<Sprite>(path);
        }

        private static void CreateMaterials()
        {
            Materials.Clear();
            Lit("Ground", new Color(0.32f, 0.36f, 0.3f));
            Lit("Wall", new Color(0.45f, 0.42f, 0.38f));
            Lit("Obstacle", new Color(0.55f, 0.4f, 0.25f));
            Lit("Player", new Color(0.2f, 0.55f, 0.95f));
            Lit("PlayerAccent", new Color(0.1f, 0.1f, 0.12f));
            Lit("Grunt", new Color(0.85f, 0.2f, 0.2f));
            Lit("Runner", new Color(0.95f, 0.6f, 0.1f));
            Lit("Brute", new Color(0.55f, 0.2f, 0.75f));
            Lit("Eyes", new Color(1f, 0.95f, 0.6f));

            var tracer = LoadOrCreate($"{MaterialsDir}/Tracer.mat", () => new Material(Shader.Find("Sprites/Default")));
            tracer.shader = Shader.Find("Sprites/Default");
            Materials["Tracer"] = tracer;
        }

        private static void Lit(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = LoadOrCreate($"{MaterialsDir}/{name}.mat", () => new Material(shader));
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.2f);
            EditorUtility.SetDirty(material);
            Materials[name] = material;
        }

        private static T LoadOrCreate<T>(string path, Func<T> create) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = create();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException($"[Builder] Missing asset {path}");
            return asset;
        }

        private static T LoadPrefab<T>(string name) where T : Component =>
            Load<GameObject>($"{PrefabsDir}/{name}.prefab").GetComponent<T>();

        private static T Data<T>(string name) where T : ScriptableObject =>
            LoadOrCreate($"{DataDir}/{name}.asset", ScriptableObject.CreateInstance<T>);

        // ------------------------------------------------------------------ data

        private static List<WeaponDefinition> CreateWeapons()
        {
            var list = new List<WeaponDefinition>
            {
                Weapon("Weapon_Pistol", "pistol", "Pistol", 0, true, 20, 3f, 1, 2f, 35f, new Color(1f, 0.9f, 0.4f)),
                Weapon("Weapon_SMG", "smg", "SMG", 150, false, 11, 11f, 1, 7f, 30f, new Color(0.5f, 1f, 0.6f)),
                Weapon("Weapon_Shotgun", "shotgun", "Shotgun", 300, false, 12, 1.3f, 7, 22f, 18f, new Color(1f, 0.55f, 0.3f)),
                Weapon("Weapon_Rifle", "rifle", "Battle Rifle", 550, false, 45, 4.5f, 1, 1f, 50f, new Color(0.5f, 0.8f, 1f)),
            };
            return list;
        }

        private static WeaponDefinition Weapon(string asset, string id, string displayName, int price, bool owned,
            float damage, float fireRate, int pellets, float spread, float range, Color tracer)
        {
            var w = Data<WeaponDefinition>(asset);
            w.id = id;
            w.displayName = displayName;
            w.price = price;
            w.ownedByDefault = owned;
            w.damage = damage;
            w.fireRate = fireRate;
            w.pellets = pellets;
            w.spreadAngle = spread;
            w.range = range;
            w.tracerColor = tracer;
            EditorUtility.SetDirty(w);
            return w;
        }

        private static WeaponCatalog CreateCatalog(List<WeaponDefinition> weapons)
        {
            var catalog = Data<WeaponCatalog>("WeaponCatalog");
            catalog.SetWeapons(weapons);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static ProgressionSettings CreateProgressionSettings()
        {
            var settings = Data<ProgressionSettings>("ProgressionSettings");
            EditorUtility.SetDirty(settings);
            return settings;
        }

        private static List<EnemyDefinition> CreateEnemies()
        {
            return new List<EnemyDefinition>
            {
                Enemy("Enemy_Grunt", "grunt", "Grunt", "Grunt", 1f, 50, 3.5f, 10, 1.6f, 1f, 10, 5, 10),
                Enemy("Enemy_Runner", "runner", "Runner", "Runner", 0.8f, 25, 6f, 6, 1.4f, 0.7f, 8, 4, 12),
                Enemy("Enemy_Brute", "brute", "Brute", "Brute", 1.5f, 180, 2.3f, 25, 2.2f, 1.6f, 35, 18, 40),
            };
        }

        private static EnemyDefinition Enemy(string asset, string id, string displayName, string material, float scale,
            float health, float speed, float damage, float range, float cooldown, int xp, int gold, int score)
        {
            var e = Data<EnemyDefinition>(asset);
            e.id = id;
            e.displayName = displayName;
            e.maxHealth = health;
            e.moveSpeed = speed;
            e.damage = damage;
            e.attackRange = range;
            e.attackCooldown = cooldown;
            e.xpReward = xp;
            e.goldReward = gold;
            e.scoreReward = score;
            e.prefab = CreateEnemyPrefab(displayName, material, scale);
            EditorUtility.SetDirty(e);
            return e;
        }

        private static WaveSettings CreateWaveSettings(List<EnemyDefinition> enemies)
        {
            var waves = Data<WaveSettings>("WaveSettings");
            waves.enemies = new List<WaveEnemyEntry>
            {
                new() { enemy = enemies[0], weight = 1f, firstWave = 1 },
                new() { enemy = enemies[1], weight = 0.6f, firstWave = 2 },
                new() { enemy = enemies[2], weight = 0.25f, firstWave = 4 },
            };
            EditorUtility.SetDirty(waves);
            return waves;
        }

        // ------------------------------------------------------------------ prefabs

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 position,
            Vector3 scale, string material, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Materials[material];
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform) SetLayerRecursive(child.gameObject, layer);
        }

        private static T SavePrefab<T>(GameObject go, string name) where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabsDir}/{name}.prefab");
            Object.DestroyImmediate(go);
            return prefab.GetComponent<T>();
        }

        private static LineRenderer CreateTracerPrefab()
        {
            var go = new GameObject("Tracer");
            var line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = 0.07f;
            line.endWidth = 0.03f;
            line.sharedMaterial = Materials["Tracer"];
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            return SavePrefab<LineRenderer>(go, "Tracer");
        }

        private static PlayerController CreatePlayerPrefab()
        {
            var root = new GameObject("Player");
            var controller = root.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 1f, 0f);
            controller.height = 2f;
            controller.radius = 0.4f;

            Primitive(PrimitiveType.Capsule, "Body", root.transform, new Vector3(0f, 1f, 0f), Vector3.one, "Player");
            Primitive(PrimitiveType.Cube, "Visor", root.transform, new Vector3(0f, 1.55f, 0.35f), new Vector3(0.6f, 0.18f, 0.2f), "PlayerAccent");
            Primitive(PrimitiveType.Cube, "Gun", root.transform, new Vector3(0.35f, 1.2f, 0.45f), new Vector3(0.14f, 0.14f, 0.75f), "PlayerAccent");
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(root.transform, false);
            muzzle.localPosition = new Vector3(0.35f, 1.2f, 0.85f);

            root.AddComponent<Health>();
            root.AddComponent<HitFlash>();
            root.AddComponent<PlayerMotor>();
            root.AddComponent<PlayerAim>();
            var holder = root.AddComponent<WeaponHolder>();
            Set(holder, "muzzle", muzzle);
            root.AddComponent<PlayerController>();

            SetLayerRecursive(root, GameLayers.Player);
            return SavePrefab<PlayerController>(root, "Player");
        }

        private static Enemy CreateEnemyPrefab(string name, string material, float scale)
        {
            var root = new GameObject("Enemy_" + name);
            root.transform.localScale = Vector3.one * scale;

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1f, 0f);
            collider.height = 2f;
            collider.radius = 0.5f;
            var body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;

            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f;
            agent.height = 2f;
            agent.acceleration = 30f;
            agent.angularSpeed = 720f;

            Primitive(PrimitiveType.Capsule, "Body", root.transform, new Vector3(0f, 1f, 0f), Vector3.one, material);
            Primitive(PrimitiveType.Cube, "Eyes", root.transform, new Vector3(0f, 1.5f, 0.42f), new Vector3(0.55f, 0.12f, 0.12f), "Eyes");

            root.AddComponent<Health>();
            var flash = root.AddComponent<HitFlash>();
            SetArray(flash, "renderers", new Object[] { root.transform.Find("Body").GetComponent<Renderer>() });
            root.AddComponent<EnemyMovement>();
            root.AddComponent<EnemyAttack>();
            root.AddComponent<Enemy>();

            SetLayerRecursive(root, GameLayers.Enemy);
            return SavePrefab<Enemy>(root, "Enemy_" + name);
        }

        private static ShopItemView CreateShopItemPrefab()
        {
            var canvas = CreateCanvas("TempCanvas");
            var row = Image("ShopItem", canvas.transform, new Color(1f, 1f, 1f, 0.06f));
            row.rectTransform.sizeDelta = new Vector2(820, 78);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 78;

            var nameText = Text("Name", row.transform, "Weapon", 30, TextAlignmentOptions.Left);
            Place(nameText.rectTransform, new Vector2(0f, 0.5f), new Vector2(20, 12), new Vector2(520, 40));
            var statsText = Text("Stats", row.transform, "stats", 20, TextAlignmentOptions.Left, new Color(0.75f, 0.78f, 0.82f));
            Place(statsText.rectTransform, new Vector2(0f, 0.5f), new Vector2(20, -18), new Vector2(560, 30));

            var button = Button("Action", row.transform, "Buy", out var label, null, 24);
            Place(button.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(-14, 0), new Vector2(210, 54));

            var view = row.gameObject.AddComponent<ShopItemView>();
            Set(view, "nameText", nameText);
            Set(view, "statsText", statsText);
            Set(view, "button", button);
            Set(view, "buttonLabel", label);

            row.transform.SetParent(null, false);
            Object.DestroyImmediate(canvas.gameObject);
            return SavePrefab<ShopItemView>(row.gameObject, "ShopItem");
        }

        // ------------------------------------------------------------------ scenes

        private static Scene NewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // A new scene can unload assets we still hold references to: reload them from disk.
            WhiteSprite = Load<Sprite>(MaterialsDir + "/White.png");
            foreach (string key in new List<string>(Materials.Keys))
                Materials[key] = Load<Material>($"{MaterialsDir}/{key}.mat");
            return scene;
        }

        private static Light CreateSun()
        {
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.6f;
            light.shadows = LightShadows.Soft;
            light.color = new Color(1f, 0.96f, 0.88f);
            go.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            return light;
        }

        private static void BuildMainMenuScene()
        {
            var scene = NewScene();
            var catalog = Load<WeaponCatalog>($"{DataDir}/WeaponCatalog.asset");

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.07f, 0.1f);

            var canvas = CreateCanvas("Canvas");
            CreateEventSystem();

            var bg = Image("Background", canvas.transform, new Color(0.07f, 0.08f, 0.11f));
            Stretch(bg.rectTransform);
            var stripe = Image("Stripe", canvas.transform, new Color(0.95f, 0.55f, 0.15f, 0.15f));
            Place(stripe.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 230), new Vector2(2200, 220));

            var title = Text("Title", canvas.transform, "WAVE SURVIVAL", 120, TextAlignmentOptions.Center, ButtonColor);
            title.fontStyle = FontStyles.Bold;
            Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1400, 150));
            var subtitle = Text("Subtitle", canvas.transform, "Survive the waves. Level up. Buy better guns.", 34);
            Place(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 160), new Vector2(1400, 50));

            var profile = Text("Profile", canvas.transform, "Loading profile...", 28, TextAlignmentOptions.Center, new Color(0.75f, 0.8f, 0.9f));
            Place(profile.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 75), new Vector2(1400, 90));

            var play = Button("SingleplayerButton", canvas.transform, "SINGLEPLAYER", out _);
            Place(play.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(460, 80));

            var multiplayer = Button("MultiplayerButton", canvas.transform, "MULTIPLAYER", out _);
            Place(multiplayer.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, -140), new Vector2(460, 80));
            multiplayer.interactable = false;

            var badge = Image("ComingSoonBadge", multiplayer.transform, new Color(0.95f, 0.85f, 0.2f));
            Place(badge.rectTransform, new Vector2(1f, 0.5f), new Vector2(215, 0), new Vector2(190, 44));
            badge.rectTransform.localRotation = Quaternion.Euler(0, 0, -8f);
            var badgeText = Text("Text", badge.transform, "COMING SOON", 22, TextAlignmentOptions.Center, new Color(0.15f, 0.1f, 0.05f));
            badgeText.fontStyle = FontStyles.Bold;
            Stretch(badgeText.rectTransform);

            var quit = Button("QuitButton", canvas.transform, "QUIT", out _, new Color(0.6f, 0.62f, 0.68f));
            Place(quit.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, -240), new Vector2(460, 80));

            var footer = Text("Footer", canvas.transform, "SAE Networking Course  |  Singleplayer build", 22, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.35f));
            Place(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(1400, 40));

            var view = canvas.gameObject.AddComponent<MainMenuView>();
            Set(view, "playButton", play);
            Set(view, "multiplayerButton", multiplayer);
            Set(view, "comingSoonLabel", badge.gameObject);
            Set(view, "quitButton", quit);
            Set(view, "profileText", profile);

            var bootstrap = new GameObject("MainMenuInstaller").AddComponent<MainMenuInstaller>();
            Set(bootstrap, "view", view);
            Set(bootstrap, "weaponCatalog", catalog);
            SetBool(bootstrap, "multiplayerAvailable", false);

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/{SceneNames.MainMenu}.unity");
        }

        private static void BuildGameScene()
        {
            var scene = NewScene();
            var catalog = Load<WeaponCatalog>($"{DataDir}/WeaponCatalog.asset");
            var progression = Load<ProgressionSettings>($"{DataDir}/ProgressionSettings.asset");
            var waves = Load<WaveSettings>($"{DataDir}/WaveSettings.asset");
            var playerPrefab = LoadPrefab<PlayerController>("Player");
            var tracerPrefab = LoadPrefab<LineRenderer>("Tracer");
            var shopItemPrefab = LoadPrefab<ShopItemView>("ShopItem");
            CreateSun();
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.42f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);

            // Arena
            var arena = new GameObject("Arena");
            int env = GameLayers.Environment;
            const float half = 30f;
            Primitive(PrimitiveType.Cube, "Ground", arena.transform, new Vector3(0, -0.5f, 0), new Vector3(half * 2, 1, half * 2), "Ground", true);
            Primitive(PrimitiveType.Cube, "Wall_N", arena.transform, new Vector3(0, 1.5f, half), new Vector3(half * 2 + 1, 3, 1), "Wall", true);
            Primitive(PrimitiveType.Cube, "Wall_S", arena.transform, new Vector3(0, 1.5f, -half), new Vector3(half * 2 + 1, 3, 1), "Wall", true);
            Primitive(PrimitiveType.Cube, "Wall_E", arena.transform, new Vector3(half, 1.5f, 0), new Vector3(1, 3, half * 2 + 1), "Wall", true);
            Primitive(PrimitiveType.Cube, "Wall_W", arena.transform, new Vector3(-half, 1.5f, 0), new Vector3(1, 3, half * 2 + 1), "Wall", true);

            Vector3[] crates =
            {
                new(-10, 0, 8), new(11, 0, 9), new(-12, 0, -10), new(9, 0, -12),
                new(0, 0, 16), new(0, 0, -17), new(-19, 0, 0), new(19, 0, 1),
            };
            for (int i = 0; i < crates.Length; i++)
            {
                bool pillar = i % 2 == 1;
                Vector3 size = pillar ? new Vector3(2f, 3f, 2f) : new Vector3(4f, 1.5f, 2f);
                var crate = Primitive(pillar ? PrimitiveType.Cylinder : PrimitiveType.Cube, $"Obstacle_{i}", arena.transform,
                    crates[i] + Vector3.up * (pillar ? 1.5f : 0.75f), pillar ? new Vector3(2f, 1.5f, 2f) : size, "Obstacle", true);
                crate.transform.rotation = Quaternion.Euler(0, i * 23f, 0);
            }
            SetLayerRecursive(arena, env);
            foreach (Transform child in arena.transform) GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic);

            var surface = arena.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = 1 << env;

            // Spawns
            var playerSpawn = new GameObject("PlayerSpawn").transform;
            var spawnRoot = new GameObject("EnemySpawnPoints").transform;
            var spawnPoints = new List<Object>();
            Vector3[] spawnPositions =
            {
                new(-26, 0, 26), new(0, 0, 26), new(26, 0, 26), new(26, 0, 0),
                new(26, 0, -26), new(0, 0, -26), new(-26, 0, -26), new(-26, 0, 0),
            };
            for (int i = 0; i < spawnPositions.Length; i++)
            {
                var point = new GameObject($"Spawn_{i}").transform;
                point.SetParent(spawnRoot, false);
                point.position = spawnPositions[i];
                spawnPoints.Add(point);
            }

            // Camera
            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.GetComponent<Camera>();
            camera.fieldOfView = 50f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.11f, 0.14f);
            cameraGo.transform.position = new Vector3(0, 16, -11);
            cameraGo.transform.rotation = Quaternion.LookRotation(new Vector3(0, -16, 11));
            var cameraRig = cameraGo.AddComponent<TopDownCamera>();

            // Systems
            var systems = new GameObject("Systems");
            var spawner = new GameObject("EnemySpawner").AddComponent<EnemySpawner>();
            spawner.transform.SetParent(systems.transform);
            SetArray(spawner, "spawnPoints", spawnPoints.ToArray());
            var director = new GameObject("WaveDirector").AddComponent<WaveDirector>();
            director.transform.SetParent(systems.transform);
            Set(director, "settings", waves);
            var tracers = new GameObject("TracerPool").AddComponent<TracerPool>();
            tracers.transform.SetParent(systems.transform);
            Set(tracers, "tracerPrefab", tracerPrefab);

            // UI
            var canvas = CreateCanvas("Canvas");
            CreateEventSystem();
            var hud = BuildHud(canvas.transform);
            var shop = BuildShop(canvas.transform, shopItemPrefab);
            var pause = BuildPause(canvas.transform);
            var gameOver = BuildGameOver(canvas.transform);

            // Composition root
            var installer = new GameObject("GameInstaller").AddComponent<GameInstaller>();
            Set(installer, "weaponCatalog", catalog);
            Set(installer, "progressionSettings", progression);
            Set(installer, "playerPrefab", playerPrefab);
            Set(installer, "playerSpawn", playerSpawn);
            Set(installer, "mainCamera", camera);
            Set(installer, "cameraRig", cameraRig);
            Set(installer, "navMeshSurface", surface);
            Set(installer, "enemySpawner", spawner);
            Set(installer, "waveDirector", director);
            Set(installer, "tracerPool", tracers);
            Set(installer, "hudView", hud);
            Set(installer, "shopView", shop);
            Set(installer, "gameOverView", gameOver);
            Set(installer, "pauseView", pause);

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/{SceneNames.Game}.unity");
        }

        // ------------------------------------------------------------------ game UI

        private static HudView BuildHud(Transform canvas)
        {
            var root = Rect("HUD", canvas);
            Stretch(root);
            var topLeft = new Vector2(0, 1);
            var topRight = new Vector2(1, 1);
            var top = new Vector2(0.5f, 1);
            var bottom = new Vector2(0.5f, 0);

            FilledBar("HealthBar", root, topLeft, new Vector2(30, -30), new Vector2(420, 34), new Color(0, 0, 0, 0.6f), new Color(0.85f, 0.2f, 0.25f), out var healthFill);
            var healthText = Text("HealthText", root, "100 / 100", 22);
            Place(healthText.rectTransform, topLeft, new Vector2(30, -30), new Vector2(420, 34));
            FilledBar("XpBar", root, topLeft, new Vector2(30, -72), new Vector2(420, 16), new Color(0, 0, 0, 0.6f), new Color(0.3f, 0.6f, 1f), out var xpFill);
            var levelText = Text("LevelText", root, "LV 1", 26, TextAlignmentOptions.Left);
            Place(levelText.rectTransform, topLeft, new Vector2(30, -94), new Vector2(500, 36));

            var waveText = Text("WaveText", root, "GET READY", 54, TextAlignmentOptions.Center, ButtonColor);
            waveText.fontStyle = FontStyles.Bold;
            Place(waveText.rectTransform, top, new Vector2(0, -20), new Vector2(700, 70));
            var enemiesText = Text("EnemiesText", root, "Enemies: 0", 26);
            Place(enemiesText.rectTransform, top, new Vector2(0, -92), new Vector2(600, 36));
            var timerText = Text("TimerText", root, "Next wave in 5", 30, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.5f));
            Place(timerText.rectTransform, top, new Vector2(0, -130), new Vector2(600, 40));

            var goldText = Text("GoldText", root, "0 G", 34, TextAlignmentOptions.Right, new Color(1f, 0.85f, 0.3f));
            Place(goldText.rectTransform, topRight, new Vector2(-30, -26), new Vector2(400, 44));
            var scoreText = Text("ScoreText", root, "SCORE 0", 30, TextAlignmentOptions.Right);
            Place(scoreText.rectTransform, topRight, new Vector2(-30, -72), new Vector2(400, 40));

            var weaponText = Text("WeaponText", root, "Pistol", 36, TextAlignmentOptions.Center);
            weaponText.fontStyle = FontStyles.Bold;
            Place(weaponText.rectTransform, bottom, new Vector2(0, 70), new Vector2(700, 50));
            var hintText = Text("HintText", root, "", 22, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.7f));
            Place(hintText.rectTransform, bottom, new Vector2(0, 24), new Vector2(1600, 36));

            var toastText = Text("ToastText", root, "", 64, TextAlignmentOptions.Center, new Color(1f, 0.95f, 0.6f));
            toastText.fontStyle = FontStyles.Bold;
            Place(toastText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(1400, 90));
            toastText.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<HudView>();
            Set(view, "healthFill", healthFill);
            Set(view, "healthText", healthText);
            Set(view, "xpFill", xpFill);
            Set(view, "levelText", levelText);
            Set(view, "goldText", goldText);
            Set(view, "scoreText", scoreText);
            Set(view, "waveText", waveText);
            Set(view, "enemiesText", enemiesText);
            Set(view, "timerText", timerText);
            Set(view, "weaponText", weaponText);
            Set(view, "hintText", hintText);
            Set(view, "toastText", toastText);
            return view;
        }

        private static ShopView BuildShop(Transform canvas, ShopItemView itemPrefab)
        {
            var holder = Rect("Shop", canvas);
            Stretch(holder);
            var panel = Image("Panel", holder, PanelColor, raycast: true);
            Place(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 720));

            var title = Text("Title", panel.transform, "SHOP", 56, TextAlignmentOptions.Center, ButtonColor);
            title.fontStyle = FontStyles.Bold;
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(800, 70));
            var gold = Text("Gold", panel.transform, "Gold: 0", 30, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
            Place(gold.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(800, 40));

            var list = Rect("Items", panel.transform);
            Place(list, new Vector2(0.5f, 1f), new Vector2(0, -140), new Vector2(820, 360));
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var medkit = Button("Medkit", panel.transform, "Medkit", out var medkitLabel, new Color(0.4f, 0.85f, 0.45f), 26);
            Place(medkit.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0, 170), new Vector2(820, 60));

            var startWave = Button("StartWave", panel.transform, "START NEXT WAVE", out _, null, 26);
            Place(startWave.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(-210, 90), new Vector2(400, 64));
            var close = Button("Close", panel.transform, "CLOSE (B)", out _, new Color(0.6f, 0.62f, 0.68f), 26);
            Place(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(210, 90), new Vector2(400, 64));

            var message = Text("Message", panel.transform, "", 24, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.6f));
            Place(message.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(820, 40));

            var view = holder.gameObject.AddComponent<ShopView>();
            Set(view, "root", panel.gameObject);
            Set(view, "goldText", gold);
            Set(view, "messageText", message);
            Set(view, "itemContainer", list);
            Set(view, "itemPrefab", itemPrefab);
            Set(view, "medkitButton", medkit);
            Set(view, "medkitLabel", medkitLabel);
            Set(view, "startWaveButton", startWave);
            Set(view, "closeButton", close);
            return view;
        }

        private static PauseView BuildPause(Transform canvas)
        {
            var holder = Rect("Pause", canvas);
            Stretch(holder);
            var dim = Image("Overlay", holder, new Color(0, 0, 0, 0.7f), raycast: true);
            Stretch(dim.rectTransform);

            var title = Text("Title", dim.transform, "PAUSED", 90, TextAlignmentOptions.Center, ButtonColor);
            title.fontStyle = FontStyles.Bold;
            Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(900, 120));
            var resume = Button("Resume", dim.transform, "RESUME", out _);
            Place(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(420, 80));
            var menu = Button("MainMenu", dim.transform, "MAIN MENU", out _, new Color(0.6f, 0.62f, 0.68f));
            Place(menu.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(420, 80));

            var view = holder.gameObject.AddComponent<PauseView>();
            Set(view, "root", dim.gameObject);
            Set(view, "resumeButton", resume);
            Set(view, "menuButton", menu);
            return view;
        }

        private static GameOverView BuildGameOver(Transform canvas)
        {
            var holder = Rect("GameOver", canvas);
            Stretch(holder);
            var dim = Image("Overlay", holder, new Color(0.15f, 0f, 0f, 0.8f), raycast: true);
            Stretch(dim.rectTransform);

            var title = Text("Title", dim.transform, "GAME OVER", 110, TextAlignmentOptions.Center, new Color(1f, 0.3f, 0.3f));
            title.fontStyle = FontStyles.Bold;
            Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1200, 140));
            var summary = Text("Summary", dim.transform, "", 38);
            Place(summary.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(1000, 160));
            var high = Text("HighScore", dim.transform, "", 40, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
            Place(high.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1000, 60));

            var retry = Button("Retry", dim.transform, "PLAY AGAIN", out _);
            Place(retry.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, -130), new Vector2(420, 80));
            var menu = Button("MainMenu", dim.transform, "MAIN MENU", out _, new Color(0.6f, 0.62f, 0.68f));
            Place(menu.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, -230), new Vector2(420, 80));

            var view = holder.gameObject.AddComponent<GameOverView>();
            Set(view, "root", dim.gameObject);
            Set(view, "summaryText", summary);
            Set(view, "highScoreText", high);
            Set(view, "retryButton", retry);
            Set(view, "menuButton", menu);
            return view;
        }
    }
}
