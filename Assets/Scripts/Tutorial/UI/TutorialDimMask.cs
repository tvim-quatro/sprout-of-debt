using System.Collections.Generic;
using UnityEngine;

namespace SproutOfDebt.Tutorial.UI
{
    /// <summary>
    /// Dark dim overlay with rectangular cutouts over the interactable targets,
    /// plus a gold highlight frame (#FFE08A) on each cutout (Figma: Dim overlay + Highlight).
    /// When an item is focused, the screen is darkened with high opacity (85%).
    /// </summary>
    public class TutorialDimMask : MonoBehaviour
    {
        [Header("Dim")]
        [SerializeField] private TutorialDimGraphic dimGraphic;

        [Header("Dim Opacity")]
        [SerializeField, Range(0f, 1f)] private float fullDimAlpha = 0.70f;
        [SerializeField, Range(0f, 1f)] private float focusedDimAlpha = 0.85f;

        [Header("Highlight")]
        [Tooltip("First highlight frame; more are cloned from it when a step has several cutouts.")]
        [SerializeField] private RectTransform highlightFrame;
        [Tooltip("Space between the target and the cutout edge (Figma: 6 px around the target).")]
        [SerializeField] private float holePadding = 6f;

        private readonly List<RectTransform> frames = new List<RectTransform>();
        private readonly List<Rect> paddedHoles = new List<Rect>();
        private readonly List<Rect> paddedFrames = new List<Rect>();
        private RectTransform dimRect;

        private void Awake()
        {
            dimRect = GetComponent<RectTransform>();
        }

        public void ShowFullscreenDim()
        {
            gameObject.SetActive(true);
            paddedHoles.Clear();
            paddedFrames.Clear();
            ApplyDim(fullDimAlpha);
            ShowFrames(0);
        }

        /// <summary>
        /// Cuts the holes and frames the highlighted targets (both in canvas frame space).
        /// clickThrough = the player must use the target; otherwise it is only shown and stays blocked.
        /// </summary>
        public void SetCutouts(IReadOnlyList<Rect> holesInCanvasSpace, IReadOnlyList<Rect> framesInCanvasSpace, bool clickThrough)
        {
            gameObject.SetActive(true);
            if (dimGraphic != null)
            {
                dimGraphic.HolesClickThrough = clickThrough;
            }
            if (dimRect == null)
            {
                dimRect = GetComponent<RectTransform>();
            }
            Rect bounds = dimRect != null ? dimRect.rect : new Rect(-960f, -540f, 1920f, 1080f);

            Pad(holesInCanvasSpace, paddedHoles, bounds);
            Pad(framesInCanvasSpace, paddedFrames, bounds);
            ApplyDim(focusedDimAlpha);
            ShowFrames(paddedFrames.Count);
        }

        private void Pad(IReadOnlyList<Rect> source, List<Rect> result, Rect bounds)
        {
            result.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                // Whole reference pixels, so the 3 px frame lines stay crisp and equal on every side.
                Rect r = source[i];
                result.Add(Rect.MinMaxRect(
                    Mathf.Clamp(Mathf.Floor(r.xMin - holePadding), bounds.xMin, bounds.xMax),
                    Mathf.Clamp(Mathf.Floor(r.yMin - holePadding), bounds.yMin, bounds.yMax),
                    Mathf.Clamp(Mathf.Ceil(r.xMax + holePadding), bounds.xMin, bounds.xMax),
                    Mathf.Clamp(Mathf.Ceil(r.yMax + holePadding), bounds.yMin, bounds.yMax)));
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ShowFrames(0);
        }

        private void ApplyDim(float alpha)
        {
            if (dimGraphic == null) return;
            dimGraphic.color = new Color(0f, 0f, 0f, alpha);
            dimGraphic.SetHoles(paddedHoles);
        }

        // In Figma the highlight frame has exactly the cutout's size.
        private void ShowFrames(int count)
        {
            if (highlightFrame == null) return;
            if (frames.Count == 0)
            {
                frames.Add(highlightFrame);
            }
            while (frames.Count < count)
            {
                frames.Add(Instantiate(highlightFrame, highlightFrame.parent));
            }
            for (int i = 0; i < frames.Count; i++)
            {
                bool shown = i < count;
                frames[i].gameObject.SetActive(shown);
                if (shown)
                {
                    frames[i].anchoredPosition = paddedFrames[i].center;
                    frames[i].sizeDelta = paddedFrames[i].size;
                }
            }
        }
    }
}
