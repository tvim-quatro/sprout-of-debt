using SproutOfDebt.Shop.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Shop.Editor.ShopUIFactory;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>
    /// Builds the Seed Shop window from the Figma "Seed Shop" frames (Navigation page, "Garden - Seed Shop ..."):
    /// a 1000x776 window with five seed cards, the selected seed row (name, price, Amount −/+), coins and total,
    /// a message line and the "Buy for N" button. Positions are Figma pixels from the window's top-left corner.
    /// Called by HudPrefabBuilder.
    /// </summary>
    internal static class SeedShopWindowBuilder
    {
        private static readonly Vector2 WindowSize = new Vector2(1000, 776);
        private const float PanelTop = 40f;
        private static readonly Vector2 TitleSize = new Vector2(378, 90);
        private const float TitleTop = 11f;
        private static readonly Vector2 ClosePosition = new Vector2(947, 20);
        private static readonly Vector2 CloseSize = new Vector2(70, 72);

        // Content column (Figma "Content": x 60, y 130, width 880).
        private const float ContentLeft = 60f;
        private const float ContentWidth = 880f;
        private const float CardsTop = 130f;
        private const float SelectedTop = 405f;
        private const float SummaryTop = 509f;
        private const float MessageTop = 572f;
        private const float BuyTop = 625f;
        private static readonly Vector2 BuySize = new Vector2(432, 87);

        // Figma "Error" text color #9E211A.
        private static readonly Color ErrorRed = new Color(0.62f, 0.13f, 0.10f);

        internal static UIWindow Build(RectTransform parent, WindowManager manager, ShopConfig config)
        {
            var rt = MakeRect("SeedShopWindow", parent);
            Place(rt, Center, Center, Vector2.zero, WindowSize);
            BuildFrame(rt, out var close);
            var window = rt.gameObject.AddComponent<SeedShopWindow>();
            Set(window, "windowManager", manager);
            Set(window, "closeButton", close);
            Set(window, "config", config);

            var cards = MakeRect("Cards", rt);
            TopLeft(cards, new Vector2(ContentLeft, CardsTop), new Vector2(ContentWidth, SeedCardBuilder.CardSize.y));
            var row = cards.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 28;
            row.childAlignment = TextAnchor.UpperCenter;
            row.childControlWidth = row.childControlHeight = false;
            row.childForceExpandWidth = row.childForceExpandHeight = false;
            Set(window, "cardTemplate", SeedCardBuilder.Build(cards));

            BuildSelectedRow(rt, window);
            BuildSummary(rt, window);

            var message = MakeText("Message", rt, "", 22, ErrorRed, TextAlignmentOptions.Center);
            TopLeft(message.rectTransform, new Vector2(ContentLeft, MessageTop), new Vector2(ContentWidth, 29));
            Set(window, "messageText", message);

            var buy = MakeButton("Button_Primary", rt, "Button_Buy", "Buy", BuySize, 26);
            TopLeft((RectTransform)buy.transform, new Vector2((WindowSize.x - BuySize.x) / 2f, BuyTop), BuySize);
            Set(window, "buyButton", buy);
            Set(window, "buyLabel", buy.GetComponentInChildren<TMP_Text>());

            // The tooltip stands over the whole windows area, so it is built next to the window, not inside it.
            Set(window, "tooltip", ItemTooltipBuilder.Build(parent));
            rt.gameObject.SetActive(false);
            return window;
        }

        // Figma "Window": the panel starts 40 px below the top, the title plate overlaps it, the close button sticks out on the right.
        private static void BuildFrame(RectTransform rt, out Button close)
        {
            var panel = MakeImage("Panel", rt, UIArtRoot + "/Panels/ui_panel_window_9s.png", false);
            panel.type = Image.Type.Sliced;
            panel.raycastTarget = true;
            panel.rectTransform.anchorMin = Vector2.zero;
            panel.rectTransform.anchorMax = Vector2.one;
            panel.rectTransform.offsetMin = Vector2.zero;
            panel.rectTransform.offsetMax = new Vector2(0, -PanelTop);

            var title = MakeImage("Title", rt, UIArtRoot + "/Panels/ui_panel_header_9s.png", false);
            title.type = Image.Type.Sliced;
            // Blocks clicks like the panel, so a click on the plate doesn't reach the dimmer behind and close the window.
            title.raycastTarget = true;
            TopLeft(title.rectTransform, new Vector2((WindowSize.x - TitleSize.x) / 2f, TitleTop), TitleSize);
            var titleText = MakeText("TitleText", title.rectTransform, "Seed Shop", 32, Cream, TextAlignmentOptions.Center);
            titleText.fontStyle = FontStyles.Bold;
            Stretch(titleText.rectTransform);

            close = MakeButton("Button_Close", rt, "Button_Close", null, CloseSize, 0);
            TopLeft((RectTransform)close.transform, ClosePosition, CloseSize);
        }

        // Figma "Selected seed" (880x80, space between) and "Hint" in the same place when nothing is selected.
        private static void BuildSelectedRow(RectTransform rt, SeedShopWindow window)
        {
            var hint = MakeText("Hint", rt, "Click a seed to choose what to buy", 26, Brown, TextAlignmentOptions.Center);
            TopLeft(hint.rectTransform, new Vector2(ContentLeft, SelectedTop), new Vector2(ContentWidth, 80));
            Set(window, "hint", hint.gameObject);

            var row = MakeRect("SelectedSeed", rt);
            TopLeft(row, new Vector2(ContentLeft, SelectedTop), new Vector2(ContentWidth, 80));
            Set(window, "selectedRow", row.gameObject);

            var name = MakeText("SeedName", row, "Lavender seeds", 32, Brown, TextAlignmentOptions.TopLeft);
            name.fontStyle = FontStyles.Bold;
            TopLeft(name.rectTransform, new Vector2(0, 4), new Vector2(420, 38));
            var price = MakeText("PriceEach", row, "10 coins each", 22, Brown, TextAlignmentOptions.TopLeft);
            TopLeft(price.rectTransform, new Vector2(0, 48), new Vector2(420, 29));
            Set(window, "selectedNameText", name);
            Set(window, "selectedPriceText", price);

            // Figma "Amount" (426x80, gap 16): label, minus, value field, plus, right-aligned.
            var label = MakeText("AmountLabel", row, "Amount", 26, Brown, TextAlignmentOptions.Right);
            label.fontStyle = FontStyles.Bold;
            // One line always: right-aligned to the minus button (16 px gap), free to grow to the left.
            label.textWrappingMode = TextWrappingModes.NoWrap;
            TopLeft(label.rectTransform, new Vector2(394, 25), new Vector2(160, 31));

            var minus = ShopWindowsBuilder.IconButton(row, "Button_Minus", UIArtRoot + "/Icons/ui_icon_minus.png", new Vector2(79, 79));
            TopLeft((RectTransform)minus.transform, new Vector2(570, 1), new Vector2(79, 79));

            var field = MakeImage("AmountField", row, UIArtRoot + "/Panels/ui_panel_small_9s.png", false);
            field.type = Image.Type.Sliced;
            TopLeft(field.rectTransform, new Vector2(665, 0), new Vector2(120, 80));
            var value = MakeText("AmountValue", field.rectTransform, "1", 32, Cream, TextAlignmentOptions.Center);
            value.fontStyle = FontStyles.Bold;
            Stretch(value.rectTransform);

            var plus = ShopWindowsBuilder.IconButton(row, "Button_Plus", UIArtRoot + "/Icons/ui_icon_plus.png", new Vector2(79, 79));
            TopLeft((RectTransform)plus.transform, new Vector2(801, 1), new Vector2(79, 79));

            Set(window, "amountText", value);
            Set(window, "minusButton", minus);
            Set(window, "plusButton", plus);
        }

        // Figma "Summary" (880x39, space between): "Your coins" on the left, "Total" on the right.
        private static void BuildSummary(RectTransform rt, SeedShopWindow window)
        {
            var coins = CoinGroup("YourCoins", rt, "Your coins", TextAnchor.MiddleLeft, out var coinsValue);
            TopLeft(coins, new Vector2(ContentLeft, SummaryTop), new Vector2(ContentWidth / 2f, 39));
            var total = CoinGroup("Total", rt, "Total", TextAnchor.MiddleRight, out var totalValue);
            TopLeft(total, new Vector2(ContentLeft + ContentWidth / 2f, SummaryTop), new Vector2(ContentWidth / 2f, 39));

            Set(window, "coinsText", coinsValue);
            Set(window, "totalGroup", total.gameObject);
            Set(window, "totalText", totalValue);
        }

        // Label, 39 px coin and value in a row with a 12 px gap, all 26 px bold dark text.
        private static RectTransform CoinGroup(string name, RectTransform parent, string label, TextAnchor alignment, out TMP_Text value)
        {
            var group = MakeRect(name, parent);
            var layout = group.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childAlignment = alignment;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            var labelText = MakeText("Label", group, label, 26, Brown, TextAlignmentOptions.Left);
            labelText.fontStyle = FontStyles.Bold;
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            var coin = MakeImage("Coin", group, UIArtRoot + "/Icons/ui_icon_coin.png", false);
            var coinSize = coin.gameObject.AddComponent<LayoutElement>();
            coinSize.preferredWidth = coinSize.preferredHeight = 39;
            value = MakeText("Value", group, "0", 26, Brown, TextAlignmentOptions.Left);
            value.fontStyle = FontStyles.Bold;
            value.textWrappingMode = TextWrappingModes.NoWrap;
            return group;
        }

        internal static void TopLeft(RectTransform rt, Vector2 position, Vector2 size)
        {
            Place(rt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(position.x, -position.y), size);
        }
    }
}
