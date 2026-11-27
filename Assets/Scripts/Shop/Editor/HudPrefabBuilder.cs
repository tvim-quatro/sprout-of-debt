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
    /// top bar (debt bar, coins, inventory button), bottom location switcher and windows (Treasury, Seed Shop, Inventory).
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
        private const float MarkerStripHeight = 25f;
        private static readonly Vector2 CoinsPosition = new Vector2(500, 18);
        // Figma "HUD counter": ui_panel_small_9s, 90 px high, hugging a 40 px coin and the 26 px bold amount (gap 10, sides 28).
        private const float CoinsHeight = 90f;
        private const float CoinIconSize = 40f;
        private static readonly Vector2 InventoryButtonOffset = new Vector2(-147, -24);
        private static readonly Vector2 ExitButtonOffset = new Vector2(-32, -24);
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
            var seedShop = SeedShopWindowBuilder.Build(windows, windowManager, config);
            var treasury = ShopWindowsBuilder.BuildTreasuryWindow(windows, windowManager);
            var inventory = InventoryWindowBuilder.BuildInventoryWindow(windows, windowManager, seedShop);
            var inventoryButton = BuildTopBar(topBar, windowManager, treasury, seedShop, inventory);
            windows.SetAsLastSibling();

            var tutorialOverlay = SproutOfDebt.Tutorial.Editor.TutorialOverlayBuilder.BuildTutorialOverlay(frame);
            tutorialOverlay.SetAsLastSibling();
            // On top of everything: items fly over open windows and the onboarding dimmer to the bag.
            InventoryFlyBuilder.Build(frame, inventoryButton);

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

        private static RectTransform BuildTopBar(RectTransform topBar, WindowManager windowManager, UIWindow treasury, UIWindow seedShop, UIWindow inventory)
        {
            var hud = topBar.gameObject.AddComponent<ShopHud>();

            var bar = BuildDebtBar(topBar);
            var barButton = bar.gameObject.AddComponent<Button>();
            barButton.navigation = new Navigation { mode = Navigation.Mode.None };
            barButton.targetGraphic = bar.Find("Frame").GetComponent<Image>();
            // Figma "HUD / Debt bar" hover: a light outline around the frame, no tint.
            barButton.transition = Selectable.Transition.None;
            var barStyle = bar.gameObject.AddComponent<SproutOfDebt.UI.ButtonStateStyle>();
            Set(barStyle, "hoverObject", BuildDebtBarHoverOutline(bar));

            var coins = BuildCoins(topBar, out var coinsText);
            var counter = coins.gameObject.AddComponent<CoinCounterView>();
            Set(counter, "amountText", coinsText);
            Set(counter, "punchTarget", coins);

            var inventoryButton = BuildTopRightIconButton(topBar, "Button_Inventory", InventoryButtonOffset, UIArtRoot + "/Icons/ui_icon_inventory.png");
            var exitButton = BuildTopRightIconButton(topBar, "Button_Exit", ExitButtonOffset, UIArtRoot + "/Icons/ui_icon_exit.png");

            Set(hud, "windowManager", windowManager);
            Set(hud, "debtBarButton", barButton);
            Set(hud, "inventoryButton", inventoryButton);
            Set(hud, "exitButton", exitButton);
            Set(hud, "treasuryWindow", treasury);
            Set(hud, "seedShopWindow", seedShop);
            Set(hud, "inventoryWindow", inventory);
            return (RectTransform)inventoryButton.transform;
        }

        // Figma "HUD / Debt bar": milestone markers on the top edge, frame below them, caption under the frame.
        // Figma "Hover outline": 3 px #FFE08A stroke, 6 px outside the frame on every side.
        private static GameObject BuildDebtBarHoverOutline(RectTransform bar)
        {
            var go = new GameObject("HoverOutline", typeof(RectTransform), typeof(CanvasRenderer), typeof(SproutOfDebt.UI.RectOutlineGraphic));
            go.transform.SetParent(bar, false);
            go.transform.SetAsFirstSibling();
            var outline = go.GetComponent<SproutOfDebt.UI.RectOutlineGraphic>();
            outline.color = new Color32(0xFF, 0xE0, 0x8A, 0xFF);
            outline.raycastTarget = false;
            Place((RectTransform)go.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(-6, -(DebtFrameTop - 6)),
                new Vector2(DebtBarSize.x + 12, DebtFrameHeight + 12));
            go.SetActive(false);
            return go;
        }

        // The plate hugs its content like the Figma auto layout, so it grows to the right with the amount.
        private static RectTransform BuildCoins(RectTransform parent, out TMP_Text amount)
        {
            var plate = MakeImage("Coins", parent, UIArtRoot + "/Panels/ui_panel_small_9s.png", false);
            plate.type = Image.Type.Sliced;
            var rt = plate.rectTransform;
            Place(rt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(CoinsPosition.x, -CoinsPosition.y), new Vector2(0, CoinsHeight));

            var layout = plate.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 0, 0);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            plate.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var coin = MakeImage("CoinIcon", rt, UIArtRoot + "/Icons/ui_icon_coin.png", false);
            coin.preserveAspect = true;
            var coinSize = coin.gameObject.AddComponent<LayoutElement>();
            coinSize.preferredWidth = coinSize.preferredHeight = CoinIconSize;

            amount = MakeText("Amount", rt, "0", 26, Cream, TextAlignmentOptions.Left);
            amount.fontStyle = FontStyles.Bold;
            amount.textWrappingMode = TextWrappingModes.NoWrap;
            return rt;
        }

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
            var fillRt = MakeBarFill(fillArea, DebtBarSize.x - DebtFillInset.x * 2);

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

        private static Button BuildTopRightIconButton(RectTransform parent, string name, Vector2 offset, string iconPath)
        {
            var button = MakeButton("Button_Icon", parent, name, null, IconButtonSize, 0);
            Place((RectTransform)button.transform, new Vector2(1, 1), new Vector2(1, 1), offset);

            var iconTransform = button.transform.Find("Icon");
            Image icon;
            if (iconTransform != null)
            {
                icon = iconTransform.GetComponent<Image>();
            }
            else
            {
                icon = MakeImage("Icon", button.transform, iconPath, false);
            }
            icon.sprite = LoadSprite(iconPath);
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.rectTransform.sizeDelta = new Vector2(54, 54);
            Place(icon.rectTransform, Center, Center, Vector2.zero);
            return button;
        }
    }
}
