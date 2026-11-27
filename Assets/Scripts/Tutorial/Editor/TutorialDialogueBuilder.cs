using SproutOfDebt.Tutorial.UI;
using SproutOfDebt.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Tutorial.Editor
{
    /// <summary>
    /// Builds the two tutorial speech views exactly as in Figma (UI Kit "Tutorial hint" and "Character dialogue"):
    /// the imp hint card and the moneylender dialogue.
    /// </summary>
    public static class TutorialDialogueBuilder
    {
        private static readonly Color DarkText = new Color32(0x3D, 0x26, 0x17, 0xFF);  // text/dark
        private static readonly Color LightText = new Color32(0xFA, 0xED, 0xD1, 0xFF); // text/light
        private const float FontSize = 26f;
        private const float BodyLineHeight = 1.3f; // Figma "Body": line height 130%

        /// <summary>
        /// Figma "Tutorial hint" (1052 x 218): portrait 216 x 218, gap 16, card 820 wide, min 180 high,
        /// padding 36 / 40 / 40 / 40, thought and action lines 12 apart. The card grows with the text and sits on
        /// the portrait's bottom edge. The view is placed by its top-left corner per step.
        /// </summary>
        public static TutorialHintView BuildTutorialHint(RectTransform parent)
        {
            var go = new GameObject("TutorialHint", typeof(RectTransform), typeof(TutorialHintView),
                typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(440f, -170f);
            rt.sizeDelta = new Vector2(1052f, 218f);

            var row = go.GetComponent<HorizontalLayoutGroup>();
            row.spacing = 16f;
            row.childAlignment = TextAnchor.LowerLeft;
            SetControl(row, true, false);
            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var frameImg = MakeImage("PortraitFrame", rt, TutorialOverlayBuilder.UIArt + "/Panels/ui_portrait_frame.png");
            AddLayoutElement(frameImg.gameObject, 216f, 218f, 0f);
            var portImg = MakeImage("Portrait", frameImg.rectTransform, TutorialOverlayBuilder.CharArt + "/Imp/char_imp_neutral.png");
            PlaceTopLeft(portImg.rectTransform, new Vector2(36f, 36f), new Vector2(144.9f, 161.7f));

            var cardImg = MakeImage("SpeechCard", rt, TutorialOverlayBuilder.UIArt + "/Panels/ui_dialog_box_9s.png");
            cardImg.type = Image.Type.Sliced;
            AddLayoutElement(cardImg.gameObject, 820f, -1f, 180f);
            var column = cardImg.gameObject.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(40, 40, 36, 40);
            column.spacing = 12f;
            column.childAlignment = TextAnchor.UpperLeft;
            SetControl(column, true, false);
            column.childForceExpandWidth = true;

            var thought = MakeText("ThoughtText", cardImg.rectTransform, DarkText, TextAlignmentOptions.TopLeft);
            SetLineHeight(thought, BodyLineHeight);
            var action = MakeText("ActionText", cardImg.rectTransform, DarkText, TextAlignmentOptions.TopLeft);
            action.fontStyle = FontStyles.Bold;

            var nextBtn = CreateRoundNextButton(cardImg.rectTransform);
            if (!nextBtn.TryGetComponent<LayoutElement>(out var nextLayout))
            {
                nextLayout = nextBtn.gameObject.AddComponent<LayoutElement>();
            }
            nextLayout.ignoreLayout = true;

            var view = go.GetComponent<TutorialHintView>();
            var so = new SerializedObject(view);
            so.FindProperty("portraitImage").objectReferenceValue = portImg;
            so.FindProperty("neutralPortrait").objectReferenceValue = LoadChar("/Imp/char_imp_neutral.png");
            so.FindProperty("worriedPortrait").objectReferenceValue = LoadChar("/Imp/char_imp_worried.png");
            so.FindProperty("happyPortrait").objectReferenceValue = LoadChar("/Imp/char_imp_happy.png");
            so.FindProperty("thoughtText").objectReferenceValue = thought;
            so.FindProperty("actionText").objectReferenceValue = action;
            so.FindProperty("nextButton").objectReferenceValue = nextBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(false);
            return view;
        }

        /// <summary>
        /// Figma "Character dialogue" (1569 x 336 at 120, 684): portrait frame 309 x 312 on the bottom-left, the raven
        /// 225 x 306 rising above it, dialogue 1240 x 240 on the right with the box 30 px below its top,
        /// nameplate at 40, 0 and the text inset 78 / 48 / 40 / 48.
        /// </summary>
        public static MoneylenderDialogueView BuildMoneylenderDialogue(RectTransform parent)
        {
            var go = new GameObject("MoneylenderDialogue", typeof(RectTransform), typeof(MoneylenderDialogueView));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            PlaceTopLeft(rt, new Vector2(120f, 684f), new Vector2(1569f, 336f));

            var frameImg = MakeImage("PortraitFrame", rt, TutorialOverlayBuilder.UIArt + "/Panels/ui_portrait_frame.png");
            PlaceTopLeft(frameImg.rectTransform, new Vector2(0f, 24f), new Vector2(309f, 312f));
            var portImg = MakeImage("Portrait", rt, TutorialOverlayBuilder.CharArt + "/Moneylender/char_moneylender_neutral.png");
            PlaceTopLeft(portImg.rectTransform, new Vector2(42f, 0f), new Vector2(225f, 306f));

            var dialogue = new GameObject("Dialogue", typeof(RectTransform));
            dialogue.transform.SetParent(rt, false);
            var dialogueRt = (RectTransform)dialogue.transform;
            PlaceTopLeft(dialogueRt, new Vector2(329f, 96f), new Vector2(1240f, 240f));

            var boxImg = MakeImage("DialogueBox", dialogueRt, TutorialOverlayBuilder.UIArt + "/Panels/ui_dialog_box_9s.png");
            boxImg.type = Image.Type.Sliced;
            SetInsets(boxImg.rectTransform, 30f, 0f, 0f, 0f);

            var text = MakeText("DialogueText", dialogueRt, DarkText, TextAlignmentOptions.TopLeft);
            SetLineHeight(text, BodyLineHeight);
            SetInsets(text.rectTransform, 78f, 48f, 40f, 48f);

            var npImg = MakeImage("Nameplate", dialogueRt, TutorialOverlayBuilder.UIArt + "/Panels/ui_dialog_nameplate.png");
            PlaceTopLeft(npImg.rectTransform, new Vector2(40f, 0f), new Vector2(207f, 75f));
            var npText = MakeText("NameText", npImg.rectTransform, LightText, TextAlignmentOptions.Center);
            npText.fontStyle = FontStyles.Bold;
            npText.text = "Moneylender";
            TutorialOverlayBuilder.Stretch(npText.rectTransform);

            var nextBtn = CreateRoundNextButton(dialogueRt);

            var view = go.GetComponent<MoneylenderDialogueView>();
            var so = new SerializedObject(view);
            so.FindProperty("portraitImage").objectReferenceValue = portImg;
            so.FindProperty("neutralPortrait").objectReferenceValue = LoadChar("/Moneylender/char_moneylender_neutral.png");
            so.FindProperty("sternPortrait").objectReferenceValue = LoadChar("/Moneylender/char_moneylender_stern.png");
            so.FindProperty("pleasedPortrait").objectReferenceValue = LoadChar("/Moneylender/char_moneylender_pleased.png");
            so.FindProperty("nameplateText").objectReferenceValue = npText;
            so.FindProperty("dialogueText").objectReferenceValue = text;
            so.FindProperty("nextButton").objectReferenceValue = nextBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(false);
            return view;
        }

        // Figma "Next": round button 70 x 70 sticking out 22 px past the box's bottom-right corner, arrow 38 x 38.
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
                img.sprite = TutorialOverlayBuilder.LoadSprite(TutorialOverlayBuilder.UIArt + "/Buttons/ui_btn_round_normal.png");
                var btnComp = btnGo.GetComponent<Button>();
                btnComp.targetGraphic = img;
                btnComp.transition = Selectable.Transition.SpriteSwap;
                var ss = btnComp.spriteState;
                ss.highlightedSprite = TutorialOverlayBuilder.LoadSprite(TutorialOverlayBuilder.UIArt + "/Buttons/ui_btn_round_hover.png");
                ss.pressedSprite = TutorialOverlayBuilder.LoadSprite(TutorialOverlayBuilder.UIArt + "/Buttons/ui_btn_round_pressed.png");
                btnComp.spriteState = ss;
            }

            var btnRt = (RectTransform)btnGo.transform;
            btnRt.anchorMin = btnRt.anchorMax = btnRt.pivot = new Vector2(1f, 0f);
            btnRt.anchoredPosition = new Vector2(22f, -22f);
            btnRt.sizeDelta = new Vector2(70f, 70f);
            btnRt.SetAsLastSibling();

            Transform iconTr = btnGo.transform.Find("Icon");
            GameObject arrowGo = iconTr != null ? iconTr.gameObject : new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (iconTr == null)
            {
                arrowGo.transform.SetParent(btnGo.transform, false);
            }
            var arrowRt = (RectTransform)arrowGo.transform;
            arrowRt.anchorMin = arrowRt.anchorMax = arrowRt.pivot = new Vector2(0.5f, 0.5f);
            arrowRt.anchoredPosition = Vector2.zero;
            arrowRt.sizeDelta = new Vector2(38f, 38f);

            var arrowImg = arrowGo.GetComponent<Image>();
            arrowImg.sprite = TutorialOverlayBuilder.LoadSprite(TutorialOverlayBuilder.UIArt + "/Icons/ui_icon_arrow_right.png");
            arrowImg.color = Color.white;
            arrowImg.preserveAspect = true;
            arrowImg.raycastTarget = false;

            return btnGo.GetComponent<Button>();
        }

        private static Image MakeImage(string name, Transform parent, string spritePath)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = TutorialOverlayBuilder.LoadSprite(spritePath);
            img.raycastTarget = false;
            return img;
        }

        private static TMP_Text MakeText(string name, Transform parent, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            var font = UIPrefabBuilder.EnsureFontAsset();
            if (font != null) text.font = font;
            text.text = string.Empty;
            text.fontSize = FontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        // TMP spaces lines by the font's own line height; add the difference to reach the Figma line height.
        private static void SetLineHeight(TMP_Text text, float lineHeight)
        {
            if (text.font == null) return;
            var face = text.font.faceInfo;
            float natural = face.lineHeight / face.pointSize;
            text.lineSpacing = (lineHeight - natural) * 100f;
        }

        private static void AddLayoutElement(GameObject go, float preferredWidth, float preferredHeight, float minHeight)
        {
            var element = go.AddComponent<LayoutElement>();
            element.preferredWidth = preferredWidth;
            element.preferredHeight = preferredHeight;
            element.minHeight = minHeight;
        }

        private static void SetControl(HorizontalOrVerticalLayoutGroup group, bool controlSize, bool forceExpand)
        {
            group.childControlWidth = controlSize;
            group.childControlHeight = controlSize;
            group.childForceExpandWidth = forceExpand;
            group.childForceExpandHeight = forceExpand;
        }

        // Places a rect like Figma does: by its top-left corner inside the parent.
        private static void PlaceTopLeft(RectTransform rt, Vector2 figmaTopLeft, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(figmaTopLeft.x, -figmaTopLeft.y);
            rt.sizeDelta = size;
        }

        private static void SetInsets(RectTransform rt, float top, float right, float bottom, float left)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static Sprite LoadChar(string path)
        {
            return TutorialOverlayBuilder.LoadSprite(TutorialOverlayBuilder.CharArt + path);
        }
    }
}
