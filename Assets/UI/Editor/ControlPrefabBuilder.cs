using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SproutOfDebt.UI.Editor.UIPrefabUtils;

namespace SproutOfDebt.UI.Editor
{
    // Builds control prefabs: inventory slot, slider, scrollbar (Prefabs/Controls).
    internal static class ControlPrefabBuilder
    {
        internal static void BuildControls(TMP_FontAsset font)
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
    }
}
