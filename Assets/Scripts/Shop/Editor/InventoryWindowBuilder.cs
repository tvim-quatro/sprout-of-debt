using System.Collections.Generic;
using System.IO;
using SproutOfDebt.Items;
using SproutOfDebt.Shop.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Shop.Editor.ShopUIFactory;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>
    /// Builds the Inventory window inside the HUD canvas according to Figma specs:
    /// 760x686 window docked at top-right (-49, -128), tabs (Seeds, Ingredients, Potions),
    /// 4-column slot grid (150x144 px slots), scrollbar, and "Buy seeds" footer.
    /// </summary>
    internal static class InventoryWindowBuilder
    {
        private const string DatabasePath = "Assets/Data/Inventory/InventoryDatabase.asset";
        private static readonly Vector2 WindowSize = new Vector2(760, 686);
        private static readonly Vector2 WindowOffset = new Vector2(-49, -128);

        internal static UIWindow BuildInventoryWindow(RectTransform parent, WindowManager manager, UIWindow seedShopWindow)
        {
            var database = EnsureDatabase();
            var rt = CreateWindowRoot("InventoryWindow", parent, out var close);
            var window = rt.gameObject.AddComponent<InventoryWindow>();
            Set(window, "windowManager", manager);
            Set(window, "closeButton", close);
            Set(window, "seedShopWindow", seedShopWindow);
            Set(window, "database", database);

            var content = MakeRect("Content", rt);
            Place(content, new Vector2(0, 1), new Vector2(0, 1), new Vector2(44, -100), new Vector2(672, 550));

            BuildTabs(content, window, out var seedsBtn, out var ingrBtn, out var potBtn,
                out var seedsBg, out var ingrBg, out var potBg,
                out var seedsText, out var ingrText, out var potText);

            BuildBody(content, window, out var slotsContainer, out var viewport, out var scrollbarRt, out var slotTemplate);
            BuildFooter(content, window, out var footerGo, out var buySeedsBtn);

            Set(window, "seedsTabButton", seedsBtn);
            Set(window, "ingredientsTabButton", ingrBtn);
            Set(window, "potionsTabButton", potBtn);
            Set(window, "seedsTabBg", seedsBg);
            Set(window, "ingredientsTabBg", ingrBg);
            Set(window, "potionsTabBg", potBg);
            Set(window, "seedsTabText", seedsText);
            Set(window, "ingredientsTabText", ingrText);
            Set(window, "potionsTabText", potText);
            Set(window, "tabActiveSprite", LoadSprite(UIArtRoot + "/Buttons/ui_btn_tab_active.png"));
            Set(window, "tabInactiveSprite", LoadSprite(UIArtRoot + "/Buttons/ui_btn_tab_inactive.png"));

            Set(window, "slotsContainer", slotsContainer);
            Set(window, "scrollViewport", viewport);
            Set(window, "scrollbarRect", scrollbarRt);
            Set(window, "slotTemplate", slotTemplate);

            Set(window, "footer", footerGo);
            Set(window, "buySeedsButton", buySeedsBtn);

            rt.gameObject.SetActive(false);
            return window;
        }

        private static RectTransform CreateWindowRoot(string name, RectTransform parent, out Button close)
        {
            var window = MakeImage(name, parent, UIArtRoot + "/Panels/ui_panel_window_9s.png", false);
            window.type = Image.Type.Sliced;
            var rt = window.rectTransform;
            Place(rt, new Vector2(1, 1), new Vector2(1, 1), WindowOffset, WindowSize);

            var header = MakeImage("Header", rt, UIArtRoot + "/Panels/ui_panel_header_9s.png", false);
            header.type = Image.Type.Sliced;
            Place(header.rectTransform, new Vector2(0.5f, 1), Center, new Vector2(0, 0), new Vector2(440, 90));
            var title = MakeText("Title", header.rectTransform, "Inventory", 32, Cream, TextAlignmentOptions.Center);
            Stretch(title.rectTransform);

            close = MakeButton("Button_Close", rt, "Button_Close", null, new Vector2(68, 70), 0);
            Place((RectTransform)close.transform, new Vector2(1, 1), Center, new Vector2(-36, -36));
            return rt;
        }

        private static void BuildTabs(RectTransform content, InventoryWindow window,
            out Button seedsBtn, out Button ingrBtn, out Button potBtn,
            out Image seedsBg, out Image ingrBg, out Image potBg,
            out TMP_Text seedsText, out TMP_Text ingrText, out TMP_Text potText)
        {
            var tabs = MakeRect("Tabs", content);
            Place(tabs, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(672, 56));
            var hlg = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = hlg.childControlHeight = false;
            hlg.childForceExpandWidth = hlg.childForceExpandHeight = false;

            seedsBtn = MakeTabButton("Tab_Seeds", tabs, "Seeds", new Vector2(184, 56), out seedsBg, out seedsText);
            ingrBtn = MakeTabButton("Tab_Ingredients", tabs, "Ingredients", new Vector2(263, 56), out ingrBg, out ingrText);
            potBtn = MakeTabButton("Tab_Potions", tabs, "Potions", new Vector2(208, 56), out potBg, out potText);
        }

        private static Button MakeTabButton(string name, RectTransform parent, string label, Vector2 size, out Image bg, out TMP_Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;

            bg = go.GetComponent<Image>();
            bg.sprite = LoadSprite(UIArtRoot + "/Buttons/ui_btn_tab_inactive.png");
            bg.type = Image.Type.Simple;
            bg.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;

            labelText = MakeText("Label", rt, label, 26, Cream, TextAlignmentOptions.Center);
            Stretch(labelText.rectTransform, 0, 6);
            return btn;
        }

        private static void BuildBody(RectTransform content, InventoryWindow window,
            out RectTransform slotsContainer, out RectTransform viewportRt, out RectTransform scrollbarRt, out InventorySlotView slotTemplate)
        {
            var body = MakeRect("Body", content);
            Place(body, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -72), new Vector2(672, 456));

            var viewport = MakeRect("Viewport", body);
            Place(viewport, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(636, 456));
            viewport.gameObject.AddComponent<RectMask2D>();
            viewportRt = viewport;

            slotsContainer = MakeRect("Slots", viewport);
            slotsContainer.anchorMin = new Vector2(0, 1);
            slotsContainer.anchorMax = new Vector2(1, 1);
            slotsContainer.pivot = new Vector2(0.5f, 1);
            slotsContainer.sizeDelta = new Vector2(0, 0);
            slotsContainer.anchoredPosition = Vector2.zero;

            var grid = slotsContainer.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150, 144);
            grid.spacing = new Vector2(12, 12);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;

            var fitter = slotsContainer.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollbarGo = BuildScrollbar(body, new Vector2(24, 456), new Vector2(648, 0));
            scrollbarRt = (RectTransform)scrollbarGo.transform;

            var scrollRect = body.gameObject.AddComponent<ScrollRect>();
            scrollRect.content = slotsContainer;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.verticalScrollbar = scrollbarGo.GetComponent<Scrollbar>();
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            slotTemplate = BuildSlotTemplate(slotsContainer);
        }

        private static GameObject BuildScrollbar(RectTransform parent, Vector2 size, Vector2 position)
        {
            var go = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            Place(rt, new Vector2(0, 1), new Vector2(0, 1), position, size);

            var trackImg = go.GetComponent<Image>();
            trackImg.sprite = LoadSprite(UIArtRoot + "/Controls/ui_scrollbar_track_9s.png");
            trackImg.type = Image.Type.Sliced;

            var slidingArea = MakeRect("Sliding Area", rt);
            Stretch(slidingArea, 3, 4);

            var handle = MakeImage("Handle", slidingArea, UIArtRoot + "/Controls/ui_scrollbar_thumb_normal.png", false);
            handle.type = Image.Type.Simple;
            var handleRt = handle.rectTransform;
            handleRt.sizeDelta = new Vector2(18, 90);

            var scrollbar = go.GetComponent<Scrollbar>();
            scrollbar.targetGraphic = handle;
            scrollbar.handleRect = handleRt;
            scrollbar.direction = Scrollbar.Direction.TopToBottom;
            return go;
        }

        private static InventorySlotView BuildSlotTemplate(RectTransform parent)
        {
            var go = new GameObject("Slot_Template", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(InventorySlotView));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(150, 144);

            var bg = go.GetComponent<Image>();
            bg.sprite = LoadSprite(UIArtRoot + "/Controls/ui_slot_normal.png");
            bg.type = Image.Type.Simple;
            bg.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.SpriteSwap;
            var ss = btn.spriteState;
            ss.highlightedSprite = LoadSprite(UIArtRoot + "/Controls/ui_slot_hover.png");
            ss.selectedSprite = LoadSprite(UIArtRoot + "/Controls/ui_slot_selected.png");
            btn.spriteState = ss;

            var iconGo = MakeImage("ItemIcon", rt, UIArtRoot + "/Icons/ui_icon_coin.png", false);
            var iconRt = iconGo.rectTransform;
            Place(iconRt, Center, Center, Vector2.zero, new Vector2(84, 84));
            iconGo.preserveAspect = true;

            var badge = MakeImage("BadgeCount", rt, UIArtRoot + "/Controls/ui_badge_count.png", false);
            badge.type = Image.Type.Simple;
            var badgeRt = badge.rectTransform;
            Place(badgeRt, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-12, 14), new Vector2(58, 32));

            var countText = MakeText("CountText", badgeRt, "1", 18, Cream, TextAlignmentOptions.Center);
            Stretch(countText.rectTransform);

            var view = go.GetComponent<InventorySlotView>();
            Set(view, "slotBackground", bg);
            Set(view, "itemIcon", iconGo);
            Set(view, "countBadge", badge.gameObject);
            Set(view, "countText", countText);
            return view;
        }

        private static void BuildFooter(RectTransform content, InventoryWindow window, out GameObject footerGo, out Button buySeedsBtn)
        {
            var footer = MakeRect("Footer", content);
            Place(footer, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -440), new Vector2(672, 87));
            footerGo = footer.gameObject;

            buySeedsBtn = MakeButton("Button_Primary", footer, "Button_BuySeeds", "Buy seeds", new Vector2(432, 87), 26);
            Place((RectTransform)buySeedsBtn.transform, Center, Center, Vector2.zero);
        }

        internal static InventoryDatabase EnsureDatabase()
        {
            var db = AssetDatabase.LoadAssetAtPath<InventoryDatabase>(DatabasePath);
            if (db == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath));
                db = ScriptableObject.CreateInstance<InventoryDatabase>();
                AssetDatabase.CreateAsset(db, DatabasePath);
            }

            var seeds = LoadAssets<SeedData>("Assets/Data/Shop/Seeds");
            var ingredients = LoadAssets<IngredientData>("Assets/Data/Ingredients");
            var potions = LoadAssets<PotionDefinition>("Assets/Data/Potions");

            SetArray(db, "seeds", seeds);
            SetArray(db, "ingredients", ingredients);
            SetArray(db, "potions", potions);

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            return db;
        }

        private static T[] LoadAssets<T>(string folder) where T : Object
        {
            var list = new List<T>();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) list.Add(asset);
            }
            return list.ToArray();
        }
    }
}
