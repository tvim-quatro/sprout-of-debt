using SproutOfDebt.Core;
using SproutOfDebt.Core.Stubs;
using SproutOfDebt.Garden.UI;
using SproutOfDebt.UI.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace SproutOfDebt.Garden.Editor
{
    /// <summary>
    /// Builds the Garden scene from the Figma "Garden" frame: background, bed with 8 plots (each with its status
    /// indicator and harvest pop-up), tools (GardenToolsBuilder), the seeds the player drags out of the inventory,
    /// the toast and the shared HUD.
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
        private const string FxLayer = "FX";
        private const int PlotOrder = 0;
        private const int BedFrameOrder = 10;
        // Plants stand in front of the bed frame; the front row draws over the back row.
        private const int PlantOrder = 20;

        // Centers in Figma pixels (origin top-left), measured from the "Garden" frame.
        private static readonly Vector2 BedCenter = new Vector2(1194.5f, 693f);
        private static readonly float[] PlotColumnsX = { 914f, 1103f, 1289f, 1475f };
        private static readonly float[] PlotRowsY = { 598.5f, 744f };
        // Figma shows the picked ingredient at 66x84 px; the icon art is 120x153 px at 100 Pixels Per Unit.
        private const float HarvestIconScale = 0.55f;
        // Test stock of every seed for the stub inventory (enough to sow the whole bed with one kind),
        // until the real inventory and starter pack exist.

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
            GameCursorSetup.PlaceInScene();
            ToastSetup.PlaceInScene();
            var catalog = PlantCatalogBuilder.Build();
            var statusPrefab = PlotStatusBuilder.Build();
            CreateBootstrap();
            var input = new GameObject("GardenInput", typeof(GardenDragController)).GetComponent<GardenDragController>();
            CreateWorld(catalog, statusPrefab, input);
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

        // The game starts with no seeds (GDD): onboarding gives the first pack, the rest is bought in the Seed Shop.
        private static void CreateBootstrap()
        {
            new GameObject("Bootstrap").AddComponent<StubServicesInstaller>();
        }

        private static void CreateWorld(PlantCatalog catalog, GameObject statusPrefab, GardenDragController input)
        {
            CreateSprite("Background", null, BackgroundPath, new Vector2(ReferenceWidth / 2f, ReferenceHeight / 2f), BackgroundLayer, 0);

            var bed = new GameObject("GardenBed").transform;
            bed.position = ToWorld(BedCenter);
            CreateSprite("BedFrame", bed, GardenArtPath + "garden_bed_frame.png", BedCenter, WorldLayer, BedFrameOrder);

            // Plots are numbered left to right, top row first (Plot_1..Plot_4, then Plot_5..Plot_8);
            // the progress index is the same order, from 0.
            int index = 0;
            for (int row = 0; row < PlotRowsY.Length; row++)
            {
                foreach (float x in PlotColumnsX)
                {
                    var plot = CreatePlot(index, bed, new Vector2(x, PlotRowsY[row]), PlantOrder + row, catalog);
                    CreateStatus(plot, statusPrefab);
                    index++;
                }
            }

            GardenToolsBuilder.CreateTools(catalog, input);
        }

        private static GardenPlot CreatePlot(int index, Transform bed, Vector2 figmaCenter, int plantOrder, PlantCatalog catalog)
        {
            var soil = CreateSprite("Plot_" + (index + 1), bed, GardenArtPath + "garden_cell_empty.png", figmaCenter, WorldLayer, PlotOrder);
            // The collider takes the size of the soil sprite, so it is added after the sprite is set.
            soil.gameObject.AddComponent<BoxCollider2D>();
            var plant = CreateSprite("Plant", soil.transform, null, figmaCenter, WorldLayer, plantOrder);
            // Click area of the ripe plant; GardenPlot fits it to the plant sprite and turns it on only when ripe.
            var plantCollider = plant.gameObject.AddComponent<BoxCollider2D>();
            plantCollider.enabled = false;

            var tillEffect = GardenEffectsBuilder.CreateTillEffect(soil.transform);

            var plot = soil.gameObject.AddComponent<GardenPlot>();
            var so = new SerializedObject(plot);
            so.FindProperty("index").intValue = index;
            so.FindProperty("catalog").objectReferenceValue = catalog;
            so.FindProperty("tillEffect").objectReferenceValue = tillEffect;
            so.FindProperty("soilRenderer").objectReferenceValue = soil;
            so.FindProperty("plantRenderer").objectReferenceValue = plant;
            so.FindProperty("plantCollider").objectReferenceValue = plantCollider;
            so.FindProperty("emptySoil").objectReferenceValue = soil.sprite;
            so.FindProperty("tilledSoil").objectReferenceValue = LoadSprite("garden_cell_tilled");
            so.FindProperty("sownSoil").objectReferenceValue = LoadSprite("garden_cell_sown");
            so.FindProperty("wateredSoil").objectReferenceValue = LoadSprite("garden_cell_watered");
            so.ApplyModifiedPropertiesWithoutUndo();
            CreateHarvestFeedback(plot, figmaCenter);
            return plot;
        }

        // The picked ingredient that pops up out of the plant and the "+1 ..." toast (Figma "Garden - Harvested").
        private static void CreateHarvestFeedback(GardenPlot plot, Vector2 figmaCenter)
        {
            var icon = CreateSprite("HarvestIcon", plot.transform, null, figmaCenter, FxLayer, 0);
            icon.transform.localScale = Vector3.one * HarvestIconScale;
            icon.enabled = false;
            var feedback = plot.gameObject.AddComponent<HarvestFeedback>();
            var so = new SerializedObject(feedback);
            so.FindProperty("iconRenderer").objectReferenceValue = icon;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // The status indicator above the cell (Figma "garden_plot_status"); it places itself above the plant.
        private static void CreateStatus(GardenPlot plot, GameObject statusPrefab)
        {
            var status = (GameObject)PrefabUtility.InstantiatePrefab(statusPrefab, plot.transform);
            status.transform.localPosition = Vector3.zero;
            var so = new SerializedObject(status.GetComponent<PlotStatusView>());
            so.FindProperty("plot").objectReferenceValue = plot;
            so.FindProperty("plantRenderer").objectReferenceValue = plot.transform.Find("Plant").GetComponent<SpriteRenderer>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite LoadSprite(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(GardenArtPath + fileName + ".png");
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

        internal static SpriteRenderer CreateSprite(string name, Transform parent, string spritePath, Vector2 figmaCenter, string sortingLayer, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.position = ToWorld(figmaCenter);

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = order;
            if (spritePath == null)
            {
                return renderer;
            }
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
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
