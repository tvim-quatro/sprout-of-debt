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
        // The same badge and progress ring the garden shows above a growing plant (Figma "garden_plot_status").
        private const string BadgePath = "Assets/UI/Art/Bars/ui_status_badge_growing.png";
        private const string RingPath = "Assets/UI/Art/Bars/ui_status_ring.png";
        private const string TimerCanvasName = "StirTimerCanvas";
        private const float TimerSize = 64f;
        private static readonly Vector2 TimerPosition = new Vector2(0f, 44f);
        // The Laboratory used to have its own software cursor; the game cursor (Control_GameCursor) replaces it.
        private const string OldCursorCanvasName = "CursorCanvas";
        private const string GameCursorName = "Control_GameCursor";

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
            // Figma "Laboratory - Stirring" (lab_stir_progress): 64x64, centered over the bowl, top 464 px from the frame top.
            RectTransform root = MakeRect("StirTimer", canvas);
            Place(root, TimerPosition, new Vector2(TimerSize, TimerSize));

            Image badge = MakeImage("Badge", root, BadgePath);
            Stretch(badge.rectTransform);
            Image ring = MakeImage("Ring", badge.rectTransform, RingPath);
            Stretch(ring.rectTransform);
            ring.type = Image.Type.Filled;
            ring.fillMethod = Image.FillMethod.Radial360;
            ring.fillOrigin = (int)Image.Origin360.Top;
            ring.fillClockwise = true;
            ring.fillAmount = 0f;

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
