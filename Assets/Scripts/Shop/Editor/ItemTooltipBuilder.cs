using SproutOfDebt.Shop.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Shop.Editor.ShopUIFactory;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>
    /// Builds the inventory item tooltip from the Figma component "Item tooltip": the ui_tooltip_9s plate that grows
    /// with its text, a bold 26 px name and a 22 px info line under it (dark text, padding 26/22/26/24, gap 4).
    /// Called by InventoryWindowBuilder.
    /// </summary>
    internal static class ItemTooltipBuilder
    {
        private const string PlatePath = "/Panels/ui_tooltip_9s.png";
        private static readonly RectOffset Padding = new RectOffset(26, 26, 22, 24);
        private const float Gap = 4f;
        private const float NameFontSize = 26f;
        private const float InfoFontSize = 22f;
        // Figma "Body small": line height 1.3, 85 % opacity.
        private const float InfoLineHeight = InfoFontSize * 1.3f;
        private const float InfoOpacity = 0.85f;

        /// <summary>Creates the tooltip under the given parent (the windows area, so it can stand over any window).</summary>
        internal static ItemTooltipView Build(RectTransform parent)
        {
            var plate = MakeImage("ItemTooltip", parent, UIArtRoot + PlatePath, false);
            plate.type = Image.Type.Sliced;
            var rt = plate.rectTransform;
            Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0f, 1f), Vector2.zero);

            var layout = plate.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = Padding;
            layout.spacing = Gap;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fitter = plate.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var nameText = MakeText("Name", rt, "Lavender seeds", NameFontSize, Brown, TextAlignmentOptions.TopLeft);
            nameText.fontStyle = FontStyles.Bold;
            nameText.textWrappingMode = TextWrappingModes.NoWrap;

            var infoColor = Brown;
            infoColor.a = InfoOpacity;
            var infoText = MakeText("Info", rt, "Grows in 0:30", InfoFontSize, infoColor, TextAlignmentOptions.MidlineLeft);
            infoText.textWrappingMode = TextWrappingModes.NoWrap;
            infoText.gameObject.AddComponent<LayoutElement>().preferredHeight = InfoLineHeight;

            var view = plate.gameObject.AddComponent<ItemTooltipView>();
            Set(view, "nameText", nameText);
            Set(view, "infoText", infoText);
            return view;
        }
    }
}
