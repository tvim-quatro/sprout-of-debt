using SproutOfDebt.Tutorial.UI;
using SproutOfDebt.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Tutorial.Editor
{
    /// <summary>
    /// Builds the tutorial overlay inside the shared HUD frame: dim with cutouts, moneylender dialogue
    /// and imp hint (the last two come from <see cref="TutorialDialogueBuilder"/>). Figma: Onboarding page.
    /// </summary>
    public static class TutorialOverlayBuilder
    {
        internal const string UIArt = "Assets/UI/Art";
        internal const string CharArt = "Assets/Art/Characters";
        private const int OverlaySortingOrder = 15;

        public static RectTransform BuildTutorialOverlay(RectTransform parent)
        {
            var overlayGo = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(TutorialManager), typeof(TutorialOverlayController));
            overlayGo.transform.SetParent(parent, false);
            var overlayRt = (RectTransform)overlayGo.transform;
            Stretch(overlayRt);

            // Above the HUD (10) and location popups such as the recipe book (12), below toasts (20).
            var overlayCanvas = overlayGo.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = OverlaySortingOrder;
            overlayGo.AddComponent<GraphicRaycaster>();

            var controller = overlayGo.GetComponent<TutorialOverlayController>();

            var dimMask = BuildDimMask(overlayRt);
            var moneylenderView = TutorialDialogueBuilder.BuildMoneylenderDialogue(overlayRt);
            var hintView = TutorialDialogueBuilder.BuildTutorialHint(overlayRt);

            var so = new SerializedObject(controller);
            so.FindProperty("dimMask").objectReferenceValue = dimMask;
            so.FindProperty("moneylenderView").objectReferenceValue = moneylenderView;
            so.FindProperty("hintView").objectReferenceValue = hintView;
            so.ApplyModifiedPropertiesWithoutUndo();

            overlayGo.SetActive(true);
            return overlayRt;
        }

        // Figma "Dim overlay": the whole frame minus the cutouts; "Highlight": a gold frame of the cutout's size.
        private static TutorialDimMask BuildDimMask(RectTransform parent)
        {
            var go = new GameObject("DimMask", typeof(RectTransform), typeof(TutorialDimMask));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            Stretch(rt);

            var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(TutorialDimGraphic));
            dimGo.transform.SetParent(rt, false);
            Stretch((RectTransform)dimGo.transform);
            var dim = dimGo.GetComponent<TutorialDimGraphic>();
            dim.color = new Color(0f, 0f, 0f, 0.85f);
            dim.raycastTarget = true;

            // A drawn 3 px outline, not a stretched sprite: on small targets (status badge, round buttons)
            // a scaled-down sprite lost its 3 px lines on some edges.
            var highlight = new GameObject("HighlightFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(RectOutlineGraphic));
            highlight.transform.SetParent(rt, false);
            var hlRt = (RectTransform)highlight.transform;
            hlRt.anchorMin = hlRt.anchorMax = hlRt.pivot = new Vector2(0.5f, 0.5f);
            var outline = highlight.GetComponent<RectOutlineGraphic>();
            outline.color = new Color32(0xFF, 0xE0, 0x8A, 0xFF);
            outline.raycastTarget = false;
            highlight.SetActive(false);

            var mask = go.GetComponent<TutorialDimMask>();
            var so = new SerializedObject(mask);
            so.FindProperty("dimGraphic").objectReferenceValue = dim;
            so.FindProperty("highlightFrame").objectReferenceValue = hlRt;
            so.ApplyModifiedPropertiesWithoutUndo();

            return mask;
        }

        internal static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        internal static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
