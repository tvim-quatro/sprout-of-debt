using SproutOfDebt.Laboratory.UI;
using SproutOfDebt.UI.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Laboratory.Editor.RecipeBookUIFactory;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class LabFeedbackBuilder
    {
        private const string CirclePath = "Assets/UI/Art/Bars/ui_status_circle.png";
        private const string HourglassPath = "Assets/UI/Art/Icons/ui_icon_hourglass.png";
        private const string DefaultCursorPath = "Assets/UI/Art/Cursors/ui_cursor_default.png";
        private const string HoverCursorPath = "Assets/UI/Art/Cursors/ui_cursor_hover.png";
        private const string GrabCursorPath = "Assets/UI/Art/Cursors/ui_cursor_grab.png";
        private const string TimerCanvasName = "StirTimerCanvas";
        private const string CursorCanvasName = "CursorCanvas";

        private static readonly Color RingColor = new Color(0.96f, 0.68f, 0.18f);

        [MenuItem("Tools/Laboratory/Build Stir Timer And Cursor")]
        public static void Build()
        {
            BrewingStation station = Object.FindAnyObjectByType<BrewingStation>();
            LaboratoryDragController controller = Object.FindAnyObjectByType<LaboratoryDragController>();
            if (station == null || controller == null)
            {
                Debug.LogError("[LabFeedbackBuilder] Open a Laboratory scene with a BrewingStation and LaboratoryDragController");
                return;
            }

            Replace(TimerCanvasName);
            Replace(CursorCanvasName);
            BuildStirTimer(station);
            BuildCursor(controller);

            EditorSceneManager.MarkSceneDirty(station.gameObject.scene);
            EditorSceneManager.SaveScene(station.gameObject.scene);
            Debug.Log("[LabFeedbackBuilder] Stir timer and cursor built");
        }

        private static void Replace(string canvasName)
        {
            GameObject old = GameObject.Find(canvasName);
            if (old != null)
            {
                Object.DestroyImmediate(old);
            }
        }

        private static void BuildStirTimer(BrewingStation station)
        {
            RectTransform canvas = FixedFrameCanvas.CreateFrame(CreateCanvas(TimerCanvasName, 6));
            RectTransform root = MakeRect("StirTimer", canvas);
            Place(root, new Vector2(-160f, 10f), new Vector2(96f, 96f));

            Image back = MakeImage("Back", root, CirclePath);
            Stretch(back.rectTransform);
            Image ring = MakeImage("Ring", root, CirclePath);
            Stretch(ring.rectTransform);
            ring.color = RingColor;
            ring.type = Image.Type.Filled;
            ring.fillMethod = Image.FillMethod.Radial360;
            ring.fillOrigin = (int)Image.Origin360.Top;
            ring.fillClockwise = true;
            ring.fillAmount = 0f;
            Image face = MakeImage("Face", root, CirclePath);
            Place(face.rectTransform, Vector2.zero, new Vector2(78f, 78f));
            Image icon = MakeImage("Hourglass", root, HourglassPath);
            Place(icon.rectTransform, Vector2.zero, new Vector2(36f, 47f));

            StirTimerView view = canvas.gameObject.AddComponent<StirTimerView>();
            Set(view, "station", station);
            Set(view, "root", root.gameObject);
            Set(view, "ring", ring);
            root.gameObject.SetActive(false);
        }

        private static void BuildCursor(LaboratoryDragController controller)
        {
            RectTransform canvas = CreateCanvas(CursorCanvasName, 30);
            Image cursor = MakeImage("Cursor", canvas, HoverCursorPath);
            cursor.rectTransform.anchorMin = Vector2.zero;
            cursor.rectTransform.anchorMax = Vector2.zero;
            cursor.rectTransform.sizeDelta = new Vector2(64f, 64f);
            cursor.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultCursorPath);

            LaboratoryCursorView view = canvas.gameObject.AddComponent<LaboratoryCursorView>();
            Set(view, "controller", controller);
            Set(view, "cursorImage", cursor);
            Set(view, "defaultSprite", AssetDatabase.LoadAssetAtPath<Sprite>(DefaultCursorPath));
            Set(view, "hoverSprite", AssetDatabase.LoadAssetAtPath<Sprite>(HoverCursorPath));
            Set(view, "grabSprite", AssetDatabase.LoadAssetAtPath<Sprite>(GrabCursorPath));
        }

        private static RectTransform CreateCanvas(string name, int sortingOrder)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            FixedFrameCanvas.ConfigureScaler(scaler);
            return (RectTransform)go.transform;
        }
    }
}
