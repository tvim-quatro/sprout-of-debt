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

        internal static UIWindow BuildTreasuryWindow(RectTransform parent, WindowManager manager)
        {
            var rt = Window("TreasuryWindow", parent, "TREASURY", new Vector2(1000, 640), out var close);
            var window = AddWindow<TreasuryWindow>(rt, manager, close);

            var summary = MakeText("Summary", rt, "Repaid 0 of 1000 coins\nRemaining: 1000", 30, Brown, TextAlignmentOptions.Center);
            Place(summary.rectTransform, Center, Center, new Vector2(0, 205), new Vector2(860, 84));

            var bar = DebtBar("DebtBar", rt, new Vector2(840, 66), true, false);
            Place(bar, Center, Center, new Vector2(0, 110));

            var amountPanel = MakeImage("Amount", rt, UIArtRoot + "/Panels/ui_panel_small_9s.png", false);
            amountPanel.type = UnityEngine.UI.Image.Type.Sliced;
            Place(amountPanel.rectTransform, Center, Center, new Vector2(0, -60), new Vector2(240, 96));
            var coin = MakeImage("CoinIcon", amountPanel.rectTransform, UIArtRoot + "/Icons/ui_icon_coin.png", false);
            Place(coin.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(28, 0), new Vector2(46, 46));
            var amount = MakeText("AmountText", amountPanel.rectTransform, "0", 40, Cream, TextAlignmentOptions.Center);
            Place(amount.rectTransform, Center, Center, new Vector2(24, 0), new Vector2(150, 70));

            var minus = IconButton(rt, "Button_Minus", UIArtRoot + "/Icons/ui_icon_minus.png");
            Place((RectTransform)minus.transform, Center, Center, new Vector2(-200, -60));
            var plus = IconButton(rt, "Button_Plus", UIArtRoot + "/Icons/ui_icon_plus.png");
            Place((RectTransform)plus.transform, Center, Center, new Vector2(200, -60));

            var all = MakeButton("Button_Secondary", rt, "Button_All", "ALL", new Vector2(240, 87), 30);
            Place((RectTransform)all.transform, Center, Center, new Vector2(-145, -175));
            var deposit = MakeButton("Button_Primary", rt, "Button_Deposit", "DEPOSIT", new Vector2(260, 87), 30);
            Place((RectTransform)deposit.transform, Center, Center, new Vector2(145, -175));

            var status = MakeText("Status", rt, "Next recipe: Moonlight Elixir at 100 repaid", 26, Brown, TextAlignmentOptions.Center);
            Place(status.rectTransform, Center, Center, new Vector2(0, -255), new Vector2(860, 40));

            Set(window, "summaryText", summary);
            Set(window, "amountText", amount);
            Set(window, "statusText", status);
            Set(window, "minusButton", minus);
            Set(window, "plusButton", plus);
            Set(window, "allButton", all);
            Set(window, "depositButton", deposit);
            return window;
        }

        private static Button IconButton(RectTransform parent, string name, string iconPath)
        {
            var button = MakeButton("Button_Icon", parent, name, null, new Vector2(99, 99), 0);
            var icon = button.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = LoadSprite(iconPath);
            icon.color = Color.white;
            icon.rectTransform.sizeDelta = new Vector2(54, 54);
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
