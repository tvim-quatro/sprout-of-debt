using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SproutOfDebt.UI.Editor.UIPrefabUtils;

namespace SproutOfDebt.UI.Editor
{
    // Builds button prefabs (Prefabs/Buttons).
    internal static class ButtonPrefabBuilder
    {
        internal static void BuildButtons(TMP_FontAsset font)
        {
            // 1. Button Primary
            BuildLabeledButton("Button_Primary",
                LoadSprite("Buttons/ui_btn_primary_normal.png"),
                LoadSprite("Buttons/ui_btn_primary_hover.png"),
                LoadSprite("Buttons/ui_btn_primary_pressed.png"),
                LoadSprite("Buttons/ui_btn_primary_disabled.png"),
                new Vector2(180, 48), "BUTTON", new Color(0.98f, 0.93f, 0.82f), font, true);

            // 2. Button Secondary
            BuildLabeledButton("Button_Secondary",
                LoadSprite("Buttons/ui_btn_secondary_normal.png"),
                LoadSprite("Buttons/ui_btn_secondary_hover.png"),
                LoadSprite("Buttons/ui_btn_secondary_pressed.png"),
                LoadSprite("Buttons/ui_btn_secondary_disabled.png"),
                new Vector2(180, 48), "BUTTON", new Color(0.24f, 0.15f, 0.09f), font, true);

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

            // 6. Button Tab (Figma "Tab": inactive, hover; the active tab is set by the window that owns the tabs)
            BuildLabeledButton("Button_Tab",
                LoadSprite("Buttons/ui_btn_tab_inactive.png"),
                LoadSprite("Buttons/ui_btn_tab_hover.png"),
                LoadSprite("Buttons/ui_btn_tab_hover.png"),
                LoadSprite("Buttons/ui_btn_tab_inactive.png"),
                new Vector2(120, 36), "SEEDS", new Color(0.98f, 0.93f, 0.82f), font);

            // 7. Button Icon
            BuildSimpleButton("Button_Icon",
                LoadSprite("Buttons/ui_btn_icon_normal.png"),
                LoadSprite("Buttons/ui_btn_icon_hover.png"),
                LoadSprite("Buttons/ui_btn_icon_pressed.png"),
                LoadSprite("Buttons/ui_btn_icon_disabled.png"),
                new Vector2(48, 48), true);
        }

        private static void BuildLabeledButton(string name, Sprite normal, Sprite hover, Sprite pressed, Sprite disabled,
            Vector2 size, string defaultText, Color textColor, TMP_FontAsset font, bool pressAndDisabledStyle = false)
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
            // Mouse-only UI: a clicked button must not stay "Selected", which hides its hover state.
            btn.navigation = new Navigation { mode = Navigation.Mode.None };

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

            // Figma "Button": pressed moves the label 6 px down, disabled turns it grey.
            if (pressAndDisabledStyle)
            {
                AddStateStyle(go, textRt, null, tmp);
            }

            SavePrefab(go, Path.Combine(PrefabsRoot, "Buttons/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void BuildSimpleButton(string name, Sprite normal, Sprite hover, Sprite pressed, Sprite disabled, Vector2 size,
            bool pressAndDisabledStyle = false)
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
            // Mouse-only UI: a clicked button must not stay "Selected", which hides its hover state.
            btn.navigation = new Navigation { mode = Navigation.Mode.None };

            // Optional child icon container
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = size * 0.65f;
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = new Color(1, 1, 1, 0); // invisible by default until assigned

            // Figma "Icon button": pressed moves the icon 6 px down, disabled fades the whole button to 40%.
            if (pressAndDisabledStyle)
            {
                AddStateStyle(go, iconRt, go.AddComponent<CanvasGroup>(), null);
            }

            SavePrefab(go, Path.Combine(PrefabsRoot, "Buttons/" + name + ".prefab"));
            Object.DestroyImmediate(go);
        }

        private static void AddStateStyle(GameObject button, RectTransform content, CanvasGroup fadeGroup, TMP_Text label)
        {
            var style = button.AddComponent<ButtonStateStyle>();
            var so = new SerializedObject(style);
            so.FindProperty("content").objectReferenceValue = content;
            so.FindProperty("fadeGroup").objectReferenceValue = fadeGroup;
            so.FindProperty("disabledLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
