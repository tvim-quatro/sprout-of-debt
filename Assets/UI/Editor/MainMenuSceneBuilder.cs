using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SproutOfDebt.UI.Editor
{
    /// <summary>
    /// Builds the Main Menu scene from Figma "Main menu" (111:396) and "Quit" (108:1462):
    /// full-frame background art, left-aligned menu column (title + New game / Continue / Quit),
    /// and the Quit confirmation modal.
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private const string BackgroundSpritePath = "Assets/UI/Art/Backgrounds/menu_bg.png";
        private const string WindowPanelPath = "Assets/UI/Art/Panels/ui_panel_window_9s.png";
        private const string HeaderPanelPath = "Assets/UI/Art/Panels/ui_panel_header_9s.png";
        private const string PrimaryBtnPrefabPath = "Assets/UI/Prefabs/Buttons/Button_Primary.prefab";
        private const string SecondaryBtnPrefabPath = "Assets/UI/Prefabs/Buttons/Button_Secondary.prefab";
        private const string CloseBtnPrefabPath = "Assets/UI/Prefabs/Buttons/Button_Close.prefab";

        // Figma "Main menu" (111:396) coordinates in 1920x1080 reference pixels.
        private static readonly Vector2 MenuPosition = new Vector2(69f, -249f);
        private static readonly Vector2 MenuSize = new Vector2(432f, 581f);
        private static readonly Vector2 TitleSize = new Vector2(432f, 208f);
        private static readonly Vector2 ButtonsPosition = new Vector2(0f, -272f);
        private static readonly Vector2 ButtonsSize = new Vector2(432f, 309f);
        private static readonly Vector2 MenuButtonSize = new Vector2(432f, 87f);
        private const float ButtonSpacing = 24f;
        private const float TitleFontSize = 84f;
        private const float ButtonFontSize = 26f;

        private static readonly Color Cream = new Color(0.98f, 0.929f, 0.82f);
        private static readonly Color Brown = new Color(0.24f, 0.15f, 0.09f);

        [MenuItem("Tools/UI/Build Main Menu Scene")]
        public static void BuildScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            GameCursorSetup.PlaceInScene();

            var canvasGo = new GameObject("MainMenu_Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuController));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            FixedFrameCanvas.ConfigureScaler(canvasGo.GetComponent<CanvasScaler>());
            var frame = FixedFrameCanvas.CreateFrame((RectTransform)canvasGo.transform);

            var controller = canvasGo.GetComponent<MainMenuController>();
            var fontAsset = UIPrefabBuilder.EnsureFontAsset();
            var primaryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrimaryBtnPrefabPath);
            var secondaryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SecondaryBtnPrefabPath);

            CreateBackground(frame);
            var continueBtn = CreateMenuColumn(frame, controller, fontAsset, primaryPrefab, secondaryPrefab);
            var quitOverlay = CreateModal(frame, fontAsset, primaryPrefab, secondaryPrefab,
                new ModalTexts("QuitModal", "Quit?", "Leaving already?\nThe debt won't pay itself...", "Leave", "Stay"),
                controller.OnConfirmQuitClicked, controller.OnCancelQuitClicked);
            var newGameOverlay = CreateModal(frame, fontAsset, primaryPrefab, secondaryPrefab,
                new ModalTexts("NewGameModal", "New game?", "Starting over erases\nyour saved progress.", "Start over", "Cancel"),
                controller.OnConfirmNewGameClicked, controller.OnCancelNewGameClicked);

            var so = new SerializedObject(controller);
            so.FindProperty("gameSceneName").stringValue = "Garden";
            so.FindProperty("continueButton").objectReferenceValue = continueBtn.gameObject;
            so.FindProperty("quitPanel").objectReferenceValue = quitOverlay;
            so.FindProperty("newGamePanel").objectReferenceValue = newGameOverlay;
            so.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(newScene, ScenePath);
            Debug.Log("[MainMenuSceneBuilder] Built and saved Main Menu scene at " + ScenePath);

            AddSceneToBuildSettings(ScenePath, 0);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateCamera()
        {
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateBackground(RectTransform frame)
        {
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(frame, false);
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
        }

        private static Button CreateMenuColumn(RectTransform frame, MainMenuController controller,
            TMP_FontAsset fontAsset, GameObject primaryPrefab, GameObject secondaryPrefab)
        {
            var menuContainer = new GameObject("MenuContainer", typeof(RectTransform));
            menuContainer.transform.SetParent(frame, false);
            var menuRt = menuContainer.GetComponent<RectTransform>();
            menuRt.anchorMin = menuRt.anchorMax = menuRt.pivot = new Vector2(0f, 1f);
            menuRt.anchoredPosition = MenuPosition;
            menuRt.sizeDelta = MenuSize;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(menuRt, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = titleRt.anchorMax = titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = Vector2.zero;
            titleRt.sizeDelta = TitleSize;

            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "Sprout of Debt";
            titleTmp.fontSize = TitleFontSize;
            titleTmp.alignment = TextAlignmentOptions.Top;
            titleTmp.color = Cream;
            if (fontAsset != null) titleTmp.font = fontAsset;

            var btnContainer = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
            btnContainer.transform.SetParent(menuRt, false);
            var btnRt = btnContainer.GetComponent<RectTransform>();
            btnRt.anchorMin = btnRt.anchorMax = btnRt.pivot = new Vector2(0.5f, 1f);
            btnRt.anchoredPosition = ButtonsPosition;
            btnRt.sizeDelta = ButtonsSize;

            var vlg = btnContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = ButtonSpacing;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var newGameBtn = CreateButtonInstance(primaryPrefab, btnRt, "Button_NewGame", "New game", MenuButtonSize, ButtonFontSize);
            UnityEventTools.AddPersistentListener(newGameBtn.onClick, controller.OnNewGameClicked);

            var continueBtn = CreateButtonInstance(primaryPrefab, btnRt, "Button_Continue", "Continue", MenuButtonSize, ButtonFontSize);
            UnityEventTools.AddPersistentListener(continueBtn.onClick, controller.OnContinueClicked);

            var quitBtn = CreateButtonInstance(secondaryPrefab, btnRt, "Button_Quit", "Quit", MenuButtonSize, ButtonFontSize);
            UnityEventTools.AddPersistentListener(quitBtn.onClick, controller.OnQuitClicked);

            return continueBtn;
        }

        private readonly struct ModalTexts
        {
            public readonly string Name;
            public readonly string Title;
            public readonly string Message;
            public readonly string ConfirmLabel;
            public readonly string CancelLabel;

            public ModalTexts(string name, string title, string message, string confirmLabel, string cancelLabel)
            {
                Name = name;
                Title = title;
                Message = message;
                ConfirmLabel = confirmLabel;
                CancelLabel = cancelLabel;
            }
        }

        // Figma "Quit window" (165:6113): 760x500 centered over dim overlay. The same window asks for any confirmation.
        private static GameObject CreateModal(RectTransform frame, TMP_FontAsset fontAsset, GameObject primaryPrefab,
            GameObject secondaryPrefab, ModalTexts texts, UnityAction onConfirm, UnityAction onCancel)
        {
            var overlay = new GameObject(texts.Name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            overlay.transform.SetParent(frame, false);
            var overlayRt = overlay.GetComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = new Color(0.05f, 0.02f, 0.01f, 0.6f);
            var dimBtn = overlay.GetComponent<Button>();
            dimBtn.transition = Selectable.Transition.None;
            UnityEventTools.AddPersistentListener(dimBtn.onClick, onCancel);

            var windowGo = new GameObject(texts.Name.Replace("Modal", "Window"), typeof(RectTransform));
            windowGo.transform.SetParent(overlayRt, false);
            var winRt = windowGo.GetComponent<RectTransform>();
            winRt.anchorMin = winRt.anchorMax = winRt.pivot = new Vector2(0.5f, 0.5f);
            winRt.anchoredPosition = Vector2.zero;
            winRt.sizeDelta = new Vector2(760f, 500f);

            var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGo.transform.SetParent(winRt, false);
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = panelRt.anchorMax = panelRt.pivot = new Vector2(0.5f, 1f);
            panelRt.anchoredPosition = new Vector2(0f, -40f);
            panelRt.sizeDelta = new Vector2(760f, 460f);
            var panelImg = panelGo.GetComponent<Image>();
            panelImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WindowPanelPath);
            panelImg.type = Image.Type.Sliced;

            var headerGo = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            headerGo.transform.SetParent(winRt, false);
            var headerRt = headerGo.GetComponent<RectTransform>();
            headerRt.anchorMin = headerRt.anchorMax = headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = new Vector2(0f, -11f);
            headerRt.sizeDelta = new Vector2(378f, 90f);
            var headerImg = headerGo.GetComponent<Image>();
            headerImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HeaderPanelPath);
            headerImg.type = Image.Type.Sliced;

            var headerTextGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            headerTextGo.transform.SetParent(headerRt, false);
            var htRt = headerTextGo.GetComponent<RectTransform>();
            htRt.anchorMin = Vector2.zero;
            htRt.anchorMax = Vector2.one;
            htRt.offsetMin = htRt.offsetMax = Vector2.zero;
            var htTmp = headerTextGo.GetComponent<TextMeshProUGUI>();
            htTmp.text = texts.Title;
            htTmp.fontSize = 32f;
            htTmp.alignment = TextAlignmentOptions.Center;
            htTmp.color = Cream;
            if (fontAsset != null) htTmp.font = fontAsset;

            var closePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CloseBtnPrefabPath);
            var closeBtn = CreateButtonInstance(closePrefab, winRt, "Button_Close", null, new Vector2(70f, 72f), 0f);
            var closeRt = (RectTransform)closeBtn.transform;
            closeRt.anchorMin = closeRt.anchorMax = closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(17f, -20f);
            UnityEventTools.AddPersistentListener(closeBtn.onClick, onCancel);

            var msgGo = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            msgGo.transform.SetParent(winRt, false);
            var msgRt = msgGo.GetComponent<RectTransform>();
            msgRt.anchorMin = msgRt.anchorMax = msgRt.pivot = new Vector2(0.5f, 1f);
            msgRt.anchoredPosition = new Vector2(0f, -200f);
            msgRt.sizeDelta = new Vector2(640f, 80f);
            var msgTmp = msgGo.GetComponent<TextMeshProUGUI>();
            msgTmp.text = texts.Message;
            msgTmp.fontSize = 26f;
            msgTmp.alignment = TextAlignmentOptions.Center;
            msgTmp.color = Brown;
            if (fontAsset != null) msgTmp.font = fontAsset;

            var leaveBtn = CreateButtonInstance(secondaryPrefab, winRt, "Button_Leave", texts.ConfirmLabel, new Vector2(308f, 63f), ButtonFontSize);
            var leaveRt = (RectTransform)leaveBtn.transform;
            leaveRt.anchorMin = leaveRt.anchorMax = leaveRt.pivot = new Vector2(0.5f, 1f);
            leaveRt.anchoredPosition = new Vector2(-166f, -308f);
            UnityEventTools.AddPersistentListener(leaveBtn.onClick, onConfirm);

            var stayBtn = CreateButtonInstance(primaryPrefab, winRt, "Button_Stay", texts.CancelLabel, new Vector2(308f, 63f), ButtonFontSize);
            var stayRt = (RectTransform)stayBtn.transform;
            stayRt.anchorMin = stayRt.anchorMax = stayRt.pivot = new Vector2(0.5f, 1f);
            stayRt.anchoredPosition = new Vector2(166f, -308f);
            UnityEventTools.AddPersistentListener(stayBtn.onClick, onCancel);

            overlay.SetActive(false);
            return overlay;
        }

        private static Button CreateButtonInstance(GameObject prefab, Transform parent, string name,
            string labelText, Vector2 size, float fontSize)
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
            rt.sizeDelta = size;

            var tmp = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null && labelText != null)
            {
                tmp.text = labelText;
                if (fontSize > 0f) tmp.fontSize = fontSize;
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
                for (int i = 0; i < scenes.Length; i++) newScenes[i + 1] = scenes[i];
            }
            else
            {
                for (int i = 0; i < scenes.Length; i++) newScenes[i] = scenes[i];
                newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            }
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
