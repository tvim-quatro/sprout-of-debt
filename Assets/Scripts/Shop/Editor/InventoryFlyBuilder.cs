using SproutOfDebt.Shop.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Shop.Editor.ShopUIFactory;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>
    /// Builds the "item flies into the inventory" layer of the HUD (Figma "Laboratory - Potion is ready"):
    /// the flying icon template and the "Added badge" ("Label plate": ui_panel_small_9s, padding 16/16/8/8,
    /// 22 px cream "+1") centered under the bag button, its top 118 px from the top of the frame.
    /// Called by HudPrefabBuilder.
    /// </summary>
    internal static class InventoryFlyBuilder
    {
        // Figma "Potion (flying to inventory)": 64x91; seed packets and ingredients fit the same box.
        private static readonly Vector2 IconSize = new Vector2(70, 91);
        private const float BadgeTop = 118f;
        private const float BadgeFontSize = 22f;

        internal static void Build(RectTransform frame, RectTransform inventoryButton)
        {
            var layer = MakeRect("InventoryFly", frame);
            Stretch(layer);
            var effect = layer.gameObject.AddComponent<InventoryFlyEffect>();

            var icon = MakeImage("FlyingIcon_Template", layer, null, false);
            icon.preserveAspect = true;
            Place(icon.rectTransform, Center, Center, Vector2.zero, IconSize);
            icon.gameObject.SetActive(false);

            var badge = BuildBadge(layer, inventoryButton, out var badgeText);

            Set(effect, "layer", layer);
            Set(effect, "iconTemplate", icon);
            Set(effect, "target", inventoryButton);
            Set(effect, "badge", badge);
            Set(effect, "badgeText", badgeText);
        }

        private static CanvasGroup BuildBadge(RectTransform layer, RectTransform inventoryButton, out TMP_Text text)
        {
            var plate = MakeImage("AddedBadge", layer, UIArtRoot + "/Panels/ui_panel_small_9s.png", false);
            plate.type = Image.Type.Sliced;
            var rt = plate.rectTransform;
            // Same anchor as the bag button (top-right), centered under it.
            float buttonCenterX = inventoryButton.anchoredPosition.x - inventoryButton.sizeDelta.x * (inventoryButton.pivot.x - 0.5f);
            Place(rt, new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(buttonCenterX, -BadgeTop));

            var layout = plate.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fitter = plate.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            text = MakeText("Value", rt, "+1", BadgeFontSize, Cream, TextAlignmentOptions.Center);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            // Figma line height 1.3.
            text.gameObject.AddComponent<LayoutElement>().preferredHeight = BadgeFontSize * 1.3f;

            var group = plate.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            group.alpha = 0f;
            return group;
        }
    }
}
