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
    }
}
