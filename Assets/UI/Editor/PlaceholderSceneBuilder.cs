using System.IO;
using SproutOfDebt.Core.Stubs;
using SproutOfDebt.Shop.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.UI.Editor
{
    /// <summary>
    /// Builds placeholder scenes for the Garden and Laboratory areas,
    /// complete with camera, URP-ready canvas, background art, and navigation bar.
    /// </summary>
    public static class PlaceholderSceneBuilder
    {
        private const string GardenScenePath = "Assets/Scenes/Garden.unity";
        private const string LaboratoryScenePath = "Assets/Scenes/Laboratory.unity";
        private const string FontPath = "Assets/UI/Fonts/PixelifySans_FontAsset.asset";
        private static readonly Color Cream = new Color(0.98f, 0.93f, 0.82f);
        private static readonly Color Brown = new Color(0.24f, 0.15f, 0.09f);

        [MenuItem("Tools/Scenes/Build Garden Scene")]
        public static void BuildGardenScene()
        {
            BuildPlaceholderLocationScene(
                GardenScenePath,
                "Garden",
                "Assets/Art/Backgrounds/garden_bg.png",
                "GARDEN",
                "Plant beds & harvesting area\n[Placeholder - Work in progress]");
        }

        [MenuItem("Tools/Scenes/Build Laboratory Scene")]
        public static void BuildLaboratoryScene()
        {
            BuildPlaceholderLocationScene(
                LaboratoryScenePath,
                "Laboratory",
                "Assets/Art/Backgrounds/lab_bg.png",
                "LABORATORY",
                "Potion brewing cauldron & crafting station\n[Placeholder - Work in progress]");
        }

        [MenuItem("Tools/Scenes/Build All Location Scenes")]
        public static void BuildAll()
        {
            BuildGardenScene();
            BuildLaboratoryScene();
            ShopSceneBuilder.BuildScene();
            EnsureBuildSettings();
            Debug.Log("[PlaceholderSceneBuilder] Built Garden, Laboratory, and ShopCounter scenes successfully!");
        }

        private static void BuildPlaceholderLocationScene(
            string scenePath,
            string areaName,
            string bgSpritePath,
            string titleText,
            string subtitleText)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            // 1. Camera
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.transform.position = new Vector3(0, 0, -10);

            // 2. EventSystem with InputSystemUIInputModule
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            // 3. Stub Services Installer (so scene can run independently)
            new GameObject("Bootstrap", typeof(StubServicesInstaller));

            // 4. Canvas (1920x1080)
            var canvasGo = new GameObject(areaName + "_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var canvasRt = (RectTransform)canvasGo.transform;

            // 5. Background Image
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(canvasRt, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgSpritePath);
            bgImg.preserveAspect = false;

            // 6. Centered Info Box
            var infoBox = new GameObject("InfoBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            infoBox.transform.SetParent(canvasRt, false);
            var infoRt = infoBox.GetComponent<RectTransform>();
            infoRt.anchorMin = new Vector2(0.5f, 0.5f);
            infoRt.anchorMax = new Vector2(0.5f, 0.5f);
            infoRt.pivot = new Vector2(0.5f, 0.5f);
            infoRt.sizeDelta = new Vector2(640, 180);
            infoRt.anchoredPosition = new Vector2(0, 60);

            var infoImg = infoBox.GetComponent<Image>();
            var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/Panels/ui_dialog_box_9s.png");
            if (panelSprite != null)
            {
                infoImg.sprite = panelSprite;
                infoImg.type = Image.Type.Sliced;
            }
            else
            {
                infoImg.color = new Color(0, 0, 0, 0.75f);
            }

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(infoRt, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.5f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = new Vector2(-20, -15);
            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = titleText;
            titleTmp.fontSize = 38;
            titleTmp.color = Brown;
            titleTmp.alignment = TextAlignmentOptions.Center;
            if (font != null) titleTmp.font = font;

            // Subtitle
            var subGo = new GameObject("Subtitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(infoRt, false);
            var subRt = subGo.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 0f);
            subRt.anchorMax = new Vector2(1f, 0.5f);
            subRt.offsetMin = new Vector2(20, 15);
            subRt.offsetMax = new Vector2(-20, 0);
            var subTmp = subGo.GetComponent<TextMeshProUGUI>();
            subTmp.text = subtitleText;
            subTmp.fontSize = 22;
            subTmp.color = new Color(0.35f, 0.25f, 0.18f);
            subTmp.alignment = TextAlignmentOptions.Center;
            if (font != null) subTmp.font = font;

            // 7. Navigation Bar (bottom)
            LocationNavBarBuilder.BuildNavBar(canvasRt);

            // 8. Save Scene
            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(scene, scenePath);
            AddSceneToBuildSettings(scenePath);
            Debug.Log($"[PlaceholderSceneBuilder] Built {areaName} scene at: {scenePath}");
        }

        public static void EnsureBuildSettings()
        {
            AddSceneToBuildSettings("Assets/Scenes/MainMenu.unity", 0);
            AddSceneToBuildSettings("Assets/Scenes/ShopCounter.unity", 1);
            AddSceneToBuildSettings(GardenScenePath, 2);
            AddSceneToBuildSettings(LaboratoryScenePath, 3);
        }

        private static void AddSceneToBuildSettings(string scenePath, int targetIndex = -1)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);
            if (targetIndex >= 0 && targetIndex < list.Count)
            {
                list.Insert(targetIndex, new EditorBuildSettingsScene(scenePath, true));
            }
            else
            {
                list.Add(new EditorBuildSettingsScene(scenePath, true));
            }
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
