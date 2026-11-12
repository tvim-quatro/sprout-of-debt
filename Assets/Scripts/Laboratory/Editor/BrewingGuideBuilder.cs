using SproutOfDebt.Laboratory.UI;
using SproutOfDebt.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Laboratory.Editor.RecipeBookUIFactory;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class BrewingGuideBuilder
    {
        private const string WindowPrefabPath = "Assets/UI/Prefabs/Panels/Panel_Window.prefab";
        private const string CheckIconPath = "Assets/UI/Art/Icons/ui_icon_check.png";
        private const string CanvasName = "BrewingGuideCanvas";
        private const int StepCount = 6;
        private const float HeaderHeight = 86f;
        private const float TitleSize = 34f;
        private const float ContentTopInset = 100f;

        private static readonly Color HighlightColor = new Color(0.239f, 0.149f, 0.09f, 0.16f);

        [MenuItem("Tools/Laboratory/Build Brewing Guide")]
        public static void Build()
        {
            BrewingStation station = Object.FindAnyObjectByType<BrewingStation>();
            GameObject windowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WindowPrefabPath);
            if (station == null || windowPrefab == null)
            {
                Debug.LogError("[BrewingGuideBuilder] Open a Laboratory scene with a BrewingStation");
                return;
            }

            GameObject oldCanvas = GameObject.Find(CanvasName);
            if (oldCanvas != null)
            {
                Object.DestroyImmediate(oldCanvas);
            }

            LoadFont();
            RectTransform canvas = CreateCanvas();
            GameObject panel = (GameObject)PrefabUtility.InstantiatePrefab(windowPrefab, canvas);
            panel.name = "GuidePanel";
            RectTransform rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(44f, -165f);
            rect.sizeDelta = new Vector2(590f, 395f);

            TMP_Text title = panel.transform.Find("Header/TitleText").GetComponent<TMP_Text>();
            RectTransform header = (RectTransform)panel.transform.Find("Header");
            header.sizeDelta = new Vector2(header.sizeDelta.x, HeaderHeight);
            title.fontSize = TitleSize;
            title.alignment = TextAlignmentOptions.Center;
            RectTransform content = (RectTransform)panel.transform.Find("ContentArea");
            content.offsetMax = new Vector2(content.offsetMax.x, -ContentTopInset);
            Vertical(content.gameObject, 4, TextAnchor.UpperLeft).childForceExpandWidth = true;

            RecipeStepView[] rows = new RecipeStepView[StepCount];
            for (int i = 0; i < StepCount; i++)
            {
                rows[i] = BuildRow(content);
            }

            BrewingGuide guide = canvas.gameObject.AddComponent<BrewingGuide>();
            Set(guide, "station", station);
            Set(guide, "panel", panel);
            Set(guide, "titleText", title);
            SetArray(guide, "stepViews", rows);

            EditorSceneManager.MarkSceneDirty(station.gameObject.scene);
            EditorSceneManager.SaveScene(station.gameObject.scene);
            Debug.Log("[BrewingGuideBuilder] Brewing guide built");
        }

        private static RecipeStepView BuildRow(RectTransform parent)
        {
            RectTransform row = MakeRect("StepRow", parent);
            Image highlight = row.gameObject.AddComponent<Image>();
            highlight.color = HighlightColor;
            highlight.raycastTarget = false;
            highlight.enabled = false;
            HorizontalLayoutGroup layout = Horizontal(row.gameObject, 14, TextAnchor.MiddleLeft);
            layout.padding = new RectOffset(14, 30, 6, 6);
            Size(row, -1, 48);
            TMP_Text number = MakeText("Number", row, "", 26, false, Dark, TextAlignmentOptions.Center);
            Size(number, 36, 36);
            Image check = MakeImage("DoneMark", row, CheckIconPath);
            Size(check, 36, 36);
            check.gameObject.SetActive(false);
            TMP_Text text = MakeText("Text", row, "", 26, false, Dark, TextAlignmentOptions.Left);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
            text.fontSizeMin = 18f;
            text.fontSizeMax = 26f;
            Flexible(text, 1, 0);
            TMP_Text counter = MakeText("Counter", row, "", 26, true, Dark, TextAlignmentOptions.Right);
            counter.textWrappingMode = TextWrappingModes.NoWrap;
            Size(counter, 70, 36);
            counter.gameObject.SetActive(false);
            RecipeStepView view = row.gameObject.AddComponent<RecipeStepView>();
            Set(view, "numberText", number);
            Set(view, "stepText", text);
            Set(view, "counterText", counter);
            Set(view, "highlight", highlight);
            Set(view, "doneMark", check.gameObject);
            return view;
        }

        private static RectTransform CreateCanvas()
        {
            GameObject go = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            FixedFrameCanvas.ConfigureScaler(scaler);
            // Content goes in the fixed 16:9 frame, like every other screen.
            return FixedFrameCanvas.CreateFrame((RectTransform)go.transform);
        }
    }
}
