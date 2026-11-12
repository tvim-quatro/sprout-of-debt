using System.IO;
using SproutOfDebt.Shop.UI;
using SproutOfDebt.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Shop.Editor.ShopUIFactory;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>
    /// Builds SOD_HUD_Canvas, the interface frame shared by every location (Figma: Navigation / HUD):
    /// top bar (debt bar, coins), bottom location switcher and the Treasury / Seed Shop windows.
    /// Location scenes only place an instance of this prefab, so a change here shows up on every screen.
    /// </summary>
    public static class HudPrefabBuilder
    {
        public const string PrefabPath = "Assets/UI/Prefabs/Canvases/SOD_HUD_Canvas.prefab";
        private const int CanvasSortingOrder = 10;

        // Figma "HUD / Top bar", in reference pixels from the top-left corner.
        private static readonly Vector2 DebtBarPosition = new Vector2(32, 10);
        private static readonly Vector2 DebtBarSize = new Vector2(440, 108);
        private const float DebtFrameTop = 14f;
        private const float DebtFrameHeight = 66f;
        private static readonly Vector2 DebtFillInset = new Vector2(14, 11);
        private const float MarkerStripHeight = 25f;
        private static readonly Vector2 CoinsPosition = new Vector2(500, 18);
        private static readonly Vector2 CoinsSize = new Vector2(149, 90);
        // Temporary Seed Shop button sits where the Inventory button will be (top-right group).
        private static readonly Vector2 SeedShopButtonOffset = new Vector2(-147, -24);
        private static readonly Vector2 IconButtonSize = new Vector2(99, 99);

        [MenuItem("Tools/Shop/Build HUD Prefab")]
        public static void BuildPrefab()
        {
            var config = ShopSceneBuilder.EnsureData();
            LocationNavBarBuilder.BuildPrefab();

            var root = CreateCanvas();
            root.gameObject.AddComponent<ShopBootstrap>();
            Set(root.GetComponent<ShopBootstrap>(), "config", config);
            // Everything sits in the 16:9 frame, so the HUD lines up with the location art on any screen.
            var frame = FixedFrameCanvas.CreateFrame(root);

            var topBar = MakeRect("TopBar", frame);
            Stretch(topBar);
            LocationNavBarBuilder.BuildNavBar(frame);

            var windows = ShopWindowsBuilder.BuildWindowsRoot(frame, out var windowManager);
            var seedShop = ShopWindowsBuilder.BuildSeedShopWindow(windows, windowManager, config);
            var treasury = ShopWindowsBuilder.BuildTreasuryWindow(windows, windowManager);
            BuildTopBar(topBar, windowManager, treasury, seedShop);
            windows.SetAsLastSibling();

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, PrefabPath);
            Object.DestroyImmediate(root.gameObject);
            Debug.Log("[HudPrefabBuilder] Saved " + PrefabPath);
        }

        /// <summary>Places the HUD prefab at the root of the open scene, building it first if it doesn't exist yet.</summary>
        public static GameObject InstantiateHud()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                BuildPrefab();
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        private static RectTransform CreateCanvas()
        {
            var go = new GameObject("SOD_HUD_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortingOrder;

            FixedFrameCanvas.ConfigureScaler(go.GetComponent<CanvasScaler>());
            return (RectTransform)go.transform;
        }

        private static void BuildTopBar(RectTransform topBar, WindowManager windowManager, UIWindow treasury, UIWindow seedShop)
        {
            var hud = topBar.gameObject.AddComponent<ShopHud>();

            var bar = BuildDebtBar(topBar);
            var barButton = bar.gameObject.AddComponent<Button>();
            barButton.targetGraphic = bar.Find("Frame").GetComponent<Image>();

            var coinsText = PriceTag("Coins", topBar, "0", CoinsSize);
            var coins = (RectTransform)coinsText.transform.parent.parent;
            Place(coins, new Vector2(0, 1), new Vector2(0, 1), new Vector2(CoinsPosition.x, -CoinsPosition.y));
            var counter = coins.gameObject.AddComponent<CoinCounterView>();
            Set(counter, "amountText", coinsText);
            Set(counter, "punchTarget", coins);

            var seedButton = BuildSeedShopButton(topBar);

            Set(hud, "windowManager", windowManager);
            Set(hud, "debtBarButton", barButton);
            Set(hud, "seedShopButton", seedButton);
            Set(hud, "treasuryWindow", treasury);
            Set(hud, "seedShopWindow", seedShop);
        }

        // Figma "HUD / Debt bar": milestone markers on the top edge, frame below them, caption under the frame.
        private static RectTransform BuildDebtBar(RectTransform parent)
        {
            var bar = MakeRect("DebtBar", parent);
            Place(bar, new Vector2(0, 1), new Vector2(0, 1), new Vector2(DebtBarPosition.x, -DebtBarPosition.y), DebtBarSize);
            var view = bar.gameObject.AddComponent<DebtBarView>();

            var frame = MakeImage("Frame", bar, UIArtRoot + "/Panels/ui_bar_frame_9s.png", false);
            frame.type = Image.Type.Sliced;
            frame.raycastTarget = true;
            Place(frame.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -DebtFrameTop), new Vector2(DebtBarSize.x, DebtFrameHeight));

            var fillArea = MakeRect("FillArea", frame.rectTransform);
            Stretch(fillArea, DebtFillInset.x, DebtFillInset.y);
            var fill = MakeImage("Fill", fillArea, UIArtRoot + "/Bars/ui_bar_fill.png", false);
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0.3f, 1);
            fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

            var markers = MakeRect("Markers", bar);
            Place(markers, new Vector2(0, 1), new Vector2(0, 1), new Vector2(DebtFillInset.x, 0),
                new Vector2(DebtBarSize.x - DebtFillInset.x * 2, MarkerStripHeight));

            var label = MakeText("Label", bar, "Debt paid 0 / 1000", 20, Cream, TextAlignmentOptions.Left);
            Place(label.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(4, -84), new Vector2(DebtBarSize.x, 24));

            Set(view, "fill", fillRt);
            Set(view, "markersRoot", markers);
            Set(view, "markerLocked", LoadSprite(UIArtRoot + "/Bars/ui_bar_milestone_locked.png"));
            Set(view, "markerReached", LoadSprite(UIArtRoot + "/Bars/ui_bar_milestone_reached.png"));
            Set(view, "label", label);
            var so = new SerializedObject(view);
            so.FindProperty("labelFormat").stringValue = "Debt paid {0} / {1}";
            so.ApplyModifiedPropertiesWithoutUndo();
            return bar;
        }

        private static Button BuildSeedShopButton(RectTransform parent)
        {
            var button = MakeButton("Button_Icon", parent, "Button_SeedShop", null, IconButtonSize, 0);
            Place((RectTransform)button.transform, new Vector2(1, 1), new Vector2(1, 1), SeedShopButtonOffset);
            var icon = button.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = LoadSprite(ArtRoot + "/Items/Seeds/icon_seed_lavender.png");
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.rectTransform.sizeDelta = new Vector2(52, 70);
            var label = MakeText("Label", button.transform, "Seeds", 22, Cream, TextAlignmentOptions.Top);
            Place(label.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0, -4), new Vector2(160, 30));
            return button;
        }
    }
}
