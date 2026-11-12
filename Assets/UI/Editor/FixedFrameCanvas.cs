using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.UI.Editor
{
    /// <summary>
    /// The game is laid out for one 1920x1080 (16:9) frame. On screens with another aspect ratio the frame stays
    /// 16:9 and is centered, and the rest of the screen stays black (the camera clear color), so nothing is stretched.
    /// Location canvases put their content into the frame made here instead of stretching it to the screen.
    /// </summary>
    public static class FixedFrameCanvas
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);

        /// <summary>
        /// Expand keeps the canvas at least 1920x1080 in both directions, so a centered 16:9 frame inside it is
        /// always exactly 1920x1080 reference pixels and every Figma position keeps working.
        /// </summary>
        public static void ConfigureScaler(CanvasScaler scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        /// <summary>
        /// Creates the centered 16:9 frame under the canvas. Put all of the canvas content inside it.
        /// It clips its content, so things sliding in from off-screen don't show up on the black bars.
        /// </summary>
        public static RectTransform CreateFrame(RectTransform canvas, string name = "Frame")
        {
            var frame = new GameObject(name, typeof(RectTransform), typeof(AspectRatioFitter), typeof(RectMask2D)).GetComponent<RectTransform>();
            frame.SetParent(canvas, false);
            frame.anchorMin = Vector2.zero;
            frame.anchorMax = Vector2.one;
            frame.offsetMin = Vector2.zero;
            frame.offsetMax = Vector2.zero;

            var fitter = frame.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = ReferenceResolution.x / ReferenceResolution.y;
            return frame;
        }
    }
}
