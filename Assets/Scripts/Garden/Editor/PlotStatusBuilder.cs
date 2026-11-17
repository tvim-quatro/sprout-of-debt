using System.IO;
using SproutOfDebt.Garden.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Garden.Editor
{
    /// <summary>
    /// Builds the prefab of the status indicator above a garden cell from the Figma component "garden_plot_status":
    /// a 64 px badge (needs water / growing / ready), a progress ring over it and the time-left plate under it.
    /// Called by GardenSceneBuilder, so the prefab is rebuilt together with the scene.
    /// </summary>
    public static class PlotStatusBuilder
    {
        public const string PrefabPath = "Assets/Prefabs/Garden/PlotStatus.prefab";

        private const string BarsArtPath = "Assets/UI/Art/Bars/";
        private const string TimerPlatePath = "Assets/UI/Art/Panels/ui_status_timer_plate.png";
        private const string FontPath = "Assets/UI/Fonts/PixelifySans_FontAsset.asset";

        // Figma sizes in pixels; the canvas is scaled so that 1 canvas unit = 1 px = 0.01 world units (PPU 100).
        private const float PixelsToWorld = 0.01f;
        private static readonly Vector2 Size = new Vector2(76f, 115f);
        private static readonly Vector2 BadgeSize = new Vector2(64f, 64f);
        private static readonly Vector2 TimerSize = new Vector2(76f, 45f);
        // Text padding inside the timer plate (Figma "Label plate": 16 px left/right, 8 px top/bottom).
        private static readonly Vector2 TimerPadding = new Vector2(16f, 8f);
        private const float TimerFontSize = 22f;
        private static readonly Color TimerTextColor = new Color32(0xFA, 0xED, 0xD1, 0xFF);

        private const string SortingLayer = "World";
        // Above the plants and the soil effects, below a held tool.
        private const int SortingOrder = 40;

        /// <summary>Builds (or rebuilds) the prefab and returns it.</summary>
        public static GameObject Build()
        {
            var root = new GameObject("PlotStatus", typeof(RectTransform), typeof(Canvas));
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = Size;
            rect.pivot = new Vector2(0.5f, 0f);
            rect.localScale = Vector3.one * PixelsToWorld;

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = SortingLayer;
            canvas.sortingOrder = SortingOrder;

            var badge = CreateImage("Badge", rect, LoadSprite(BarsArtPath + "ui_status_badge_water.png"), BadgeSize);
            var ring = CreateImage("Ring", badge.rectTransform, LoadSprite(BarsArtPath + "ui_status_ring.png"), BadgeSize);
            ring.type = Image.Type.Filled;
            ring.fillMethod = Image.FillMethod.Radial360;
            ring.fillOrigin = (int)Image.Origin360.Top;
            ring.fillClockwise = true;
            ring.fillAmount = 0f;

            var timer = CreateImage("Timer", rect, LoadSprite(TimerPlatePath), TimerSize);
            var text = CreateTimerText(timer.rectTransform);

            var view = root.AddComponent<PlotStatusView>();
            var so = new SerializedObject(view);
            so.FindProperty("badge").objectReferenceValue = badge.rectTransform;
            so.FindProperty("badgeImage").objectReferenceValue = badge;
            so.FindProperty("ring").objectReferenceValue = ring;
            so.FindProperty("timer").objectReferenceValue = timer.gameObject;
            so.FindProperty("timerText").objectReferenceValue = text;
            so.FindProperty("needsWaterBadge").objectReferenceValue = badge.sprite;
            so.FindProperty("growingBadge").objectReferenceValue = LoadSprite(BarsArtPath + "ui_status_badge_growing.png");
            so.FindProperty("readyBadge").objectReferenceValue = LoadSprite(BarsArtPath + "ui_status_badge_ready.png");
            so.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // Badge and timer stand on the bottom edge of the indicator, centered; the ring covers the badge.
        private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateTimerText(RectTransform plate)
        {
            var go = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = (RectTransform)go.transform;
            rect.SetParent(plate, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = TimerPadding;
            rect.offsetMax = -TimerPadding;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            text.fontSize = TimerFontSize;
            text.color = TimerTextColor;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            text.text = "0:00";
            return text;
        }

        private static Sprite LoadSprite(string path)
        {
            GardenEffectsBuilder.SetPixelArtImport(path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning("[PlotStatusBuilder] Sprite not found: " + path);
            }
            return sprite;
        }
    }
}
