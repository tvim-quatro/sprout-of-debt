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
        private const string TimerCanvasName = "StirTimerCanvas";
        private const float TimerSize = 64f;
        private static readonly Vector2 TimerPosition = new Vector2(0f, 40f);
        // The Laboratory used to have its own software cursor; the game cursor (Control_GameCursor) replaces it.
        private const string OldCursorCanvasName = "CursorCanvas";
        private const string GameCursorName = "Control_GameCursor";

        private static readonly Color RingColor = new Color(0.96f, 0.68f, 0.18f);

        [MenuItem("Tools/Laboratory/Build Stir Timer")]
        public static void Build()
        {
            BrewingStation station = Object.FindAnyObjectByType<BrewingStation>();
            if (station == null)
            {
                Debug.LogError("[LabFeedbackBuilder] Open a Laboratory scene with a BrewingStation");
                return;
            }

            Replace(TimerCanvasName);
            Replace(OldCursorCanvasName);
            BuildStirTimer(station);
            EnsureGameCursor();

            EditorSceneManager.MarkSceneDirty(station.gameObject.scene);
            EditorSceneManager.SaveScene(station.gameObject.scene);
            Debug.Log("[LabFeedbackBuilder] Stir timer built");
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
            Place(root, TimerPosition, new Vector2(TimerSize, TimerSize));

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
            Place(face.rectTransform, Vector2.zero, new Vector2(TimerSize * 0.8f, TimerSize * 0.8f));
            Image icon = MakeImage("Hourglass", root, HourglassPath);
            Place(icon.rectTransform, Vector2.zero, new Vector2(TimerSize * 0.375f, TimerSize * 0.49f));

            StirTimerView view = canvas.gameObject.AddComponent<StirTimerView>();
            Set(view, "station", station);
            Set(view, "root", root.gameObject);
            Set(view, "ring", ring);
            root.gameObject.SetActive(false);
        }

        private static void EnsureGameCursor()
        {
            if (GameObject.Find(GameCursorName) == null)
            {
                GameCursorSetup.PlaceInScene();
            }
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
