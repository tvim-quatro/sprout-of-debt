using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DrugCrafter.UI.Editor
{
    [InitializeOnLoad]
    public static class UIPrefabBuilder
    {
        private const string ArtRoot = "Assets/UI/Art";
        private const string PrefabsRoot = "Assets/UI/Prefabs";
        private const string FontsRoot = "Assets/UI/Fonts";

        static UIPrefabBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                // Run once if prefabs don't exist yet
                if (!File.Exists(Path.Combine(PrefabsRoot, "Buttons/Button_Primary.prefab")))
                {
                    BuildAllPrefabs();
                }
            };
        }

        [MenuItem("Tools/UI/Generate All UI Prefabs")]
        public static void BuildAllPrefabs()
        {
            EnsureDirectories();
            var fontAsset = EnsureFontAsset();

            BuildButtons(fontAsset);
            BuildPanels(fontAsset);
            BuildControls(fontAsset);
            BuildBars(fontAsset);
            BuildSampleCanvas(fontAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UIPrefabBuilder] Successfully generated all UI Prefabs in " + PrefabsRoot);
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Buttons"));
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Panels"));
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Controls"));
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Bars"));
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Canvases"));
        }

        public static TMP_FontAsset EnsureFontAsset()
        {
            // Use TMP default font asset (LiberationSans SDF) or any existing valid font asset
            // To create PixelifySans, use Unity Editor's Window > TextMeshPro > Font Asset Creator
            string fontAssetPath = Path.Combine(FontsRoot, "PixelifySans_FontAsset.asset").Replace("\\", "/");
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
            if (fontAsset != null && fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
            {
                return fontAsset;
            }

            // Fallback to default TMP font asset so text never crashes
            string defaultPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            var defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(defaultPath);
            if (defaultFont != null)
            {
                return defaultFont;
            }

            return null;
        }

        private static Sprite LoadSprite(string relativePath)
        {
            string fullPath = Path.Combine(ArtRoot, relativePath).Replace("\\", "/");
            return AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
        }

        #region Buttons
        private static void BuildButtons(TMP_FontAsset font)
        {
            // 1. Button Primary
            BuildLabeledButton("Button_Primary",
                LoadSprite("Buttons/ui_btn_primary_normal.png"),
                LoadSprite("Buttons/ui_btn_primary_hover.png"),
                LoadSprite("Buttons/ui_btn_primary_pressed.png"),
                LoadSprite("Buttons/ui_btn_primary_disabled.png"),
                new Vector2(180, 48), "BUTTON", new Color(0.98f, 0.93f, 0.82f), font);

            // 2. Button Secondary
            BuildLabeledButton("Button_Secondary",
                LoadSprite("Buttons/ui_btn_secondary_normal.png"),
                LoadSprite("Buttons/ui_btn_secondary_hover.png"),
                LoadSprite("Buttons/ui_btn_secondary_pressed.png"),
                LoadSprite("Buttons/ui_btn_secondary_disabled.png"),
                new Vector2(180, 48), "BUTTON", new Color(0.24f, 0.15f, 0.09f), font);

            // 3. Button Close
            BuildSimpleButton("Button_Close",
                LoadSprite("Buttons/ui_btn_close_normal.png"),
                LoadSprite("Buttons/ui_btn_close_hover.png"),
                LoadSprite("Buttons/ui_btn_close_pressed.png"),
                null,
                new Vector2(36, 36));

            // 4. Button Round
            BuildSimpleButton("Button_Round",
                LoadSprite("Buttons/ui_btn_round_normal.png"),
                LoadSprite("Buttons/ui_btn_round_hover.png"),
                LoadSprite("Buttons/ui_btn_round_pressed.png"),
                null,
                new Vector2(66, 66));

            // 5. Button Nav
            BuildLabeledButton("Button_Nav",
                LoadSprite("Buttons/ui_btn_nav_normal.png"),
                LoadSprite("Buttons/ui_btn_nav_hover.png"),
                LoadSprite("Buttons/ui_btn_nav_active.png"),
                LoadSprite("Buttons/ui_btn_nav_locked.png"),
                new Vector2(120, 40), "GARDEN", new Color(0.98f, 0.93f, 0.82f), font);

            // 6. Button Tab
            BuildLabeledButton("Button_Tab",
                LoadSprite("Buttons/ui_btn_tab_active.png"),
                LoadSprite("Buttons/ui_btn_tab_active.png"),
                LoadSprite("Buttons/ui_btn_tab_inactive.png"),
                LoadSprite("Buttons/ui_btn_tab_inactive.png"),
                new Vector2(120, 36), "SEEDS", new Color(0.24f, 0.15f, 0.09f), font);

            // 7. Button Icon
            BuildSimpleButton("Button_Icon",
                LoadSprite("Buttons/ui_btn_icon_normal.png"),
                LoadSprite("Buttons/ui_btn_icon_hover.png"),
                LoadSprite("Buttons/ui_btn_icon_pressed.png"),
                LoadSprite("Buttons/ui_btn_icon_disabled.png"),
                new Vector2(48, 48));
        }

        private static void BuildLabeledButton(string name, Sprite normal, Sprite hover, Sprite pressed, Sprite disabled,
            Vector2 size, string defaultText, Color textColor, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.sprite = normal;
            img.type = (normal != null && normal.border != Vector4.zero) ? Image.Type.Sliced : Image.Type.Simple;

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.SpriteSwap;
            var ss = btn.spriteState;
            ss.highlightedSprite = hover;
            ss.pressedSprite = pressed;
            ss.disabledSprite = disabled;
            btn.spriteState = ss;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.fontSize = 20;
            if (font != null) tmp.font = font;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Buttons/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void BuildSimpleButton(string name, Sprite normal, Sprite hover, Sprite pressed, Sprite disabled, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.sprite = normal;
            img.type = (normal != null && normal.border != Vector4.zero) ? Image.Type.Sliced : Image.Type.Simple;

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.SpriteSwap;
            var ss = btn.spriteState;
            ss.highlightedSprite = hover;
            ss.pressedSprite = pressed;
            ss.disabledSprite = disabled;
            btn.spriteState = ss;

            // Optional child icon container
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = size * 0.65f;
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = new Color(1, 1, 1, 0); // invisible by default until assigned

            SavePrefab(go, Path.Combine(PrefabsRoot, "Buttons/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }
        #endregion

        #region Panels
        private static void BuildPanels(TMP_FontAsset font)
        {
            // 1. Panel Window
            BuildWindowPanel("Panel_Window", LoadSprite("Panels/ui_panel_window_9s.png"),
                LoadSprite("Panels/ui_panel_header_9s.png"), font);

            // 2. Panel Small
            BuildSimplePanel("Panel_Small", LoadSprite("Panels/ui_panel_small_9s.png"), new Vector2(123, 126));

            // 3. Panel Dialog Box
            BuildDialogPanel("Panel_DialogBox", LoadSprite("Panels/ui_dialog_box_9s.png"),
                LoadSprite("Panels/ui_dialog_nameplate.png"), font);

            // 4. Panel Tooltip
            BuildTooltipPanel("Panel_Tooltip", LoadSprite("Panels/ui_tooltip_9s.png"), font);

            // 5. Panel Price Tag
            BuildPriceTagPanel("Panel_PriceTag", LoadSprite("Panels/ui_price_tag.png"),
                LoadSprite("Icons/ui_icon_coin.png"), font);

            // 6. Panel Portrait Frame
            BuildSimplePanel("Panel_PortraitFrame", LoadSprite("Panels/ui_portrait_frame.png"), new Vector2(90, 90));
        }

        private static void BuildWindowPanel(string name, Sprite bgSprite, Sprite headerSprite, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 380);

            var img = go.GetComponent<Image>();
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;

            // Header
            var headerGo = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            headerGo.transform.SetParent(go.transform, false);
            var headerRt = headerGo.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.sizeDelta = new Vector2(0, 50);
            headerRt.anchoredPosition = new Vector2(0, 0);

            var headerImg = headerGo.GetComponent<Image>();
            headerImg.sprite = headerSprite;
            headerImg.type = Image.Type.Sliced;

            // Title
            var titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = new Vector2(-50, 0);

            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "WINDOW TITLE";
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            titleTmp.color = new Color(0.98f, 0.93f, 0.82f);
            titleTmp.fontSize = 22;
            if (font != null) titleTmp.font = font;

            // Content Area
            var contentGo = new GameObject("ContentArea", typeof(RectTransform));
            contentGo.transform.SetParent(go.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = new Vector2(25, 25);
            contentRt.offsetMax = new Vector2(-25, -60);

            SavePrefab(go, Path.Combine(PrefabsRoot, "Panels/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void BuildSimplePanel(string name, Sprite sprite, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = (sprite != null && sprite.border != Vector4.zero) ? Image.Type.Sliced : Image.Type.Simple;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Panels/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void BuildDialogPanel(string name, Sprite boxSprite, Sprite nameplateSprite, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(700, 160);

            var img = go.GetComponent<Image>();
            img.sprite = boxSprite;
            img.type = Image.Type.Sliced;

            // Nameplate
            var nameplateGo = new GameObject("Nameplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            nameplateGo.transform.SetParent(go.transform, false);
            var nameRt = nameplateGo.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 1);
            nameRt.anchorMax = new Vector2(0, 1);
            nameRt.pivot = new Vector2(0, 0);
            nameRt.anchoredPosition = new Vector2(20, -5);
            nameRt.sizeDelta = new Vector2(160, 40);

            var nameImg = nameplateGo.GetComponent<Image>();
            nameImg.sprite = nameplateSprite;

            var nameTextGo = new GameObject("SpeakerName", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameTextGo.transform.SetParent(nameplateGo.transform, false);
            var ntRt = nameTextGo.GetComponent<RectTransform>();
            ntRt.anchorMin = Vector2.zero;
            ntRt.anchorMax = Vector2.one;
            ntRt.offsetMin = Vector2.zero;
            ntRt.offsetMax = Vector2.zero;

            var ntTmp = nameTextGo.GetComponent<TextMeshProUGUI>();
            ntTmp.text = "SPEAKER";
            ntTmp.alignment = TextAlignmentOptions.Center;
            ntTmp.color = new Color(0.98f, 0.93f, 0.82f);
            ntTmp.fontSize = 18;
            if (font != null) ntTmp.font = font;

            // Dialogue Text
            var dialogTextGo = new GameObject("DialogueText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            dialogTextGo.transform.SetParent(go.transform, false);
            var dtRt = dialogTextGo.GetComponent<RectTransform>();
            dtRt.anchorMin = Vector2.zero;
            dtRt.anchorMax = Vector2.one;
            dtRt.offsetMin = new Vector2(25, 20);
            dtRt.offsetMax = new Vector2(-25, -20);

            var dtTmp = dialogTextGo.GetComponent<TextMeshProUGUI>();
            dtTmp.text = "Hello! I am here to discuss your debt...";
            dtTmp.alignment = TextAlignmentOptions.TopLeft;
            dtTmp.color = new Color(0.24f, 0.15f, 0.09f);
            dtTmp.fontSize = 20;
            if (font != null) dtTmp.font = font;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Panels/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void BuildTooltipPanel(string name, Sprite sprite, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(267, 108);

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;

            var textGo = new GameObject("TooltipText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(15, 15);
            textRt.offsetMax = new Vector2(-15, -15);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "Tooltip description goes here.";
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = new Color(0.24f, 0.15f, 0.09f);
            tmp.fontSize = 18;
            if (font != null) tmp.font = font;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Panels/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void BuildPriceTagPanel(string name, Sprite plateSprite, Sprite coinSprite, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(110, 40);

            var img = go.GetComponent<Image>();
            img.sprite = plateSprite;

            var coinGo = new GameObject("CoinIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            coinGo.transform.SetParent(go.transform, false);
            var coinRt = coinGo.GetComponent<RectTransform>();
            coinRt.anchorMin = new Vector2(0, 0.5f);
            coinRt.anchorMax = new Vector2(0, 0.5f);
            coinRt.pivot = new Vector2(0, 0.5f);
            coinRt.anchoredPosition = new Vector2(8, 0);
            coinRt.sizeDelta = new Vector2(24, 24);
            var coinImg = coinGo.GetComponent<Image>();
            coinImg.sprite = coinSprite;

            var textGo = new GameObject("PriceText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.offsetMin = new Vector2(36, 0);
            textRt.offsetMax = new Vector2(-8, 0);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "10";
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = new Color(0.98f, 0.93f, 0.82f);
            tmp.fontSize = 18;
            if (font != null) tmp.font = font;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Panels/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }
        #endregion

        #region Controls
        private static void BuildControls(TMP_FontAsset font)
        {
            // 1. Inventory Slot
            BuildInventorySlot("Control_InventorySlot",
                LoadSprite("Controls/ui_slot_normal.png"),
                LoadSprite("Controls/ui_slot_hover.png"),
                LoadSprite("Controls/ui_slot_selected.png"),
                LoadSprite("Controls/ui_badge_count.png"), font);

            // 2. Slider
            BuildSliderControl("Control_Slider",
                LoadSprite("Controls/ui_slider_track_9s.png"),
                LoadSprite("Controls/ui_slider_fill.png"),
                LoadSprite("Controls/ui_slider_knob_normal.png"),
                LoadSprite("Controls/ui_slider_knob_hover.png"),
                LoadSprite("Controls/ui_slider_knob_pressed.png"));

            // 3. Scrollbar
            BuildScrollbarControl("Control_Scrollbar",
                LoadSprite("Controls/ui_scrollbar_track_9s.png"),
                LoadSprite("Controls/ui_scrollbar_thumb_normal.png"),
                LoadSprite("Controls/ui_scrollbar_thumb_hover.png"));
        }

        private static void BuildInventorySlot(string name, Sprite normal, Sprite hover, Sprite selected, Sprite badgeSprite, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(58, 56);

            var img = go.GetComponent<Image>();
            img.sprite = normal;

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.SpriteSwap;
            var ss = btn.spriteState;
            ss.highlightedSprite = hover;
            ss.selectedSprite = selected;
            btn.spriteState = ss;

            // Item Icon
            var iconGo = new GameObject("ItemIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = new Vector2(40, 40);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = new Color(1, 1, 1, 0);

            // Badge Count
            var badgeGo = new GameObject("BadgeCount", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            badgeGo.transform.SetParent(go.transform, false);
            var badgeRt = badgeGo.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(1, 0);
            badgeRt.anchorMax = new Vector2(1, 0);
            badgeRt.pivot = new Vector2(1, 0);
            badgeRt.anchoredPosition = new Vector2(0, 0);
            badgeRt.sizeDelta = new Vector2(24, 20);

            var badgeImg = badgeGo.GetComponent<Image>();
            badgeImg.sprite = badgeSprite;

            var countTextGo = new GameObject("CountText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            countTextGo.transform.SetParent(badgeGo.transform, false);
            var ctRt = countTextGo.GetComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.offsetMin = Vector2.zero;
            ctRt.offsetMax = Vector2.zero;

            var ctTmp = countTextGo.GetComponent<TextMeshProUGUI>();
            ctTmp.text = "1";
            ctTmp.alignment = TextAlignmentOptions.Center;
            ctTmp.color = new Color(0.98f, 0.93f, 0.82f);
            ctTmp.fontSize = 14;
            if (font != null) ctTmp.font = font;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Controls/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void BuildSliderControl(string name, Sprite track, Sprite fill, Sprite knobNormal, Sprite knobHover, Sprite knobPressed)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(309, 36);

            var slider = go.GetComponent<Slider>();

            // Background Track
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.25f);
            bgRt.anchorMax = new Vector2(1, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = track;
            bgImg.type = Image.Type.Sliced;

            // Fill Area
            var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGo.transform.SetParent(go.transform, false);
            var faRt = fillAreaGo.GetComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.25f);
            faRt.anchorMax = new Vector2(1, 0.75f);
            faRt.offsetMin = new Vector2(10, 0);
            faRt.offsetMax = new Vector2(-10, 0);

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;

            var fillImg = fillGo.GetComponent<Image>();
            fillImg.sprite = fill;
            fillImg.type = Image.Type.Tiled;

            // Handle Slide Area
            var handleAreaGo = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaGo.transform.SetParent(go.transform, false);
            var haRt = handleAreaGo.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.offsetMin = new Vector2(15, 0);
            haRt.offsetMax = new Vector2(-15, 0);

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handleGo.transform.SetParent(handleAreaGo.transform, false);
            var handleRt = handleGo.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(30, 36);

            var handleImg = handleGo.GetComponent<Image>();
            handleImg.sprite = knobNormal;

            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.75f;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Controls/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void BuildScrollbarControl(string name, Sprite track, Sprite thumbNormal, Sprite thumbHover)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Scrollbar));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(24, 165);

            var scrollbar = go.GetComponent<Scrollbar>();

            // Background
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = track;
            bgImg.type = Image.Type.Sliced;

            // Sliding Area
            var slideAreaGo = new GameObject("Sliding Area", typeof(RectTransform));
            slideAreaGo.transform.SetParent(go.transform, false);
            var saRt = slideAreaGo.GetComponent<RectTransform>();
            saRt.anchorMin = Vector2.zero;
            saRt.anchorMax = Vector2.one;
            saRt.offsetMin = new Vector2(5, 5);
            saRt.offsetMax = new Vector2(-5, -5);

            // Handle
            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handleGo.transform.SetParent(slideAreaGo.transform, false);
            var handleRt = handleGo.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(0, 40);

            var handleImg = handleGo.GetComponent<Image>();
            handleImg.sprite = thumbNormal;

            scrollbar.handleRect = handleRt;
            scrollbar.targetGraphic = handleImg;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Controls/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }
        #endregion

        #region Bars
        private static void BuildBars(TMP_FontAsset font)
        {
            var go = new GameObject("Control_DebtBar", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(282, 40);

            // Frame
            var frameGo = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameGo.transform.SetParent(go.transform, false);
            var frameRt = frameGo.GetComponent<RectTransform>();
            frameRt.anchorMin = Vector2.zero;
            frameRt.anchorMax = Vector2.one;
            frameRt.offsetMin = Vector2.zero;
            frameRt.offsetMax = Vector2.zero;
            var frameImg = frameGo.GetComponent<Image>();
            frameImg.sprite = LoadSprite("Panels/ui_bar_frame_9s.png");
            frameImg.type = Image.Type.Sliced;

            // Fill
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(go.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0, 0.15f);
            fillRt.anchorMax = new Vector2(0.7f, 0.85f);
            fillRt.offsetMin = new Vector2(6, 0);
            fillRt.offsetMax = new Vector2(-6, 0);
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.sprite = LoadSprite("Bars/ui_bar_fill.png");
            fillImg.type = Image.Type.Sliced;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Bars/Control_DebtBar.prefab"));
            Object.DestroyImmediate(go);
        }
        #endregion

        #region Sample Canvas
        private static void BuildSampleCanvas(TMP_FontAsset font)
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
        #endregion

        private static void SavePrefab(GameObject go, string localPath)
        {
            string unityPath = localPath.Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAsset(go, unityPath);
        }
    }
}
