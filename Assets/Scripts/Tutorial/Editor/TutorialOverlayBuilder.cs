using System.IO;
using SproutOfDebt.Tutorial.UI;
using SproutOfDebt.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Tutorial.Editor
{
    public static class TutorialOverlayBuilder
    {
        private const string UIArt = "Assets/UI/Art";
        private const string CharArt = "Assets/Art/Characters";
        private const string TutArt = "Assets/Art/Tutorial";

        public static RectTransform BuildTutorialOverlay(RectTransform parent)
        {
            var overlayGo = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(TutorialManager), typeof(TutorialOverlayController));
            overlayGo.transform.SetParent(parent, false);
            var overlayRt = (RectTransform)overlayGo.transform;
            Stretch(overlayRt);

            var manager = overlayGo.GetComponent<TutorialManager>();
            var controller = overlayGo.GetComponent<TutorialOverlayController>();

            var dimMask = BuildDimMask(overlayRt);
            var moneylenderView = BuildMoneylenderDialogue(overlayRt);
            var hintView = BuildTutorialHint(overlayRt);

            var so = new SerializedObject(controller);
            so.FindProperty("dimMask").objectReferenceValue = dimMask;
            so.FindProperty("moneylenderView").objectReferenceValue = moneylenderView;
            so.FindProperty("hintView").objectReferenceValue = hintView;
            so.ApplyModifiedPropertiesWithoutUndo();

            overlayGo.SetActive(true);
            return overlayRt;
        }

        private static TutorialDimMask BuildDimMask(RectTransform parent)
        {
            var go = new GameObject("DimMask", typeof(RectTransform), typeof(TutorialDimMask));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            Stretch(rt);

            Color dimColor = new Color(0f, 0f, 0f, 0.85f);

            var full = CreateDimPanel("FullDim", rt, dimColor);
            Stretch(full);

            var top = CreateDimPanel("TopPanel", rt, dimColor);
            var bottom = CreateDimPanel("BottomPanel", rt, dimColor);
            var left = CreateDimPanel("LeftPanel", rt, dimColor);
            var right = CreateDimPanel("RightPanel", rt, dimColor);

            var highlight = new GameObject("HighlightFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            highlight.transform.SetParent(rt, false);
            var hlRt = (RectTransform)highlight.transform;
            hlRt.anchorMin = hlRt.anchorMax = hlRt.pivot = new Vector2(0.5f, 0.5f);
            var hlImg = highlight.GetComponent<Image>();
            hlImg.sprite = LoadSprite(TutArt + "/ui_tutorial_highlight.png");
            hlImg.type = Image.Type.Sliced;
            hlImg.color = new Color(1f, 0.88f, 0.54f); // #FFE08A gold
            hlImg.raycastTarget = false;
            highlight.SetActive(false);

            var mask = go.GetComponent<TutorialDimMask>();
            var so = new SerializedObject(mask);
            so.FindProperty("fullDimPanel").objectReferenceValue = full;
            so.FindProperty("topPanel").objectReferenceValue = top;
            so.FindProperty("bottomPanel").objectReferenceValue = bottom;
            so.FindProperty("leftPanel").objectReferenceValue = left;
            so.FindProperty("rightPanel").objectReferenceValue = right;
            so.FindProperty("highlightFrame").objectReferenceValue = hlRt;
            so.ApplyModifiedPropertiesWithoutUndo();

            return mask;
        }

        private static RectTransform CreateDimPanel(string name, RectTransform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return rt;
        }

        private static MoneylenderDialogueView BuildMoneylenderDialogue(RectTransform parent)
        {
            var go = new GameObject("MoneylenderDialogue", typeof(RectTransform), typeof(MoneylenderDialogueView));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            // Figma: x: 120, y: 684, w: 1569, h: 336 (bottom center)
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 40);
            rt.sizeDelta = new Vector2(1569, 336);

            // Portrait frame: 309 x 312
            var frameGo = new GameObject("PortraitFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameGo.transform.SetParent(rt, false);
            var frameRt = (RectTransform)frameGo.transform;
            frameRt.anchorMin = frameRt.anchorMax = frameRt.pivot = new Vector2(0f, 0.5f);
            frameRt.anchoredPosition = new Vector2(0, 0);
            frameRt.sizeDelta = new Vector2(309, 312);
            var frameImg = frameGo.GetComponent<Image>();
            frameImg.sprite = LoadSprite(UIArt + "/Panels/ui_portrait_frame.png");
            frameImg.raycastTarget = false;

            // Portrait: 225 x 306
            var portGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portGo.transform.SetParent(frameGo.transform, false);
            var portRt = (RectTransform)portGo.transform;
            portRt.anchorMin = portRt.anchorMax = portRt.pivot = new Vector2(0.5f, 0.5f);
            portRt.anchoredPosition = Vector2.zero;
            portRt.sizeDelta = new Vector2(225, 306);
            var portImg = portGo.GetComponent<Image>();
            portImg.sprite = LoadSprite(CharArt + "/Moneylender/char_moneylender_neutral.png");
            portImg.raycastTarget = false;

            // Dialogue box: 1240 x 240
            var boxGo = new GameObject("DialogueBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            boxGo.transform.SetParent(rt, false);
            var boxRt = (RectTransform)boxGo.transform;
            boxRt.anchorMin = boxRt.anchorMax = boxRt.pivot = new Vector2(1f, 0.5f);
            boxRt.anchoredPosition = new Vector2(0, 0);
            boxRt.sizeDelta = new Vector2(1240, 240);
            var boxImg = boxGo.GetComponent<Image>();
            boxImg.sprite = LoadSprite(UIArt + "/Panels/ui_dialog_box_9s.png");
            boxImg.type = Image.Type.Sliced;
            boxImg.raycastTarget = false;

            // Nameplate: 207 x 75
            var npGo = new GameObject("Nameplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            npGo.transform.SetParent(boxGo.transform, false);
            var npRt = (RectTransform)npGo.transform;
            npRt.anchorMin = npRt.anchorMax = npRt.pivot = new Vector2(0f, 1f);
            npRt.anchoredPosition = new Vector2(40, 18);
            npRt.sizeDelta = new Vector2(207, 75);
            var npImg = npGo.GetComponent<Image>();
            npImg.sprite = LoadSprite(UIArt + "/Panels/ui_dialog_nameplate.png");
            npImg.raycastTarget = false;

            var npText = MakeText("NameText", npRt, "Moneylender", 26, new Color(0.98f, 0.93f, 0.82f), TextAlignmentOptions.Center);
            Stretch(npText.rectTransform);

            // Dialogue text
            var textGo = MakeText("DialogueText", boxRt, "", 26, new Color(0.24f, 0.15f, 0.09f), TextAlignmentOptions.TopLeft);
            var textRt = textGo.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(52, 28);
            textRt.offsetMax = new Vector2(-120, -84);

            // Next button: 70 x 70
            var nextBtn = CreateRoundNextButton(boxRt);

            var view = go.GetComponent<MoneylenderDialogueView>();
            var so = new SerializedObject(view);
            so.FindProperty("portraitImage").objectReferenceValue = portImg;
            so.FindProperty("neutralPortrait").objectReferenceValue = LoadSprite(CharArt + "/Moneylender/char_moneylender_neutral.png");
            so.FindProperty("sternPortrait").objectReferenceValue = LoadSprite(CharArt + "/Moneylender/char_moneylender_stern.png");
            so.FindProperty("pleasedPortrait").objectReferenceValue = LoadSprite(CharArt + "/Moneylender/char_moneylender_pleased.png");
            so.FindProperty("nameplateText").objectReferenceValue = npText;
            so.FindProperty("dialogueText").objectReferenceValue = textGo;
            so.FindProperty("nextButton").objectReferenceValue = nextBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(false);
            return view;
        }

        private static TutorialHintView BuildTutorialHint(RectTransform parent)
        {
            var go = new GameObject("TutorialHint", typeof(RectTransform), typeof(TutorialHintView));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            // Figma: x: 440, y: 170, w: 1052, h: 218 (top center)
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -170);
            rt.sizeDelta = new Vector2(1052, 218);

            // Portrait frame: 216 x 218
            var frameGo = new GameObject("PortraitFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameGo.transform.SetParent(rt, false);
            var frameRt = (RectTransform)frameGo.transform;
            frameRt.anchorMin = frameRt.anchorMax = frameRt.pivot = new Vector2(0f, 0.5f);
            frameRt.anchoredPosition = new Vector2(0, 0);
            frameRt.sizeDelta = new Vector2(216, 218);
            var frameImg = frameGo.GetComponent<Image>();
            frameImg.sprite = LoadSprite(UIArt + "/Panels/ui_portrait_frame.png");
            frameImg.raycastTarget = false;

            // Portrait: 145 x 162
            var portGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portGo.transform.SetParent(frameGo.transform, false);
            var portRt = (RectTransform)portGo.transform;
            portRt.anchorMin = portRt.anchorMax = portRt.pivot = new Vector2(0.5f, 0.5f);
            portRt.anchoredPosition = Vector2.zero;
            portRt.sizeDelta = new Vector2(145, 162);
            var portImg = portGo.GetComponent<Image>();
            portImg.sprite = LoadSprite(CharArt + "/Imp/char_imp_neutral.png");
            portImg.raycastTarget = false;

            // Speech card: 820 x 180
            var cardGo = new GameObject("SpeechCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardGo.transform.SetParent(rt, false);
            var cardRt = (RectTransform)cardGo.transform;
            cardRt.anchorMin = cardRt.anchorMax = cardRt.pivot = new Vector2(1f, 0.5f);
            cardRt.anchoredPosition = new Vector2(0, 0);
            cardRt.sizeDelta = new Vector2(820, 180);
            var cardImg = cardGo.GetComponent<Image>();
            cardImg.sprite = LoadSprite(UIArt + "/Panels/ui_dialog_box_9s.png");
            cardImg.type = Image.Type.Sliced;
            cardImg.raycastTarget = false;

            // Thought text (shifted lower in the card)
            var thought = MakeText("ThoughtText", cardRt, "", 26, new Color(0.24f, 0.15f, 0.09f), TextAlignmentOptions.TopLeft);
            var thRt = thought.rectTransform;
            thRt.anchorMin = new Vector2(0f, 0.5f);
            thRt.anchorMax = Vector2.one;
            thRt.offsetMin = new Vector2(44, 2);
            thRt.offsetMax = new Vector2(-110, -50);

            // Action text (shifted lower in the card)
            var action = MakeText("ActionText", cardRt, "", 26, new Color(0.24f, 0.15f, 0.09f), TextAlignmentOptions.BottomLeft);
            action.fontStyle = FontStyles.Bold;
            var acRt = action.rectTransform;
            acRt.anchorMin = Vector2.zero;
            acRt.anchorMax = new Vector2(1f, 0.5f);
            acRt.offsetMin = new Vector2(44, 18);
            acRt.offsetMax = new Vector2(-110, -6);

            // Next button: 70 x 70
            var nextBtn = CreateRoundNextButton(cardRt);

            var view = go.GetComponent<TutorialHintView>();
            var so = new SerializedObject(view);
            so.FindProperty("portraitImage").objectReferenceValue = portImg;
            so.FindProperty("neutralPortrait").objectReferenceValue = LoadSprite(CharArt + "/Imp/char_imp_neutral.png");
            so.FindProperty("worriedPortrait").objectReferenceValue = LoadSprite(CharArt + "/Imp/char_imp_worried.png");
            so.FindProperty("happyPortrait").objectReferenceValue = LoadSprite(CharArt + "/Imp/char_imp_happy.png");
            so.FindProperty("thoughtText").objectReferenceValue = thought;
            so.FindProperty("actionText").objectReferenceValue = action;
            so.FindProperty("nextButton").objectReferenceValue = nextBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(false);
            return view;
        }

        private static Button CreateRoundNextButton(RectTransform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/Buttons/Button_Round.prefab");
            GameObject btnGo;
            if (prefab != null)
            {
                btnGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                btnGo.name = "Button_Next";
            }
            else
            {
                btnGo = new GameObject("Button_Next", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(parent, false);
                var img = btnGo.GetComponent<Image>();
                img.sprite = LoadSprite(UIArt + "/Buttons/ui_btn_round_normal.png");
                var btnComp = btnGo.GetComponent<Button>();
                btnComp.targetGraphic = img;
                btnComp.transition = Selectable.Transition.SpriteSwap;
                var ss = btnComp.spriteState;
                ss.highlightedSprite = LoadSprite(UIArt + "/Buttons/ui_btn_round_hover.png");
                ss.pressedSprite = LoadSprite(UIArt + "/Buttons/ui_btn_round_pressed.png");
                btnComp.spriteState = ss;
            }

            var btnRt = (RectTransform)btnGo.transform;
            btnRt.anchorMin = btnRt.anchorMax = new Vector2(1f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0.5f);
            btnRt.anchoredPosition = new Vector2(4f, -4f);
            btnRt.sizeDelta = new Vector2(70, 70);
            btnRt.SetAsLastSibling();

            // Child arrow icon
            Transform iconTr = btnGo.transform.Find("Icon");
            GameObject arrowGo = iconTr != null ? iconTr.gameObject : new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (iconTr == null)
            {
                arrowGo.transform.SetParent(btnGo.transform, false);
            }
            var arrowRt = (RectTransform)arrowGo.transform;
            arrowRt.anchorMin = arrowRt.anchorMax = arrowRt.pivot = new Vector2(0.5f, 0.5f);
            arrowRt.anchoredPosition = Vector2.zero;
            arrowRt.sizeDelta = new Vector2(38, 38);

            var arrowImg = arrowGo.GetComponent<Image>();
            arrowImg.sprite = LoadSprite(UIArt + "/Icons/ui_icon_arrow_right.png");
            arrowImg.color = Color.white;
            arrowImg.preserveAspect = true;
            arrowImg.raycastTarget = false;

            return btnGo.GetComponent<Button>();
        }

        private static TMP_Text MakeText(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            var font = UIPrefabBuilder.EnsureFontAsset();
            if (font != null) text.font = font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
