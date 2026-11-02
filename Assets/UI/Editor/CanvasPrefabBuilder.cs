using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SproutOfDebt.UI.Editor.UIPrefabUtils;

namespace SproutOfDebt.UI.Editor
{
    // Builds the sample UI canvas prefab (Prefabs/Canvases).
    internal static class CanvasPrefabBuilder
    {
        internal static void BuildSampleCanvas(TMP_FontAsset font)
        {
            var canvasGo = new GameObject("SOD_UI_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Background
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = LoadSprite("Backgrounds/menu_bg.png");

            SavePrefab(canvasGo, Path.Combine(PrefabsRoot, "Canvases/SOD_UI_Canvas.prefab"));
            Object.DestroyImmediate(canvasGo);
        }
    }
}
