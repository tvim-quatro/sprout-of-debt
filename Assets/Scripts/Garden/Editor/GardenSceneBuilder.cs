using SproutOfDebt.Core;
using SproutOfDebt.Core.Stubs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace SproutOfDebt.Garden.Editor
{
    /// <summary>
    /// Builds the Garden scene from the Figma "Garden" frame: background, bed with 8 plots, tools and the shared HUD.
    /// Re-running it rebuilds the scene from scratch, so change the layout here, not in the scene.
    /// </summary>
    public static class GardenSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Garden.unity";
        private const string BackgroundPath = "Assets/Art/Backgrounds/garden_bg.png";
        private const string GardenArtPath = "Assets/Art/Garden/";
        // Shared interface frame (top bar, nav bar, windows), built by the Shop editor tools.
        private const string HudPrefabPath = "Assets/UI/Prefabs/Canvases/SOD_HUD_Canvas.prefab";
        private const string BuildHudMenu = "Tools/Shop/Build HUD Prefab";

        // Figma frame is 1920x1080 and sprites use 100 Pixels Per Unit, so the camera shows 19.2 x 10.8 units.
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float PixelsPerUnit = 100f;

        private const string BackgroundLayer = "Background";
        private const string WorldLayer = "World";
        private const int PlotOrder = 0;
        private const int BedFrameOrder = 10;
        private const int ToolOrder = 30;

        // Centers in Figma pixels (origin top-left), measured from the "Garden" frame.
        private static readonly Vector2 BedCenter = new Vector2(1194.5f, 693f);
        private static readonly float[] PlotColumnsX = { 914f, 1103f, 1289f, 1475f };
        private static readonly float[] PlotRowsY = { 598.5f, 744f };
        private static readonly Vector2 RakeCenter = new Vector2(407f, 684f);
        private static readonly Vector2 WateringCanCenter = new Vector2(282.6f, 902.8f);

        // Test stock for the stub inventory until the real inventory and starter pack exist.
        private static readonly string[] StarterSeedIds =
        {
            "seed_lavender", "seed_moonpetal", "seed_nightshade", "seed_emberroot", "seed_mandrake"
        };
        private const int StarterSeedCount = 5;

        [MenuItem("Tools/Garden/Build Garden Scene")]
        public static void BuildScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            CreateBootstrap();
            CreateWorld();
            CreateHud();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[GardenSceneBuilder] Built " + ScenePath);
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraGo.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = ReferenceHeight / 2f / PixelsPerUnit;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            // Shows the whole 16:9 frame on any screen, with black bars instead of stretching (same as the UI frame).
            cameraGo.AddComponent<FixedAspectCamera>();
        }

        private static void CreateBootstrap()
        {
            var installer = new GameObject("Bootstrap").AddComponent<StubServicesInstaller>();
            var so = new SerializedObject(installer);
            var items = so.FindProperty("startingItems");
            items.arraySize = StarterSeedIds.Length;
            for (int i = 0; i < StarterSeedIds.Length; i++)
            {
                var item = items.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("itemId").stringValue = StarterSeedIds[i];
                item.FindPropertyRelative("count").intValue = StarterSeedCount;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateWorld()
        {
            CreateSprite("Background", null, BackgroundPath, new Vector2(ReferenceWidth / 2f, ReferenceHeight / 2f), BackgroundLayer, 0);

            var bed = new GameObject("GardenBed").transform;
            bed.position = ToWorld(BedCenter);
            CreateSprite("BedFrame", bed, GardenArtPath + "garden_bed_frame.png", BedCenter, WorldLayer, BedFrameOrder);

            // Plots are numbered left to right, top row first (Plot_1..Plot_4, then Plot_5..Plot_8).
            int number = 1;
            foreach (float y in PlotRowsY)
            {
                foreach (float x in PlotColumnsX)
                {
                    CreateSprite("Plot_" + number, bed, GardenArtPath + "garden_cell_empty.png", new Vector2(x, y), WorldLayer, PlotOrder);
                    number++;
                }
            }

            var tools = new GameObject("Tools").transform;
            CreateSprite("Rake", tools, GardenArtPath + "garden_rake.png", RakeCenter, WorldLayer, ToolOrder);
            CreateSprite("WateringCan", tools, GardenArtPath + "garden_watering_can.png", WateringCanCenter, WorldLayer, ToolOrder);
        }

        private static void CreateHud()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath) == null)
            {
                EditorApplication.ExecuteMenuItem(BuildHudMenu);
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[GardenSceneBuilder] HUD prefab not found, run " + BuildHudMenu);
                return;
            }
            PrefabUtility.InstantiatePrefab(prefab);
        }

        private static SpriteRenderer CreateSprite(string name, Transform parent, string spritePath, Vector2 figmaCenter, string sortingLayer, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.position = ToWorld(figmaCenter);

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = order;
            if (renderer.sprite == null)
            {
                Debug.LogWarning("[GardenSceneBuilder] Sprite not found: " + spritePath);
            }
            return renderer;
        }

        // Converts a Figma position (pixels, origin top-left) to world units (origin at the screen center).
        private static Vector3 ToWorld(Vector2 figmaPosition)
        {
            return new Vector3(
                (figmaPosition.x - ReferenceWidth / 2f) / PixelsPerUnit,
                (ReferenceHeight / 2f - figmaPosition.y) / PixelsPerUnit,
                0f);
        }
    }
}
