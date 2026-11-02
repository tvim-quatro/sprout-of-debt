using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SproutOfDebt.UI.Editor.UIPrefabUtils;

namespace SproutOfDebt.UI.Editor
{
    // Builds panel prefabs (Prefabs/Panels).
    internal static class PanelPrefabBuilder
    {
        internal static void BuildPanels(TMP_FontAsset font)
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
    }
}
