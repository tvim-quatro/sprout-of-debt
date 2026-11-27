using System;
using System.IO;
using SproutOfDebt.Core.Stubs;
using SproutOfDebt.Items;
using SproutOfDebt.Shop.UI;
using SproutOfDebt.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static SproutOfDebt.Shop.Editor.ShopUIFactory;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>
    /// Creates the shop data assets (potions, seeds, customers, ShopConfig) and builds the Shop Counter scene.
    /// The top bar, nav bar and windows come from the shared HUD prefab (<see cref="HudPrefabBuilder"/>).
    /// Existing data assets are kept, so tweaked balance is not lost.
    /// </summary>
    public static class ShopSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/ShopCounter.unity";
        private const string DataRoot = "Assets/Data/Shop";
        private const string PotionsRoot = "Assets/Data/Potions";

        // key, shared asset name (Data/Potions/Potion_<asset>), display name, sell price, debt threshold that unlocks it (Figma: 0, 150, 350, 500, 750)
        private static readonly (string key, string asset, string name, int price, int threshold)[] Potions =
        {
            ("calming", "CalmingDraught", "Calming Potion", 25, 0),
            ("moonlight", "MoonlightElixir", "Moonlight Potion", 40, 150),
            ("shadow", "ShadowTonic", "Shadow Potion", 55, 350),
            ("ember", "EmberBrew", "Ember Potion", 90, 500),
            ("mandrake", "MandrakeRemedy", "Mandrake Potion", 150, 750),
        };

        // plant key, display name, buy price, potion key it unlocks with (Figma: Laboratory page, Potions table)
        private static readonly (string key, string name, int price, string potion)[] Seeds =
        {
            ("lavender", "Lavender", 10, "calming"),
            ("moonpetal", "Moonpetal", 15, "calming"),
            ("nightshade", "Nightshade", 20, "moonlight"),
            ("emberroot", "Emberroot", 25, "ember"),
            ("mandrake", "Mandrake", 40, "mandrake"),
        };

        // Figma: Counter components, Customers. key (sprite/asset name), display name, greetings, order lines
        // ({order} is filled in by the game), line after a sale, line after "Sorry, we're out".
        private static readonly (string key, string name, string[] greeting, string[] order, string[] thanks, string[] sorry)[] Customers =
        {
            ("fox", "Rowan",
                new[] { "Evening, friend! Long road behind me.", "Ah, a new shop on my route. Lovely." },
                new[] { "I'll take {order} for the journey.", "Got {order}? My paws are aching." },
                new[] { "Splendid! I'll tell every inn about you." },
                new[] { "No luck? Well, the road goes on." }),
            ("mage", "Pip",
                new[] { "H-hello! Master sent me.", "Hi! Sorry, am I too early?" },
                new[] { "I need {order} for my exam.", "Master wrote: {order}. I think." },
                new[] { "Thank you! Master will be so pleased!" },
                new[] { "Oh no... how do I tell Master?" }),
            ("sprout", "Morel",
                new[] { "...hello.", "Moss says hi." },
                new[] { "{order}. Please.", "Need {order}. For the roots." },
                new[] { "Good. Very good." },
                new[] { "...okay. Will wait." }),
            ("werewolf", "Bram",
                new[] { "Oh, um... hello. Don't mind the fur.", "Evening. I'll be quick." },
                new[] { "Could I have {order}? Full moon's close.", "{order}, if it's no trouble." },
                new[] { "Thank you kindly. Truly." },
                new[] { "That's alright. I'll manage... somehow." }),
            ("witch", "Granny Hazel",
                new[] { "Well now, look at this little shop!", "Back again, dearie." },
                new[] { "Let's see your skill. {order}, please.", "My old bones want {order}." },
                new[] { "Not bad, little one. Not bad at all." },
                new[] { "Out already? Brew faster, dearie." }),
        };

        [MenuItem("Tools/Shop/Build Shop Counter Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Load after NewScene: opening a scene unloads unused assets, which would invalidate earlier references.
            var config = EnsureData();
            HudPrefabBuilder.BuildPrefab();

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            camGo.transform.position = new Vector3(0, 0, -10);

            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            GameCursorSetup.PlaceInScene();
            ToastSetup.PlaceInScene();

            new GameObject("Services", typeof(StubServicesInstaller));

            var canvasGo = new GameObject("ShopCounter_Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            FixedFrameCanvas.ConfigureScaler(canvasGo.GetComponent<CanvasScaler>());
            // The location is drawn in the fixed 16:9 frame, with black bars on other screen shapes.
            var frame = FixedFrameCanvas.CreateFrame((RectTransform)canvasGo.transform);

            BuildLocation(frame, config);
            // Top bar, nav bar and windows are drawn by the shared HUD canvas on top of the location.
            HudPrefabBuilder.InstantiateHud();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[ShopSceneBuilder] Built " + ScenePath);
        }

        #region Data

        [MenuItem("Tools/Shop/Create Missing Shop Data")]
        public static ShopConfig EnsureData()
        {
            Directory.CreateDirectory(PotionsRoot);
            Directory.CreateDirectory(DataRoot + "/Seeds");
            Directory.CreateDirectory(DataRoot + "/Customers");

            var config = LoadOrCreate<ShopConfig>(DataRoot + "/ShopConfig.asset", c => { });
            bool fillConfig = config.milestones.Count == 0 && config.seeds.Count == 0 && config.customers.Count == 0;

            foreach (var p in Potions)
            {
                // Potions are shared with the Laboratory: existing assets are kept as they are.
                var potion = LoadOrCreate<PotionDefinition>(PotionPath(p.key), a =>
                {
                    var so = new SerializedObject(a);
                    so.FindProperty("id").stringValue = "potion_" + p.key;
                    so.FindProperty("displayName").stringValue = p.name;
                    so.FindProperty("price").intValue = p.price;
                    so.FindProperty("icon").objectReferenceValue = LoadSprite($"{ArtRoot}/Items/Potions/icon_potion_{p.key}.png");
                    so.ApplyModifiedPropertiesWithoutUndo();
                });
                if (fillConfig) config.milestones.Add(new DebtMilestone { threshold = p.threshold, potion = potion });
            }

            foreach (var s in Seeds)
            {
                var seed = LoadOrCreate<SeedData>($"{DataRoot}/Seeds/Seed_{s.key}.asset", a =>
                {
                    a.id = "seed_" + s.key;
                    a.displayName = s.name;
                    a.buyPrice = s.price;
                    a.icon = LoadSprite($"{ArtRoot}/Items/Seeds/icon_seed_{s.key}.png");
                    a.unlockedWith = AssetDatabase.LoadAssetAtPath<PotionDefinition>(PotionPath(s.potion));
                });
                if (fillConfig) config.seeds.Add(seed);
            }

            foreach (var c in Customers)
            {
                var customer = LoadOrCreate<CustomerData>($"{DataRoot}/Customers/Customer_{c.key}.asset", a =>
                {
                    string path = $"{ArtRoot}/Characters/Customers/char_customer_{c.key}_";
                    a.neutral = LoadSprite(path + "neutral.png");
                    a.happy = LoadSprite(path + "happy.png");
                    a.disappointed = LoadSprite(path + "disappointed.png");
                });

                // Customers made before the Figma Counter lines get their names and lines once; later edits are kept.
                if (customer.greetingLines == null || customer.greetingLines.Length == 0)
                {
                    customer.displayName = c.name;
                    customer.greetingLines = c.greeting;
                    customer.orderLines = c.order;
                    customer.thanksLines = c.thanks;
                    customer.disappointedLines = c.sorry;
                    EditorUtility.SetDirty(customer);
                }
                if (fillConfig) config.customers.Add(customer);
            }

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static string PotionPath(string key)
        {
            foreach (var p in Potions)
            {
                if (p.key == key) return $"{PotionsRoot}/Potion_{p.asset}.asset";
            }
            throw new ArgumentException("[ShopSceneBuilder] Unknown potion key: " + key);
        }

        private static T LoadOrCreate<T>(string path, Action<T> init) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            init(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        #endregion

        #region Location

        // Figma Counter frames are 1920x1080 with the origin at the top left; the location frame is centered.
        private static void PlaceFigma(RectTransform rt, float x, float y, float width, float height)
        {
            Place(rt, Center, Center, new Vector2(x + width / 2f - 960f, 540f - (y + height / 2f)), new Vector2(width, height));
        }

        private static void BuildLocation(RectTransform canvas, ShopConfig config)
        {
            var location = MakeRect("Location_ShopCounter", canvas);
            Stretch(location);
            var controller = location.gameObject.AddComponent<ShopCounterController>();

            Stretch(MakeImage("BackWall", location, ArtRoot + "/Backgrounds/counter_bg_back.png", false).rectTransform);

            // Text on the blank hanging sign of the background: OPEN or the break countdown.
            var sign = MakeText("SignText", location, "OPEN", 32, Cream, TextAlignmentOptions.Center);
            Place(sign.rectTransform, Center, Center, new Vector2(-240, 320), new Vector2(190, 90));
            var signView = sign.gameObject.AddComponent<ShopSignView>();
            Set(signView, "text", sign);

            // The visitor stands in the middle, behind the counter table that hides their bottom edge.
            var customerImage = MakeImage("Customer", location, ArtRoot + "/Characters/Customers/char_customer_fox_neutral.png");
            var customerRt = customerImage.rectTransform;
            Place(customerRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 182));
            customerRt.gameObject.AddComponent<CanvasGroup>();
            var customerView = customerRt.gameObject.AddComponent<CustomerView>();
            Set(customerView, "image", customerImage);

            var counter = MakeImage("CounterFront", location, ArtRoot + "/Backgrounds/counter_front.png");
            Place(counter.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero);

            // Sale reward: coin pile on the counter tray with two sparkles (Figma "Counter - Sale - Potion sold").
            var pile = MakeImage("CoinPile", location, ArtRoot + "/Effects/counter_coins_pile.png");
            PlaceFigma(pile.rectTransform, 1160, 770, 276, 90);
            var sparkleLeft = MakeImage("Sparkle_1", pile.rectTransform, ArtRoot + "/Effects/fx_sparkle.png", false);
            Place(sparkleLeft.rectTransform, new Vector2(0, 1), Center, new Vector2(82, 8), new Vector2(40, 40));
            var sparkleRight = MakeImage("Sparkle_2", pile.rectTransform, ArtRoot + "/Effects/fx_sparkle.png", false);
            Place(sparkleRight.rectTransform, new Vector2(1, 1), Center, new Vector2(-14, -12), new Vector2(40, 40));
            pile.gameObject.SetActive(false);

            var dialogue = BuildCustomerDialogue(location);
            var orderPanel = BuildOrderPanel(location);

            // Coins fly from the pile up to the HUD coin counter, above everything on the location.
            var fxLayer = MakeRect("CoinFx", location);
            Stretch(fxLayer);
            var burst = fxLayer.gameObject.AddComponent<CoinBurst>();
            Set(burst, "coinPile", pile);
            SetArray(burst, "sparkles", new Object[] { sparkleLeft, sparkleRight });
            SetArray(burst, "spinFrames", new Object[]
            {
                LoadSprite(ArtRoot + "/Effects/fx_coin_01.png"),
                LoadSprite(ArtRoot + "/Effects/fx_coin_02.png"),
                LoadSprite(ArtRoot + "/Effects/fx_coin_03.png"),
                LoadSprite(ArtRoot + "/Effects/fx_coin_04.png"),
            });

            Set(controller, "config", config);
            Set(controller, "customerView", customerView);
            Set(controller, "dialog", dialogue);
            Set(controller, "orderPanel", orderPanel);
            Set(controller, "coinBurst", burst);
        }

        private static CustomerDialogue BuildCustomerDialogue(RectTransform parent)
        {
            // Dialogue box at the bottom left, over the counter table, with the name on a plate at its top edge.
            var box = MakeImage("DialogueBox", parent, UIArtRoot + "/Panels/ui_dialog_box_9s.png", false);
            box.type = Image.Type.Sliced;
            box.raycastTarget = true;
            var boxRt = box.rectTransform;
            PlaceFigma(boxRt, 145, 735, 952, 200);
            var dialogue = box.gameObject.AddComponent<CustomerDialogue>();
            var advance = box.gameObject.AddComponent<Button>();
            advance.transition = Selectable.Transition.None;

            var nameplate = MakeImage("Nameplate", boxRt, UIArtRoot + "/Panels/ui_dialog_nameplate.png");
            Place(nameplate.rectTransform, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(35, -3));
            var speaker = MakeText("SpeakerName", nameplate.rectTransform, "Rowan", 28, Cream, TextAlignmentOptions.Center);
            Stretch(speaker.rectTransform);

            var body = MakeText("Body", boxRt, "Evening, friend! Long road behind me. I'll take a Calming Potion for the journey.", 28, Brown, TextAlignmentOptions.TopLeft);
            var bodyRt = body.rectTransform;
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(43, 24);
            bodyRt.offsetMax = new Vector2(-43, -44);

            Set(dialogue, "speakerText", speaker);
            Set(dialogue, "bodyText", body);
            Set(dialogue, "typewriter", body.gameObject.AddComponent<SproutOfDebt.UI.TypewriterText>());
            Set(dialogue, "advanceArea", advance);
            return dialogue;
        }

        private static OrderPanel BuildOrderPanel(RectTransform parent)
        {
            // Figma "Order panel": right of the visitor, 512x618, "Order" header plate over its top edge.
            var window = MakeImage("OrderPanel", parent, UIArtRoot + "/Panels/ui_panel_window_9s.png", false);
            window.type = Image.Type.Sliced;
            window.raycastTarget = true;
            var rt = window.rectTransform;
            PlaceFigma(rt, 1385, 312, 512, 618);
            var panel = window.gameObject.AddComponent<OrderPanel>();

            var header = MakeImage("Header", rt, UIArtRoot + "/Panels/ui_panel_header_9s.png", false);
            header.type = Image.Type.Sliced;
            Place(header.rectTransform, new Vector2(0.5f, 1), Center, new Vector2(0, -16), new Vector2(376, 87));
            var title = MakeText("Title", header.rectTransform, "Order", 34, Cream, TextAlignmentOptions.Center);
            Stretch(title.rectTransform);

            // Potion slot: dark frame with the potion icon.
            var slot = MakeRect("PotionSlot", rt);
            Place(slot, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -88), new Vector2(172, 166));
            var slotFill = MakeImage("Fill", slot, null, false);
            slotFill.sprite = null;
            slotFill.color = new Color(0.2f, 0.12f, 0.08f);
            Stretch(slotFill.rectTransform, 30, 30);
            Stretch(MakeImage("Frame", slot, UIArtRoot + "/Panels/ui_portrait_frame.png", false).rectTransform);
            var icon = MakeImage("PotionIcon", slot, ArtRoot + "/Items/Potions/icon_potion_calming.png", false);
            icon.preserveAspect = true;
            Place(icon.rectTransform, Center, Center, Vector2.zero, new Vector2(84, 106));

            var potionName = MakeText("PotionName", rt, "Calming Potion", 28, Brown, TextAlignmentOptions.Left);
            Place(potionName.rectTransform, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(233, -150), new Vector2(250, 40));
            potionName.textWrappingMode = TextWrappingModes.NoWrap;
            potionName.enableAutoSizing = true;
            potionName.fontSizeMin = 20;
            potionName.fontSizeMax = 28;
            var amount = MakeText("Amount", rt, "×1", 28, Brown, TextAlignmentOptions.Left);
            Place(amount.rectTransform, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(233, -190), new Vector2(250, 40));

            var have = InfoRow(rt, "Row_YouHave", "You have", "3", -290, false);
            var pays = InfoRow(rt, "Row_Pays", "Pays", "25", -345, true);

            var sell = MakeButton("Button_Primary", rt, "Button_Sell", "Sell", new Vector2(432, 82), 28);
            Place((RectTransform)sell.transform, new Vector2(0.5f, 1), Center, new Vector2(0, -428));
            var sorry = MakeButton("Button_Secondary", rt, "Button_Sorry", "Sorry, we're out", new Vector2(432, 82), 28);
            Place((RectTransform)sorry.transform, new Vector2(0.5f, 1), Center, new Vector2(0, -529));

            Set(panel, "potionIcon", icon);
            Set(panel, "potionNameText", potionName);
            Set(panel, "amountText", amount);
            Set(panel, "haveText", have);
            Set(panel, "paysText", pays);
            Set(panel, "sellButton", sell);
            Set(panel, "sorryButton", sorry);
            return panel;
        }

        /// <summary>Label on the left, value on the right (with a coin icon before it for payments). Returns the value text.</summary>
        private static TMP_Text InfoRow(RectTransform panel, string name, string label, string value, float y, bool coin)
        {
            var row = MakeRect(name, panel);
            row.anchorMin = new Vector2(0, 1);
            row.anchorMax = new Vector2(1, 1);
            row.pivot = new Vector2(0.5f, 0.5f);
            row.offsetMin = new Vector2(40, y - 22);
            row.offsetMax = new Vector2(-42, y + 22);

            var labelText = MakeText("Label", row, label, 26, Brown, TextAlignmentOptions.Left);
            Stretch(labelText.rectTransform);
            var valueText = MakeText("Value", row, value, 26, Brown, TextAlignmentOptions.Right);
            Place(valueText.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(90, 44));
            if (coin)
            {
                var icon = MakeImage("CoinIcon", row, UIArtRoot + "/Icons/ui_icon_coin.png", false);
                Place(icon.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(40, 40));
                valueText.rectTransform.sizeDelta = new Vector2(46, 44);
            }
            return valueText;
        }

        #endregion

        #region Helpers

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                if (scene.path == path) return;
            }
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
            {
                new EditorBuildSettingsScene(path, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
        }

        #endregion
    }
}
