#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Gameplay;
using SomnusNet.UI;
using SomnusNet.Units;
using SomnusNet.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.Editor
{
    public static class LevelOneSetup
    {
        const string Root = "Assets/SomnusNet";
        const string DataPath = Root + "/Data/Level01";
        const string PrefabPath = Root + "/Prefabs";
        const string AllFeaturesScenePath = Root + "/Scenes/AllFeaturesLevel.unity";
        const string StoryModeScenePath = Root + "/Scenes/StoryMode1.unity";
        const string StoryMode2ScenePath = Root + "/Scenes/StoryMode2.unity";
        const string MainMenuScenePath = Root + "/Scenes/MainMenu.unity";
        const string WorldMapScenePath = Root + "/Scenes/WorldMap.unity";

        [MenuItem("Somnus Net/Build Shared Assets")]
        public static void BuildSharedAssetsFromMenu() => CreateSharedAssets();

        [MenuItem("Somnus Net/Build All Features Level")]
        public static void BuildAllFeaturesFromMenu() => CreateAllFeaturesLevel();

        [MenuItem("Somnus Net/Build Story Mode 1")]
        public static void BuildStoryModeFromMenu() => CreateStoryMode1();

        [MenuItem("Somnus Net/Build Story Mode 2")]
        public static void BuildStoryMode2FromMenu() => CreateStoryMode2();

        [MenuItem("Somnus Net/Build Main Menu")]
        public static void BuildMainMenuFromMenu() => CreateMainMenu();

        [MenuItem("Somnus Net/Build World Map")]
        public static void BuildWorldMapFromMenu() => CreateWorldMap();

        public static void CreateSharedAssets()
        {
            EnsureFolders();
            var sprites = CreateSprites();
            CreateBlobDefinitions(sprites);
            CreateGlitchDefinitions(sprites);
            CreateWaveSet();
            CreateBlobPrefab();
            CreateGlitchPrefab();
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Somnus Net shared assets built under " + DataPath);
        }

        public static void CreateAllFeaturesLevel()
        {
            EnsureFolders();

            var waves = AssetDatabase.LoadAssetAtPath<LevelWaveSet>(DataPath + "/Level01_Waves.asset");
            var blobPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/DreamBlob.prefab");
            var glitchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/Glitch.prefab");
            var blobDefs = LoadBlobDefinitions();
            var glitchDefs = LoadGlitchDefinitions();

            if (waves == null || blobPrefab == null || glitchPrefab == null || blobDefs.Count == 0 || glitchDefs.Count == 0)
            {
                Debug.LogError("Run Somnus Net → Build Shared Assets first to create shared assets.");
                return;
            }

            CreateScene(new LevelBuildOptions
            {
                scenePath = AllFeaturesScenePath,
                levelTitle = "All Features Level",
                levelCompleteBanner = string.Empty,
                infiniteRounds = true,
                storyMode = false,
                upgradesFromStart = true,
                enableHarmonyTypes = true,
                enableDevSkip = true,
                totalRounds = int.MaxValue
            }, blobDefs, glitchDefs, waves, blobPrefab, glitchPrefab);
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("All Features Level built. Open scene: " + AllFeaturesScenePath);
        }

        public static void CreateStoryMode1()
        {
            EnsureFolders();

            var waves = AssetDatabase.LoadAssetAtPath<LevelWaveSet>(DataPath + "/Level01_Waves.asset");
            var blobPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/DreamBlob.prefab");
            var glitchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/Glitch.prefab");
            var blobDefs = LoadBlobDefinitions();
            var glitchDefs = LoadGlitchDefinitions();

            if (waves == null || blobPrefab == null || glitchPrefab == null || blobDefs.Count == 0 || glitchDefs.Count == 0)
            {
                Debug.LogError("Run Somnus Net → Build Shared Assets first to create shared assets.");
                return;
            }

            CreateScene(new LevelBuildOptions
            {
                scenePath = StoryModeScenePath,
                levelTitle = "Story Mode 1",
                levelCompleteBanner = "Story Mode 1 Complete",
                infiniteRounds = false,
                storyMode = true,
                storyModeVariant = 1,
                enableStoryDialogs = true,
                upgradesFromStart = true,
                enableHarmonyTypes = true,
                totalRounds = StoryModeRules.TotalRounds
            }, blobDefs, glitchDefs, waves, blobPrefab, glitchPrefab);
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Story Mode 1 built. Open scene: " + StoryModeScenePath);
        }

        public static void CreateStoryMode2()
        {
            EnsureFolders();

            var waves = AssetDatabase.LoadAssetAtPath<LevelWaveSet>(DataPath + "/Level01_Waves.asset");
            var blobPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/DreamBlob.prefab");
            var glitchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/Glitch.prefab");
            var blobDefs = LoadBlobDefinitions();
            var glitchDefs = LoadGlitchDefinitions();

            if (waves == null || blobPrefab == null || glitchPrefab == null || blobDefs.Count == 0 || glitchDefs.Count == 0)
            {
                Debug.LogError("Run Somnus Net → Build Shared Assets first to create shared assets.");
                return;
            }

            CreateScene(new LevelBuildOptions
            {
                scenePath = StoryMode2ScenePath,
                levelTitle = "Story Mode 2",
                levelCompleteBanner = "Story Mode 2 Complete",
                infiniteRounds = false,
                storyMode = true,
                storyModeVariant = 2,
                enableStoryDialogs = false,
                upgradesFromStart = true,
                enableHarmonyTypes = true,
                enableDevSkip = false,
                totalRounds = StoryModeRules.StoryMode2TotalRounds
            }, blobDefs, glitchDefs, waves, blobPrefab, glitchPrefab);
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Story Mode 2 built. Open scene: " + StoryMode2ScenePath);
        }

        public static void CreateMainMenu()
        {
            EnsureFolders();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.07f, 0.14f);
            cam.orthographic = true;
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();

            var menu = new GameObject("MainMenu");
            menu.AddComponent<MainMenu>();

            if (!Directory.Exists(Path.GetDirectoryName(MainMenuScenePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(MainMenuScenePath)!);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Main Menu built. Open scene: " + MainMenuScenePath);
        }

        public static void CreateWorldMap()
        {
            EnsureFolders();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.white;
            cam.orthographic = true;
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();

            var map = new GameObject("WorldMap");
            map.AddComponent<WorldMapController>();

            if (!Directory.Exists(Path.GetDirectoryName(WorldMapScenePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(WorldMapScenePath)!);
            EditorSceneManager.SaveScene(scene, WorldMapScenePath);
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("World Map built. Open scene: " + WorldMapScenePath);
        }

        static List<BlobDefinition> LoadBlobDefinitions()
        {
            var list = new List<BlobDefinition>();
            foreach (BlobKind kind in Enum.GetValues(typeof(BlobKind)))
            {
                var path = $"{DataPath}/Blob_{kind}.asset";
                var def = AssetDatabase.LoadAssetAtPath<BlobDefinition>(path);
                if (def != null) list.Add(def);
            }
            return list;
        }

        static List<GlitchDefinition> LoadGlitchDefinitions()
        {
            var list = new List<GlitchDefinition>();
            foreach (GlitchKind kind in Enum.GetValues(typeof(GlitchKind)))
            {
                var path = $"{DataPath}/Glitch_{kind}.asset";
                var def = AssetDatabase.LoadAssetAtPath<GlitchDefinition>(path);
                if (def != null) list.Add(def);
            }
            return list;
        }

        static void SetBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var path in new[] { MainMenuScenePath, StoryModeScenePath, StoryMode2ScenePath, WorldMapScenePath, AllFeaturesScenePath })
            {
                if (File.Exists(path))
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            if (scenes.Count > 0)
                EditorBuildSettings.scenes = scenes.ToArray();
        }

        struct LevelBuildOptions
        {
            public string scenePath;
            public string levelTitle;
            public string levelCompleteBanner;
            public bool infiniteRounds;
            public bool storyMode;
            public int storyModeVariant;
            public bool enableStoryDialogs;
            public bool upgradesFromStart;
            public bool enableHarmonyTypes;
            public bool enableDevSkip;
            public int totalRounds;
        }

        static void EnsureFolders()
        {
            foreach (var p in new[] { DataPath, PrefabPath, Root + "/Scenes", Root + "/Sprites" })
            {
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            }
        }

        static Dictionary<string, Sprite> CreateSprites()
        {
            var map = new Dictionary<string, Sprite>();
            const float ppu = SpriteFactory.DefaultPpu;
            var pivot = new Vector2(0.5f, 0.4f);

            void SaveTex(string key, Texture2D tex)
            {
                var path = $"{Root}/Sprites/{key}.asset";
                AssetDatabase.CreateAsset(tex, path);
                var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu);
                sprite.name = key;
                AssetDatabase.AddObjectToAsset(sprite, path);
                map[key] = sprite;
            }

            SaveTex("gatekeeper_blob", SpriteFactory.SoftBlobTexture(Color.white, new Color(0.88f, 0.88f, 0.92f)));
            SaveTex("creator_blob", SpriteFactory.SoftBlobTexture(
                new Color(0.12f, 0.42f, 0.18f), new Color(0.08f, 0.28f, 0.12f)));
            SaveTex("fourohfour_blob", SpriteFactory.SoftBlobTexture(
                new Color(0.62f, 0.84f, 0.98f), new Color(0.38f, 0.62f, 0.88f)));
            SaveTex("blaze_blob", SpriteFactory.SoftBlobTexture(
                new Color(0.98f, 0.42f, 0.28f), new Color(0.78f, 0.18f, 0.12f)));
            SaveTex("dealer_blob", SpriteFactory.SoftBlobTexture(
                new Color(0.72f, 0.55f, 0.98f), new Color(0.42f, 0.28f, 0.72f)));
            SaveTex("cheerful_blob", SpriteFactory.SoftBlobTexture(
                new Color(0.48f, 0.92f, 0.52f), new Color(0.18f, 0.58f, 0.28f)));
            SaveTex("archivist_blob", SpriteFactory.SoftBlobTexture(
                new Color(0.88f, 0.78f, 0.52f), new Color(0.52f, 0.42f, 0.28f)));
            SaveTex("countdown_blob", SpriteFactory.SoftBlobTexture(
                new Color(0.98f, 0.52f, 0.22f), new Color(0.72f, 0.18f, 0.12f)));
            SaveTex("lantern_blob", SpriteFactory.SoftBlobTexture(
                new Color(0.62f, 0.78f, 0.98f), new Color(0.28f, 0.42f, 0.72f)));
            SaveTex("glitch_mite", SpriteFactory.GlitchTexture(new Color(0.92f, 0.78f, 1f), new Color(0.68f, 0.48f, 0.98f)));
            SaveTex("lag_beetle", SpriteFactory.GlitchTexture(new Color(0.88f, 0.72f, 1f), new Color(0.62f, 0.42f, 0.95f), angular: true));
            SaveTex("shell_glitch", SpriteFactory.GlitchTexture(new Color(0.95f, 0.8f, 1f), new Color(0.7f, 0.5f, 0.98f)));
            SaveTex("packet_swarm", SpriteFactory.GlitchTexture(new Color(0.9f, 0.75f, 1f), new Color(0.65f, 0.45f, 0.96f), angular: true));
            SaveTex("glitch_overload", SpriteFactory.GlitchTexture(
                new Color(1f, 0.58f, 0.82f), new Color(0.78f, 0.24f, 0.62f), size: 96));
            SaveTex("glitch_faze", SpriteFactory.GlitchTexture(
                new Color(0.78f, 0.96f, 1f), new Color(0.4f, 0.7f, 0.98f), size: 96));
            SaveTex("glitch_sink", SpriteFactory.GlitchTexture(
                new Color(0.84f, 0.68f, 1f), new Color(0.5f, 0.3f, 0.92f), size: 96, angular: true));
            SaveTex("glitch_cicadian_rhythm", SpriteFactory.GlitchTexture(
                new Color(0.72f, 0.66f, 0.88f), new Color(0.22f, 0.16f, 0.32f), size: 96));
            SaveTex("shell_overlay", SpriteFactory.ShellOverlayTexture());
            return map;
        }

        static List<BlobDefinition> CreateBlobDefinitions(Dictionary<string, Sprite> s)
        {
            BlobDefinition Make(BlobKind kind, string name, string verb, int cost, Sprite sprite, int hp = 0)
            {
                var def = ScriptableObject.CreateInstance<BlobDefinition>();
                def.kind = kind;
                def.displayName = name;
                def.verbDescription = verb;
                def.ponderCost = cost;
                def.sprite = sprite;
                def.maxHealth = hp;
                var path = $"{DataPath}/Blob_{kind}.asset";
                AssetDatabase.CreateAsset(def, path);
                return def;
            }

            var list = new List<BlobDefinition>
            {
                Make(BlobKind.Gatekeeper, "Gatekeeper", "Shoot — steady thought-pulses", 50, s["gatekeeper_blob"]),
                Make(BlobKind.Creator, "Creator", "Focus — damage ramps while attacking the same glitch", 80, s["creator_blob"]),
                Make(BlobKind.FourOhFour, "404", "Absent — long-range pressure with slow volleys", 80, s["fourohfour_blob"]),
                Make(BlobKind.Blaze, "Blaze", "Burn — fast close-range pressure", 80, s["blaze_blob"]),
                Make(BlobKind.Dealer, "Dealer", "Split — shards scatter on impact", 100, s["dealer_blob"]),
                Make(BlobKind.Cheerful, "Cheerful", "Cheer — buffs a nearby Best Friend", 90, s["cheerful_blob"]),
                Make(BlobKind.Archivist, "Archivist", "Pause — long-range shots that can freeze glitches", 95,
                    s["archivist_blob"]),
                Make(BlobKind.Countdown, "Countdown", "Mine — lays landmines on the road", 88, s["countdown_blob"]),
                Make(BlobKind.Lantern, "Lantern", "Pulse — grants Looped Sight to nearby blobs", 92, s["lantern_blob"])
            };

            list[0].typeLabel = "Harmony";
            list[0].typeLabels = new[] { "Harmony" };
            list[0].projectileDamage = 20;
            list[0].fireInterval = 1f;
            list[0].projectileSpeed = 9f;
            list[0].shotsPerVolley = 1;

            list[1].displayName = "Creator";
            list[1].verbDescription = "Focus — damage ramps while attacking the same glitch";
            list[1].ponderCost = 80;
            list[1].typeLabel = "Speed";
            list[1].typeLabels = new[] { "Speed", "Dream Team" };
            list[1].projectileDamage = 20;
            list[1].fireInterval = 1f;
            list[1].projectileSpeed = 9f;
            list[1].shotsPerVolley = 1;
            list[1].slowDuration = 0f;
            list[1].slowMultiplier = 1f;

            list[2].displayName = "404";
            list[2].verbDescription = "Absent — long-range pressure with slow volleys";
            list[2].ponderCost = 80;
            list[2].typeLabel = "Dream Team";
            list[2].typeLabels = new[] { "Dream Team", "Absent History" };
            list[2].projectileDamage = 18;
            list[2].fireInterval = 2.1f;
            list[2].projectileSpeed = 8f;
            list[2].shotsPerVolley = 1;
            list[2].rangeSize = 7;

            list[3].displayName = "Blaze";
            list[3].verbDescription = "Burn — fast close-range pressure";
            list[3].ponderCost = 80;
            list[3].typeLabel = "Dream Team";
            list[3].typeLabels = new[] { "Dream Team", "Broken Rings" };
            list[3].projectileDamage = 17;
            list[3].fireInterval = 0.58f;
            list[3].projectileSpeed = 9f;
            list[3].shotsPerVolley = 1;
            list[3].rangeSize = 3;

            list[4].displayName = "Dealer";
            list[4].verbDescription = "Split — shards scatter on impact";
            list[4].ponderCost = 100;
            list[4].typeLabel = "Broken Rings";
            list[4].typeLabels = new[] { "Broken Rings", "Slimy Support" };
            list[4].projectileDamage = 20;
            list[4].fireInterval = 1f;
            list[4].projectileSpeed = 9f;
            list[4].shotsPerVolley = 1;
            list[4].rangeSize = 5;

            list[5].displayName = "Cheerful";
            list[5].verbDescription = "Cheer — buffs a nearby Best Friend";
            list[5].ponderCost = 90;
            list[5].typeLabel = "Slimy Support";
            list[5].typeLabels = new[] { "Slimy Support", "Entertainer" };
            list[5].projectileDamage = 0;
            list[5].fireInterval = 2f;
            list[5].projectileSpeed = 8f;
            list[5].shotsPerVolley = 1;
            list[5].rangeSize = 1;

            list[6].displayName = "Archivist";
            list[6].verbDescription = "Pause — long-range shots that can freeze glitches";
            list[6].ponderCost = 95;
            list[6].typeLabel = "Broken Rings";
            list[6].typeLabels = new[] { "Broken Rings", "Absent History" };
            list[6].projectileDamage = 19;
            list[6].fireInterval = 1.22f;
            list[6].projectileSpeed = 8.5f;
            list[6].shotsPerVolley = 1;
            list[6].rangeSize = 9;

            list[7].displayName = "Countdown";
            list[7].verbDescription = "Mine — lays landmines on the road";
            list[7].ponderCost = 88;
            list[7].typeLabel = "Bench Trio";
            list[7].typeLabels = new[] { "Bench Trio", "Burdened Crown" };
            list[7].projectileDamage = 18;
            list[7].fireInterval = 1.15f;
            list[7].projectileSpeed = 8.5f;
            list[7].shotsPerVolley = 1;
            list[7].rangeSize = 3;

            list[8].displayName = "Lantern";
            list[8].verbDescription = "Pulse — grants Looped Sight to nearby blobs";
            list[8].ponderCost = 92;
            list[8].typeLabel = "Bench Trio";
            list[8].typeLabels = new[] { "Bench Trio", "Absent History" };
            list[8].projectileDamage = 0;
            list[8].fireInterval = 2.2f;
            list[8].projectileSpeed = 8f;
            list[8].shotsPerVolley = 1;
            list[8].rangeSize = 3;

            EditorUtility.SetDirty(list[0]);
            EditorUtility.SetDirty(list[1]);
            EditorUtility.SetDirty(list[2]);
            EditorUtility.SetDirty(list[3]);
            EditorUtility.SetDirty(list[4]);
            EditorUtility.SetDirty(list[5]);
            EditorUtility.SetDirty(list[6]);
            EditorUtility.SetDirty(list[7]);
            EditorUtility.SetDirty(list[8]);
            return list;
        }

        static List<GlitchDefinition> CreateGlitchDefinitions(Dictionary<string, Sprite> s)
        {
            GlitchDefinition Make(GlitchKind kind, string name, string fantasy, Sprite sprite, int hp, float speed,
                int shell = 0, float visualScale = 1f)
            {
                var def = ScriptableObject.CreateInstance<GlitchDefinition>();
                def.kind = kind;
                def.displayName = name;
                def.fantasyNote = fantasy;
                def.sprite = sprite;
                def.maxHealth = hp;
                def.moveSpeed = speed;
                def.shellHealth = shell;
                def.ponderReward = PonderEconomyRules.KillReward;
                def.visualScaleMultiplier = visualScale;
                var path = $"{DataPath}/Glitch_{kind}.asset";
                AssetDatabase.CreateAsset(def, path);
                return def;
            }

            const float miniBossScale = 1.22f;
            const float roundBossScale = CicadianRhythmRules.DefaultVisualScaleMultiplier;

            var list = new List<GlitchDefinition>
            {
                Make(GlitchKind.GlitchMite, "Glitch Mite", "A tiny corruption speck — slow but steady", s["glitch_mite"], 90, 0.32f),
                Make(GlitchKind.LagBeetle, "Lag Beetle", "Stutters forward in bursts — reaches lanes fast", s["lag_beetle"], 70, 0.52f),
                Make(GlitchKind.ShellGlitch, "Shell Glitch", "Wears corrupted packet armor — soak damage", s["shell_glitch"], 80, 0.28f, shell: 140),
                Make(GlitchKind.PacketSwarm, "Packet Swarm", "Fragile data-motes that rush in numbers", s["packet_swarm"], 45, 0.48f),
                Make(GlitchKind.Overload, "Overload", "Mini boss — immense corrupted bulk", s["glitch_overload"], 360, 0.28f, visualScale: miniBossScale),
                Make(GlitchKind.Faze, "Faze", "Mini boss — phases through some attacks", s["glitch_faze"], 115, 0.36f, visualScale: miniBossScale),
                Make(GlitchKind.Sink, "Sink", "Mini boss — surges at double speed", s["glitch_sink"], 85, 0.7f, visualScale: miniBossScale),
                Make(GlitchKind.CicadianRhythm, "Cicadian Rhythm", "Round boss — looped rhythm that warps damage and surges forward",
                    s["glitch_cicadian_rhythm"], 880, 0.3f, visualScale: roundBossScale)
            };
            list[2].shellOverlaySprite = s["shell_overlay"];
            EditorUtility.SetDirty(list[2]);
            return list;
        }

        static LevelWaveSet CreateWaveSet()
        {
            var waves = ScriptableObject.CreateInstance<LevelWaveSet>();
            waves.levelTitle = "Level 01 — First Breach";
            waves.briefing =
                "The Bug Swarm infects the Somnus Net. +1 Ponder/s, +5 per defeat. Defeat 10 glitches to win. The 10th spawn each round is a random mini boss.";
            waves.delayBetweenWaves = 14f;
            waves.spawns = new List<WaveSpawnEntry>
            {
                Entry(GlitchKind.GlitchMite, 2, 8f),
                Entry(GlitchKind.GlitchMite, 1, 22f),
                Entry(GlitchKind.LagBeetle, 3, 38f),
                Entry(GlitchKind.GlitchMite, 0, 52f),
                Entry(GlitchKind.GlitchMite, 4, 54f),
                Entry(GlitchKind.ShellGlitch, 2, 68f),
                Entry(GlitchKind.PacketSwarm, 1, 82f),
                Entry(GlitchKind.PacketSwarm, 3, 84f),
                Entry(GlitchKind.LagBeetle, 0, 96f),
                Entry(GlitchKind.GlitchMite, 2, 108f)
            };
            AssetDatabase.CreateAsset(waves, DataPath + "/Level01_Waves.asset");
            return waves;
        }

        static WaveSpawnEntry Entry(GlitchKind glitch, int lane, float delay) =>
            new() { glitch = glitch, lane = lane, delayFromWaveStart = delay };

        static GameObject CreateBlobPrefab()
        {
            var root = new GameObject("DreamBlob");
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            var glow = new GameObject("Glow");
            glow.transform.SetParent(root.transform);
            glow.transform.localScale = Vector3.one * 0.9f;
            var glowSr = glow.AddComponent<SpriteRenderer>();
            glowSr.sortingOrder = 1;
            glowSr.color = new Color(1f, 1f, 1f, 0.35f);

            var blob = root.AddComponent<DreamBlob>();
            var so = new SerializedObject(blob);
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("glowChild").objectReferenceValue = glow.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            var path = PrefabPath + "/DreamBlob.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject CreateGlitchPrefab()
        {
            var root = new GameObject("Glitch");

            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(root.transform);
            var glowSr = glowGo.AddComponent<SpriteRenderer>();
            glowSr.sortingOrder = 9;

            var body = root.AddComponent<SpriteRenderer>();
            body.sortingOrder = 10;

            var shellGo = new GameObject("Shell");
            shellGo.transform.SetParent(root.transform);
            var shellSr = shellGo.AddComponent<SpriteRenderer>();
            shellSr.sortingOrder = 11;

            var col = root.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            col.isTrigger = true;

            var glitch = root.AddComponent<Glitch>();
            var so = new SerializedObject(glitch);
            so.FindProperty("bodyRenderer").objectReferenceValue = body;
            so.FindProperty("shellRenderer").objectReferenceValue = shellSr;
            so.FindProperty("glowRenderer").objectReferenceValue = glowSr;
            so.ApplyModifiedPropertiesWithoutUndo();

            var path = PrefabPath + "/Glitch.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static void CreateScene(
            LevelBuildOptions options,
            List<BlobDefinition> blobs,
            List<GlitchDefinition> glitches,
            LevelWaveSet waves,
            GameObject blobPrefab,
            GameObject glitchPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = GridManager.CameraOrthoSize;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.12f);
            cam.transform.position = new Vector3(0f, GridManager.CameraYOffset, -10f);
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();

            var bg = new GameObject("SomnusBackdrop");
            var bgSr = bg.AddComponent<SpriteRenderer>();
            SomnusBackground.ApplyTo(bgSr, 30f, 17f);

            var systems = new GameObject("Systems");
            var grid = systems.AddComponent<GridManager>();
            grid.ApplyBoardLayout();

            var core = new GameObject("DreamCore");
            core.transform.position = grid.CoreWorldPosition;
            var coreSr = core.AddComponent<SpriteRenderer>();
            var coreTex = SpriteFactory.SoftBlobTexture(new Color(0.6f, 0.85f, 1f), new Color(0.2f, 0.35f, 0.9f));
            coreSr.sprite = Sprite.Create(coreTex, new Rect(0, 0, coreTex.width, coreTex.height),
                new Vector2(0.5f, 0.4f), SpriteFactory.DefaultPpu);
            coreSr.sortingOrder = 6;
            core.transform.localScale = Vector3.one * grid.BlobVisualScale * 1.15f;
            var economy = systems.AddComponent<PonderEconomy>();
            var economySo = new SerializedObject(economy);
            economySo.FindProperty("startingPonders").intValue =
                options.storyMode ? StoryModeRules.StartingPonders : PonderEconomyRules.StandardStartingPonders;
            economySo.ApplyModifiedPropertiesWithoutUndo();

            var levelSettings = systems.AddComponent<LevelSettings>();
            var settingsSo = new SerializedObject(levelSettings);
            settingsSo.FindProperty("levelTitle").stringValue = options.levelTitle;
            settingsSo.FindProperty("infiniteRounds").boolValue = options.infiniteRounds;
            settingsSo.FindProperty("storyMode").boolValue = options.storyMode;
            settingsSo.FindProperty("storyModeVariant").intValue = options.storyModeVariant;
            settingsSo.FindProperty("enableStoryDialogs").boolValue = options.enableStoryDialogs;
            settingsSo.FindProperty("upgradesUnlockedFromStart").boolValue = options.upgradesFromStart;
            settingsSo.FindProperty("enableHarmonyTypes").boolValue = options.enableHarmonyTypes;
            settingsSo.FindProperty("enableDevSkip").boolValue = options.enableDevSkip;
            settingsSo.FindProperty("totalRounds").intValue = options.totalRounds;
            settingsSo.FindProperty("levelCompleteBanner").stringValue = options.levelCompleteBanner;
            settingsSo.ApplyModifiedPropertiesWithoutUndo();

            systems.AddComponent<GameManager>();
            var catalog = systems.AddComponent<GameCatalog>();
            catalog.blobs = blobs;
            catalog.glitches = glitches;
            catalog.level01Waves = waves;

            var placement = systems.AddComponent<PlacementController>();
            var wavesCtrl = systems.AddComponent<WaveController>();
            systems.AddComponent<GridVisualizer>();
            if (options.enableHarmonyTypes)
                grid.gameObject.AddComponent<HarmonyBandVisualizer>();
            grid.gameObject.AddComponent<DealerSecondVisualizer>();
            grid.gameObject.AddComponent<CheerfulHeartVisualizer>();
            var board = systems.AddComponent<GridBoardRenderer>();
            var boardSo = new SerializedObject(board);
            boardSo.FindProperty("grid").objectReferenceValue = grid;
            boardSo.ApplyModifiedPropertiesWithoutUndo();

            var placementSo = new SerializedObject(placement);
            placementSo.FindProperty("catalog").objectReferenceValue = catalog;
            placementSo.FindProperty("blobPrefab").objectReferenceValue = blobPrefab;
            placementSo.ApplyModifiedPropertiesWithoutUndo();

            var waveSo = new SerializedObject(wavesCtrl);
            waveSo.FindProperty("catalog").objectReferenceValue = catalog;
            waveSo.FindProperty("glitchPrefab").objectReferenceValue = glitchPrefab;
            waveSo.ApplyModifiedPropertiesWithoutUndo();

            CreateUI(catalog, placement, options);

            if (!Directory.Exists(Path.GetDirectoryName(options.scenePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(options.scenePath)!);
            EditorSceneManager.SaveScene(scene, options.scenePath);
        }

        static void CreateUI(GameCatalog catalog, PlacementController placement, LevelBuildOptions options)
        {
            var levelCompleteBanner = options.levelCompleteBanner;
            var enableHarmonyTypes = options.enableHarmonyTypes;
            var storyMode = options.storyMode;
            var enableStoryDialogs = options.enableStoryDialogs;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            Text MakeText(string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor align)
            {
                var go = new GameObject(name);
                go.transform.SetParent(canvasGo.transform, false);
                var textRect = go.AddComponent<RectTransform>();
                textRect.anchorMin = anchorMin;
                textRect.anchorMax = anchorMax;
                textRect.offsetMin = textRect.offsetMax = Vector2.zero;
                var t = go.AddComponent<Text>();
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.fontSize = fontSize;
                t.alignment = align;
                t.color = new Color(0.85f, 0.9f, 1f);
                t.raycastTarget = false;
                return t;
            }

            var ponders = MakeText("Ponders", new Vector2(0.02f, 0.92f), new Vector2(0.35f, 0.99f), 22, TextAnchor.UpperLeft);
            var status = MakeText("Status", new Vector2(0.3f, 0.92f), new Vector2(0.98f, 0.99f), 18, TextAnchor.UpperRight);

            var levelComplete = MakeText("LevelComplete", new Vector2(0.2f, 0.4f), new Vector2(0.8f, 0.6f), 36, TextAnchor.MiddleCenter);
            levelComplete.text = string.IsNullOrEmpty(levelCompleteBanner) ? "End of Level 1" : levelCompleteBanner;
            levelComplete.fontStyle = FontStyle.Bold;
            levelComplete.gameObject.SetActive(false);

            var kinds = storyMode || !enableHarmonyTypes
                ? new[] { BlobKind.Gatekeeper }
                : new[] { BlobKind.Gatekeeper, BlobKind.Creator, BlobKind.FourOhFour, BlobKind.Blaze };
            var buttons = new Button[kinds.Length];
            const float buttonWidth = 0.085f;
            const float buttonHeight = 0.10f;
            const float gap = 0.02f;
            const float bottomY = 0.02f;
            var totalWidth = kinds.Length * buttonWidth + (kinds.Length - 1) * gap;
            var startX = 0.5f - totalWidth * 0.5f;

            for (var i = 0; i < kinds.Length; i++)
            {
                var go = new GameObject($"BlobBtn_{kinds[i]}");
                go.transform.SetParent(canvasGo.transform, false);
                var btnRect = go.AddComponent<RectTransform>();
                var xMin = startX + i * (buttonWidth + gap);
                btnRect.anchorMin = new Vector2(xMin, bottomY);
                btnRect.anchorMax = new Vector2(xMin + buttonWidth, bottomY + buttonHeight);
                btnRect.offsetMin = btnRect.offsetMax = Vector2.zero;
                var img = go.AddComponent<Image>();
                img.color = new Color(0.25f, 0.2f, 0.45f, 0.85f);
                buttons[i] = go.AddComponent<Button>();

                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(go.transform, false);
                var lrt = labelGo.AddComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
                var txt = labelGo.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 11;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;
            }

            var hud = canvasGo.AddComponent<GameHud>();
            canvasGo.AddComponent<BlobUpgradePanel>();
            canvasGo.AddComponent<PauseMenu>();
            if (options.enableDevSkip)
                canvasGo.AddComponent<DevRoundSkip>();
            if (storyMode && enableStoryDialogs)
                canvasGo.AddComponent<StoryModeIntroController>();
            if (storyMode && enableStoryDialogs)
                canvasGo.AddComponent<StoryModeHarmonyTutorial>();
            if (storyMode && enableStoryDialogs)
                canvasGo.AddComponent<StoryModeRoundTwoTutorial>();
            if (storyMode && enableStoryDialogs)
                canvasGo.AddComponent<StoryModeSkyCompassRewardPopup>();
            if (enableHarmonyTypes)
            {
                canvasGo.AddComponent<HarmonyTypeHud>();
                canvasGo.AddComponent<SpeedTypeHud>();
                canvasGo.AddComponent<DreamTeamTypeHud>();
                canvasGo.AddComponent<AbsentHistoryTypeHud>();
                canvasGo.AddComponent<BrokenRingsTypeHud>();
                canvasGo.AddComponent<SlimySupportTypeHud>();
                canvasGo.AddComponent<EntertainerTypeHud>();
                canvasGo.AddComponent<TypeHudLayoutDriver>();
            }

            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("pondersText").objectReferenceValue = ponders;
            hudSo.FindProperty("statusText").objectReferenceValue = status;
            hudSo.FindProperty("levelCompleteText").objectReferenceValue = levelComplete;
            hudSo.FindProperty("placement").objectReferenceValue = placement;
            hudSo.FindProperty("catalog").objectReferenceValue = catalog;
            hudSo.FindProperty("blobButtons").arraySize = kinds.Length;
            hudSo.FindProperty("blobButtonKinds").arraySize = kinds.Length;
            for (var i = 0; i < kinds.Length; i++)
            {
                hudSo.FindProperty("blobButtons").GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
                hudSo.FindProperty("blobButtonKinds").GetArrayElementAtIndex(i).enumValueIndex = (int)kinds[i];
            }
            hudSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
