using SproutOfDebt.Shop.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SproutOfDebt.Shop.Editor.ShopUIFactory;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>Builds the Seed Shop and Treasury windows, the window manager and the debt bar used inside them.</summary>
    internal static class ShopWindowsBuilder
    {
        internal static RectTransform BuildWindowsRoot(RectTransform canvas, out WindowManager manager)
        {
            var root = MakeRect("Windows", canvas);
            Stretch(root);
            manager = root.gameObject.AddComponent<WindowManager>();

            var dimmerGo = new GameObject("Dimmer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            dimmerGo.transform.SetParent(root, false);
            Stretch((RectTransform)dimmerGo.transform);
            dimmerGo.GetComponent<Image>().color = new Color(0.05f, 0.02f, 0.01f, 0.6f);
            var dimmer = dimmerGo.GetComponent<Button>();
            dimmer.transition = Selectable.Transition.None;
            Set(manager, "dimmer", dimmer);
            dimmerGo.SetActive(false);
            return root;
        }

        private static RectTransform Window(string name, RectTransform parent, string title, Vector2 size, out Button close)
        {
            var window = MakeImage(name, parent, UIArtRoot + "/Panels/ui_panel_window_9s.png", false);
            window.type = UnityEngine.UI.Image.Type.Sliced;
            var rt = window.rectTransform;
            Place(rt, Center, Center, new Vector2(0, -20), size);

            var header = MakeImage("Header", rt, UIArtRoot + "/Panels/ui_panel_header_9s.png", false);
            header.type = UnityEngine.UI.Image.Type.Sliced;
            Place(header.rectTransform, new Vector2(0.5f, 1), Center, new Vector2(0, 0), new Vector2(480, 90));
            var titleText = MakeText("Title", header.rectTransform, title, 40, Cream, TextAlignmentOptions.Center);
            Stretch(titleText.rectTransform);

            close = MakeButton("Button_Close", rt, "Button_Close", null, new Vector2(68, 70), 0);
            Place((RectTransform)close.transform, new Vector2(1, 1), Center, new Vector2(-36, -36));

            rt.gameObject.SetActive(false);
            return rt;
        }

        private static T AddWindow<T>(RectTransform rt, WindowManager manager, Button close) where T : UIWindow
        {
            var window = rt.gameObject.AddComponent<T>();
            Set(window, "windowManager", manager);
            Set(window, "closeButton", close);
            return window;
        }

        internal static UIWindow BuildSeedShopWindow(RectTransform parent, WindowManager manager, ShopConfig config)
        {
            var rt = Window("SeedShopWindow", parent, "SEED SHOP", new Vector2(1340, 620), out var close);
            var window = AddWindow<SeedShopWindow>(rt, manager, close);

            var cards = MakeRect("Cards", rt);
            Place(cards, Center, Center, new Vector2(0, -20), new Vector2(1220, 460));
            var layout = cards.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 25;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            var template = BuildSeedCard(cards);
            Set(window, "config", config);
            Set(window, "cardTemplate", template);
            return window;
        }

        private static SeedCardView BuildSeedCard(RectTransform parent)
        {
            var root = MakeRect("SeedCard_Template", parent);
            root.sizeDelta = new Vector2(220, 450);
            var view = root.gameObject.AddComponent<SeedCardView>();

            var card = MakeImage("Card", root, UIArtRoot + "/Panels/ui_card_item_normal.png");
            Place(card.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero);

            var icon = MakeImage("Icon", card.rectTransform, ArtRoot + "/Items/Seeds/icon_seed_lavender.png", false);
            icon.preserveAspect = true;
            Place(icon.rectTransform, Center, Center, Vector2.zero, new Vector2(78, 104));

            var lockIcon = MakeImage("Lock", card.rectTransform, UIArtRoot + "/Icons/ui_icon_lock.png", false);
            Place(lockIcon.rectTransform, Center, Center, Vector2.zero, new Vector2(54, 69));

            var nameText = MakeText("Name", root, "Lavender", 28, Brown, TextAlignmentOptions.Center);
            Place(nameText.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -180), new Vector2(220, 36));
            var owned = MakeText("Owned", root, "Owned: 0", 22, Brown, TextAlignmentOptions.Center);
            Place(owned.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -216), new Vector2(220, 30));

            var price = PriceTag("PriceTag", root, "3", new Vector2(160, 60));
            var priceTag = (RectTransform)price.transform.parent.parent;
            Place(priceTag, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -254));

            var buy = MakeButton("Button_Primary", root, "Button_Buy", "BUY", new Vector2(200, 72), 28);
            Place((RectTransform)buy.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -328));

            var locked = MakeText("LockedText", root, "Repay 250\nto unlock", 24, Brown, TextAlignmentOptions.Top);
            Place(locked.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -222), new Vector2(220, 80));

            Set(view, "card", card);
            Set(view, "cardNormal", LoadSprite(UIArtRoot + "/Panels/ui_card_item_normal.png"));
            Set(view, "cardLocked", LoadSprite(UIArtRoot + "/Panels/ui_card_item_locked.png"));
            Set(view, "icon", icon);
            Set(view, "lockIcon", lockIcon.gameObject);
            Set(view, "nameText", nameText);
            Set(view, "ownedText", owned);
            Set(view, "priceTag", priceTag.gameObject);
            Set(view, "priceText", price);
            Set(view, "buyButton", buy);
            Set(view, "lockedText", locked);
            return view;
        }

        // Figma "Treasury window" (131:3903): 880x656 at (520, 170) -> centered (0, 42).
        internal static UIWindow BuildTreasuryWindow(RectTransform parent, WindowManager manager)
        {
            var rt = Window("TreasuryWindow", parent, "Treasury", new Vector2(880, 656), out var close);
            Place(rt, Center, Center, new Vector2(0, 42), new Vector2(880, 656));
            var window = AddWindow<TreasuryWindow>(rt, manager, close);

            // Left column: Moneylender portrait inside frame (Figma: x=50, y=130, 232x234).
            var portraitFrame = MakeImage("PortraitFrame", rt, UIArtRoot + "/Panels/ui_portrait_frame.png", false);
            Place(portraitFrame.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(50, -130), new Vector2(232, 234));
            var portrait = MakeImage("Moneylender", rt, ArtRoot + "/Characters/Moneylender/char_moneylender_neutral.png", false);
            portrait.preserveAspect = true;
            Place(portrait.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(82, -115), new Vector2(169, 230));

            // Right column (Figma "Info": x=330, y=130, w=480).
            var debtHeader = MakeText("DebtPaidLabel", rt, "Debt paid", 26, Brown, TextAlignmentOptions.Left);
            Place(debtHeader.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(330, -130), new Vector2(200, 34));

            var bar = DebtBar("DebtBar", rt, new Vector2(440, 66), false, false);
            Place(bar, new Vector2(0, 1), new Vector2(0, 1), new Vector2(330, -187));

            var summaryPlate = MakeImage("DebtValuePlate", rt, UIArtRoot + "/Panels/ui_panel_small_9s.png", false);
            summaryPlate.type = Image.Type.Sliced;
            Place(summaryPlate.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(610, -259), new Vector2(160, 45));
            var summary = MakeText("Summary", summaryPlate.rectTransform, "0 / 1000", 24, Cream, TextAlignmentOptions.Center);
            Stretch(summary.rectTransform);

            // Your coins row (Figma: x=330, y=316).
            var yourCoinsLabel = MakeText("YourCoinsLabel", rt, "Your coins", 26, Brown, TextAlignmentOptions.Left);
            Place(yourCoinsLabel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(330, -320), new Vector2(140, 34));
            var yourCoinIcon = MakeImage("YourCoinIcon", rt, UIArtRoot + "/Icons/ui_icon_coin.png", false);
            Place(yourCoinIcon.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(476, -316), new Vector2(39, 39));
            var coinsText = MakeText("CoinsValue", rt, "0", 26, Brown, TextAlignmentOptions.Left);
            Place(coinsText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(527, -320), new Vector2(140, 34));

            // Pay amount selector row (Figma: x=330, y=367).
            var payLabel = MakeText("PayLabel", rt, "Pay", 26, Brown, TextAlignmentOptions.Left);
            Place(payLabel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(330, -392), new Vector2(55, 34));

            var minus = IconButton(rt, "Button_Minus", UIArtRoot + "/Icons/ui_icon_minus.png", new Vector2(79, 79));
            Place((RectTransform)minus.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(393, -368));

            var amountPanel = MakeImage("Amount", rt, UIArtRoot + "/Panels/ui_panel_small_9s.png", false);
            amountPanel.type = Image.Type.Sliced;
            Place(amountPanel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(488, -367), new Vector2(180, 80));
            var amount = MakeText("AmountText", amountPanel.rectTransform, "0", 32, Cream, TextAlignmentOptions.Center);
            Stretch(amount.rectTransform);

            var plus = IconButton(rt, "Button_Plus", UIArtRoot + "/Icons/ui_icon_plus.png", new Vector2(79, 79));
            Place((RectTransform)plus.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(684, -368));

            var all = MakeButton("Button_Secondary", rt, "Button_All", "All", new Vector2(78, 79), 24);
            Place((RectTransform)all.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(772, -368));

            // Debt after payment / no coins status (Figma: x=330, y=459).
            var status = MakeText("Status", rt, "Debt after payment: 0 / 1000", 24, Brown, TextAlignmentOptions.Left);
            Place(status.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(330, -462), new Vector2(510, 36));

            // Primary Pay action button (Figma: x=280, y=528, 320x72). Keep name Button_Deposit for tutorial locator.
            var deposit = MakeButton("Button_Primary", rt, "Button_Deposit", "Pay 0", new Vector2(320, 72), 26);
            Place((RectTransform)deposit.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -528));
            var depositLabel = deposit.GetComponentInChildren<TMP_Text>();

            Set(window, "summaryText", summary);
            Set(window, "coinsText", coinsText);
            Set(window, "amountText", amount);
            Set(window, "statusText", status);
            Set(window, "depositLabel", depositLabel);
            Set(window, "minusButton", minus);
            Set(window, "plusButton", plus);
            Set(window, "allButton", all);
            Set(window, "depositButton", deposit);
            Set(window, "moneylenderPortrait", portrait);
            Set(window, "neutralPortrait", LoadSprite(ArtRoot + "/Characters/Moneylender/char_moneylender_neutral.png"));
            Set(window, "pleasedPortrait", LoadSprite(ArtRoot + "/Characters/Moneylender/char_moneylender_pleased.png"));
            return window;
        }

        private static Button IconButton(RectTransform parent, string name, string iconPath, Vector2 size)
        {
            var button = MakeButton("Button_Icon", parent, name, null, size, 0);
            var icon = button.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = LoadSprite(iconPath);
            icon.color = Color.white;
            icon.rectTransform.sizeDelta = new Vector2(size.x * 0.55f, size.y * 0.55f);
            return button;
        }

        internal static RectTransform DebtBar(string name, RectTransform parent, Vector2 size, bool potionIcons, bool withLabel)
        {
            var bar = MakeRect(name, parent);
            bar.sizeDelta = size;
            var view = bar.gameObject.AddComponent<DebtBarView>();

            var frame = MakeImage("Frame", bar, UIArtRoot + "/Panels/ui_bar_frame_9s.png", false);
            frame.type = UnityEngine.UI.Image.Type.Sliced;
            Stretch(frame.rectTransform);

            var fillArea = MakeRect("FillArea", bar);
            Stretch(fillArea, 15, 15);
            var fill = MakeImage("Fill", fillArea, UIArtRoot + "/Bars/ui_bar_fill.png", false);
            fill.raycastTarget = false;
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0.3f, 1);
            fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

            var markers = MakeRect("Markers", bar);
            Stretch(markers, 15, 0);

            TMP_Text label = null;
            if (withLabel)
            {
                label = MakeText("Label", bar, "DEBT REPAID  0 / 1000", 24, Cream, TextAlignmentOptions.Top);
                Place(label.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0, -4), new Vector2(500, 32));
            }

            Set(view, "fill", fillRt);
            Set(view, "markersRoot", markers);
            Set(view, "markerLocked", LoadSprite(UIArtRoot + "/Bars/ui_bar_milestone_locked.png"));
            Set(view, "markerReached", LoadSprite(UIArtRoot + "/Bars/ui_bar_milestone_reached.png"));
            Set(view, "label", label);
            SetBool(view, "showPotionIcons", potionIcons);
            return bar;
        }
    }
}
