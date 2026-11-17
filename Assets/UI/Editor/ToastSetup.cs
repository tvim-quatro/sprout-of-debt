using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.UI.Editor
{
    /// <summary>
    /// Builds Panel_Toast from the Figma component "Toast": its own overlay canvas with the 16:9 frame and the banner
    /// (ui_toast_banner, a fixed-size picture) with a bold 32 px cream line, at the place shown in "Garden - Harvested".
    /// Scene builders place it once (PlaceInScene); gameplay code shows messages through GameServices.Toasts.
    /// </summary>
    public static class ToastSetup
    {
        public const string MenuPath = "Tools/UI/Build Toast Prefab";
        public const string PrefabPath = "Assets/UI/Prefabs/Panels/Panel_Toast.prefab";
        private const string BannerPath = "Panels/ui_toast_banner.png";
        private const string FontPath = UIPrefabUtils.FontsRoot + "/PixelifySans_FontAsset.asset";
        // Above the shared HUD (10) and its windows, so a toast is never covered.
        private const int SortingOrder = 20;
        private const float FontSize = 32f;
        // Figma auto layout padding: 68 px on the sides and 24 px at the bottom (the banner tails).
        private const float SidePadding = 68f;
        private const float BottomPadding = 24f;

        private static readonly Vector2 Size = new Vector2(378f, 90f);
        // Top-left corner of the toast in the 1920x1080 frame (Figma "Garden - Harvested").
        private static readonly Vector2 FigmaPosition = new Vector2(803f, 150f);
        private static readonly Color TextColor = new Color32(0xFA, 0xED, 0xD1, 0xFF);

        [MenuItem(MenuPath)]
        public static void BuildPrefab()
        {
            var root = new GameObject("ToastCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            FixedFrameCanvas.ConfigureScaler(root.GetComponent<CanvasScaler>());
            var frame = FixedFrameCanvas.CreateFrame((RectTransform)root.transform);

            var banner = CreateBanner(frame);
            var text = CreateText(banner);
            var view = banner.gameObject.AddComponent<ToastView>();
            var so = new SerializedObject(view);
            so.FindProperty("messageText").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();

            UIPrefabUtils.SavePrefab(root, PrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log("[ToastSetup] Saved " + PrefabPath);
        }

        /// <summary>Places the toast in the open scene, building the prefab first if it doesn't exist yet.</summary>
        public static GameObject PlaceInScene()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                BuildPrefab();
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        private static RectTransform CreateBanner(RectTransform frame)
        {
            var banner = new GameObject("Toast", typeof(RectTransform), typeof(Image), typeof(CanvasGroup)).GetComponent<RectTransform>();
            banner.SetParent(frame, false);
            banner.anchorMin = banner.anchorMax = banner.pivot = new Vector2(0f, 1f);
            banner.anchoredPosition = new Vector2(FigmaPosition.x, -FigmaPosition.y);
            banner.sizeDelta = Size;

            var image = banner.GetComponent<Image>();
            image.sprite = UIPrefabUtils.LoadSprite(BannerPath);
            image.raycastTarget = false;
            if (image.sprite == null)
            {
                Debug.LogWarning("[ToastSetup] Sprite not found: " + BannerPath);
            }
            return banner;
        }

        private static TMP_Text CreateText(RectTransform banner)
        {
            var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            var rect = text.rectTransform;
            rect.SetParent(banner, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(SidePadding, BottomPadding);
            rect.offsetMax = new Vector2(-SidePadding, 0f);

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font != null)
            {
                text.font = font;
            }
            text.text = "+1 Lavender";
            text.fontSize = FontSize;
            text.fontStyle = FontStyles.Bold;
            text.color = TextColor;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }
    }
}
