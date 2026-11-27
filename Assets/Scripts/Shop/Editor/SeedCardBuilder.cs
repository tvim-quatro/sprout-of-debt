using SproutOfDebt.Shop.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Shop.Editor.ShopUIFactory;
using static SproutOfDebt.Shop.Editor.SeedShopWindowBuilder;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>
    /// Builds the card template of the Seed Shop from the Figma component "Seed card" (150x251):
    /// a 150x183 card area (frame, seed packet, lock, selected check) and the 150x56 price plate 12 px below it.
    /// Called by SeedShopWindowBuilder.
    /// </summary>
    internal static class SeedCardBuilder
    {
        internal static readonly Vector2 CardSize = new Vector2(150, 251);
        private static readonly Vector2 CardAreaSize = new Vector2(150, 183);
        private static readonly Vector2 FrameSize = new Vector2(138, 171);
        private const float PriceTop = 195f;
        private static readonly Vector2 PriceSize = new Vector2(150, 56);

        internal static SeedCardView Build(RectTransform parent)
        {
            var root = MakeRect("SeedCard_Template", parent);
            root.sizeDelta = CardSize;
            // Transparent hit area, so hover and clicks work over the whole card.
            var hit = root.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            var view = root.gameObject.AddComponent<SeedCardView>();

            var area = MakeRect("CardArea", root);
            TopLeft(area, Vector2.zero, CardAreaSize);
            var frame = MakeImage("Frame", area, UIArtRoot + "/Panels/ui_card_item_normal.png", false);
            Place(frame.rectTransform, Center, Center, Vector2.zero, FrameSize);

            var icon = MakeImage("Icon", area, ArtRoot + "/Items/Seeds/icon_seed_lavender.png", false);
            icon.preserveAspect = true;
            TopLeft(icon.rectTransform, new Vector2(34, 37), new Vector2(82, 109));
            var lockIcon = MakeImage("Lock", area, UIArtRoot + "/Icons/ui_icon_lock.png", false);
            TopLeft(lockIcon.rectTransform, new Vector2(46, 54), new Vector2(59, 76));
            var check = MakeImage("SelectedCheck", area, UIArtRoot + "/Icons/ui_icon_check.png", false);
            TopLeft(check.rectTransform, new Vector2(105, 4), new Vector2(41.4f, 36));

            var priceValue = BuildPrice(root, out var priceText);

            Set(view, "frame", frame);
            Set(view, "icon", icon);
            Set(view, "lockIcon", lockIcon.gameObject);
            Set(view, "selectedCheck", check.gameObject);
            Set(view, "priceText", priceText);
            Set(view, "priceValue", priceValue);
            Set(view, "frameNormal", LoadSprite(UIArtRoot + "/Panels/ui_card_item_normal.png"));
            Set(view, "frameHover", LoadSprite(UIArtRoot + "/Panels/ui_card_item_hover.png"));
            Set(view, "frameLocked", LoadSprite(UIArtRoot + "/Panels/ui_card_item_locked.png"));
            return view;
        }

        // Figma "Price": the price tag plate with a centered value row (30 px coin, 8 px gap, 26 px bold cream number).
        private static CanvasGroup BuildPrice(RectTransform root, out TMP_Text priceText)
        {
            var plate = MakeImage("PriceTag", root, UIArtRoot + "/Panels/ui_price_tag.png", false);
            TopLeft(plate.rectTransform, new Vector2(0, PriceTop), PriceSize);

            var value = MakeRect("Value", plate.rectTransform);
            Stretch(value);
            var layout = value.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var group = value.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            var coin = MakeImage("Coin", value, UIArtRoot + "/Icons/ui_icon_coin.png", false);
            var coinSize = coin.gameObject.AddComponent<LayoutElement>();
            coinSize.preferredWidth = coinSize.preferredHeight = 30;
            priceText = MakeText("Amount", value, "10", 26, Cream, TextAlignmentOptions.Center);
            priceText.fontStyle = FontStyles.Bold;
            priceText.textWrappingMode = TextWrappingModes.NoWrap;
            return group;
        }
    }
}
