using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.UI.Editor
{
    /// <summary>
    /// Builds Panel_Toast from the Figma component "Toast": its own overlay canvas with the 16:9 frame and the banner
    /// (ui_toast_banner) with a bold 32 px cream line, centered at the height shown in "Garden - Harvested".
    /// Like the Figma auto layout, the banner hugs the text with the same padding on both sides and never gets narrower
    /// than the Figma size: only its flat middle stretches, the curved ends and tails keep their shape.
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

        // Figma "Toast" size: the smallest the banner gets, and its fixed height.
        private static readonly Vector2 MinSize = new Vector2(378f, 90f);
        // Top edge of the toast in the 1920x1080 frame (Figma "Garden - Harvested"); centered horizontally.
        private const float TopY = 150f;
        // ui_toast_banner is 315 px wide; columns 93..221 are the flat middle of the ribbon, so 93 px on each side
        // (the curved ends and tails) keep their shape when the banner gets wider.
        private const float BannerSideBorder = 93f;
        private const float BannerSpriteWidth = 315f;
        private static readonly Color TextColor = new Color32(0xFA, 0xED, 0xD1, 0xFF);

        [MenuItem(MenuPath)]
        public static void BuildPrefab()
        {
            EnsureBannerBorders();
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

        // The sprite borders live in the texture's import settings; set them here so a rebuild never depends on a manual step.
        private static void EnsureBannerBorders()
        {
            var path = UIPrefabUtils.ArtRoot + "/" + BannerPath;
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                return;
            }
            var border = new Vector4(BannerSideBorder, 0f, BannerSideBorder, 0f);
            if (importer.spriteBorder != border)
            {
                importer.spriteBorder = border;
                importer.SaveAndReimport();
            }
        }

        private static RectTransform CreateBanner(RectTransform frame)
        {
            var banner = new GameObject("Toast", typeof(RectTransform), typeof(Image), typeof(CanvasGroup),
                typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            banner.SetParent(frame, false);
            banner.anchorMin = banner.anchorMax = banner.pivot = new Vector2(0.5f, 1f);
            banner.anchoredPosition = new Vector2(0f, -TopY);
            banner.sizeDelta = MinSize;

            var image = banner.GetComponent<Image>();
            image.sprite = UIPrefabUtils.LoadSprite(BannerPath);
            image.type = Image.Type.Sliced;
            // The banner is drawn at the Figma size (378 px for the 315 px sprite), so the ends scale the same way.
            image.pixelsPerUnitMultiplier = BannerSpriteWidth / MinSize.x;
            image.raycastTarget = false;
            if (image.sprite == null)
            {
                Debug.LogWarning("[ToastSetup] Sprite not found: " + BannerPath);
            }

            // Hug the text: padding + text width, at least the Figma width; the height stays fixed.
            var layout = banner.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset((int)SidePadding, (int)SidePadding, 0, (int)BottomPadding);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            banner.GetComponent<LayoutElement>().minWidth = MinSize.x;
            banner.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return banner;
        }

        private static TMP_Text CreateText(RectTransform banner)
        {
            var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            var rect = text.rectTransform;
            rect.SetParent(banner, false);

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
