using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SproutOfDebt.UI;

namespace SproutOfDebt.UI.Editor
{
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private const string BackgroundSpritePath = "Assets/UI/Art/Backgrounds/menu_bg.png";
        private const string FontAssetPath = "Assets/UI/Fonts/PixelifySans_FontAsset.asset";
        private const string PrimaryBtnPrefabPath = "Assets/UI/Prefabs/Buttons/Button_Primary.prefab";
        private const string SecondaryBtnPrefabPath = "Assets/UI/Prefabs/Buttons/Button_Secondary.prefab";

        [MenuItem("Tools/UI/Build Main Menu Scene")]
        public static void BuildScene()
        {
            // 1. Create a new scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Camera
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.transform.position = new Vector3(0, 0, -10);

            // 3. EventSystem with InputSystemUIInputModule (Compatible with new Input System)
            var eventSystemGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            // 4. Canvas
            var canvasGo = new GameObject("MainMenu_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuController));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var controller = canvasGo.GetComponent<MainMenuController>();

            // 5. Background Image (menu_bg)
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var bgImg = bgGo.GetComponent<Image>();
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
            if (bgSprite != null)
            {
                bgImg.sprite = bgSprite;
                bgImg.preserveAspect = true;
            }

            // 6. Menu Container (Centered, width: 450, height: 750)
            var menuContainer = new GameObject("MenuContainer", typeof(RectTransform));
            menuContainer.transform.SetParent(canvasGo.transform, false);
            var menuRt = menuContainer.GetComponent<RectTransform>();
            menuRt.anchorMin = new Vector2(0.5f, 0.5f);
            menuRt.anchorMax = new Vector2(0.5f, 0.5f);
            menuRt.pivot = new Vector2(0.5f, 0.5f);
            menuRt.sizeDelta = new Vector2(450, 750);
            menuRt.anchoredPosition = new Vector2(0, -30);

            // 7. Title (Text: Sprout of Debt)
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(menuContainer.transform, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.sizeDelta = new Vector2(450, 220);
            titleRt.anchoredPosition = new Vector2(0, 0);

            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "Sprout\nof Debt";
            titleTmp.fontSize = 72;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.98f, 0.93f, 0.82f); // Warm parchment color

            var fontAsset = UIPrefabBuilder.EnsureFontAsset();
            if (fontAsset != null)
            {
                titleTmp.font = fontAsset;
            }

            // 8. Buttons Container (Vertical Layout)
            var btnContainer = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
            btnContainer.transform.SetParent(menuContainer.transform, false);
            var btnRt = btnContainer.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0f);
            btnRt.anchorMax = new Vector2(0.5f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0f);
            btnRt.sizeDelta = new Vector2(432, 450);
            btnRt.anchoredPosition = new Vector2(0, 40);

            var vlg = btnContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var primaryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrimaryBtnPrefabPath);
            var secondaryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SecondaryBtnPrefabPath);

            // Button: New Game (Primary)
            var newGameBtn = CreateButtonInstance(primaryPrefab, btnContainer.transform, "Button_NewGame", "NEW GAME");
            UnityEventTools.AddPersistentListener(newGameBtn.onClick, controller.OnNewGameClicked);

            // Button: Continue (Primary)
            var continueBtn = CreateButtonInstance(primaryPrefab, btnContainer.transform, "Button_Continue", "CONTINUE");
            UnityEventTools.AddPersistentListener(continueBtn.onClick, controller.OnContinueClicked);

            // Button: Settings (Secondary)
            var settingsBtn = CreateButtonInstance(secondaryPrefab, btnContainer.transform, "Button_Settings", "SETTINGS");
            UnityEventTools.AddPersistentListener(settingsBtn.onClick, controller.OnSettingsClicked);

            // Button: Quit (Secondary)
            var quitBtn = CreateButtonInstance(secondaryPrefab, btnContainer.transform, "Button_Quit", "QUIT");
            UnityEventTools.AddPersistentListener(quitBtn.onClick, controller.OnQuitClicked);

            // Hook serialized fields to controller
            var so = new SerializedObject(controller);
            so.FindProperty("continueButton").objectReferenceValue = continueBtn.gameObject;
            so.ApplyModifiedProperties();

            // 9. Save scene
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(newScene, ScenePath);
            Debug.Log("[MainMenuSceneBuilder] Successfully built and saved Main Menu scene at " + ScenePath);

            // 10. Ensure both MainMenu and SampleScene are in Build Settings
            AddSceneToBuildSettings(ScenePath, 0);
            AddSceneToBuildSettings("Assets/Scenes/SampleScene.unity", 1);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Button CreateButtonInstance(GameObject prefab, Transform parent, string name, string labelText)
        {
            GameObject instance;
            if (prefab != null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            }
            else
            {
                instance = new GameObject(name, typeof(RectTransform), typeof(Button));
                instance.transform.SetParent(parent, false);
            }

            instance.name = name;
            var rt = instance.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(432, 87);

            var tmp = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = labelText;
                tmp.fontSize = 28;
            }

            return instance.GetComponent<Button>();
        }

        private static void AddSceneToBuildSettings(string scenePath, int targetIndex)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            if (targetIndex == 0)
            {
                newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
                for (int i = 0; i < scenes.Length; i++)
                {
                    newScenes[i + 1] = scenes[i];
                }
            }
            else
            {
                for (int i = 0; i < scenes.Length; i++)
                {
                    newScenes[i] = scenes[i];
                }
                newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            }
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
