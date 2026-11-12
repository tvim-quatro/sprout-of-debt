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

        // key, shared asset name (Data/Potions/Potion_<asset>), display name, sell price, debt threshold that unlocks it
        private static readonly (string key, string asset, string name, int price, int threshold)[] Potions =
        {
            ("calming", "CalmingDraught", "Calming Draught", 10, 0),
            ("moonlight", "MoonlightElixir", "Moonlight Elixir", 18, 100),
            ("shadow", "ShadowTonic", "Shadow Tonic", 25, 250),
            ("ember", "EmberBrew", "Ember Brew", 40, 500),
            ("mandrake", "MandrakeRemedy", "Mandrake Remedy", 60, 750),
        };

        // plant key, display name, buy price, potion key it unlocks with
        private static readonly (string key, string name, int price, string potion)[] Seeds =
        {
            ("lavender", "Lavender", 3, "calming"),
            ("moonpetal", "Moonpetal", 5, "moonlight"),
            ("nightshade", "Nightshade", 8, "shadow"),
            ("emberroot", "Emberroot", 12, "ember"),
            ("mandrake", "Mandrake", 20, "mandrake"),
        };

        // key, display name, request lines (said after the story), thanks, disappointed,
        // stories in the order of Potions: calming, moonlight, shadow, ember, mandrake
        private static readonly (string key, string name, string[] request, string[] thanks, string[] sorry, string[] stories)[] Customers =
        {
            ("mage", "Apprentice Mage",
                new[] { "So, um... could I please get...", "C-can you help me? I need..." },
                new[] { "Thank you! Master will be so pleased!" },
                new[] { "Oh no... I'll be scolded again." },
                new[]
                {
                    "My final exam is tomorrow and my hands won't stop shaking. Last time I panicked, I turned my teacher's beard into moss. Never again!",
                    "Master wants me to copy star charts on the tower roof all night. I can barely see the ink by moonlight... literally.",
                    "Someone keeps stealing my spell notes. Tonight I'll hide in the library shadows and catch them red-handed!",
                    "I'm practising fire spells, but my sparks keep fizzling out. Master says a warm brew will help the flame catch.",
                    "I tried a levitation spell on myself and... landed on the roof. Well, through the roof. My ankle hurts so much.",
                }),
            ("fox", "Talking Fox",
                new[] { "So, shall we make a deal? I'll take...", "Here's what I'm after:" },
                new[] { "A pleasure doing business." },
                new[] { "Pity. I'll drop by another time." },
                new[]
                {
                    "My cousin is marrying into the henhouse family and the whole farm is in a panic. A calm neighbourhood is good for business, you see.",
                    "I travel at night to avoid the toll bridges. A sharp eye in the dark keeps my satchel full and my tail out of traps.",
                    "A certain badger owes me money and won't pay. I simply need to... visit him unseen. Purely for negotiations.",
                    "My den's chimney collapsed and the nights are getting frosty. A fox with cold paws makes very poor deals.",
                    "Got my tail caught in a hunter's snare last week. Escaped, of course, but it still aches when I sit down.",
                }),
            ("werewolf", "Werewolf",
                new[] { "S-so... could I please have...", "Would you... um... have..." },
                new[] { "Thanks... really. You're very kind." },
                new[] { "Oh... that's okay. Don't worry." },
                new[]
                {
                    "The full moon is in three days and I get so nervous... Last time I howled all night and woke the whole village.",
                    "When I change, everything goes blurry and I bump into trees. The neighbours think it's funny, but it really hurts.",
                    "I'm too big and fluffy, everyone stares at me at the market. I just want to buy turnips in peace...",
                    "My flannel shirts tear every time I change, and the cold gets in. A warm brew helps me sleep through the winter.",
                    "I chased my own tail again and sprained my paw. Please don't tell anyone. It's... a little embarrassing.",
                }),
            ("witch", "Old Witch",
                new[] { "So, be a dear and fetch me...", "Now then, I'll be needing..." },
                new[] { "Bless your horns, little one." },
                new[] { "Never mind, dearie. Next time." },
                new[]
                {
                    "My grandchildren visit this week. All seven of them. At once. I'll need something to keep my nerves in one piece, dearie.",
                    "My old eyes can't read recipe books after sunset anymore. And the best brewing happens at night, as you well know.",
                    "The village children keep sneaking into my garden to see if I'm a real witch. I'd like to give them a kind little scare.",
                    "My cauldron fire went out for the first time in sixty years. It's a family heirloom, dearie, it mustn't go cold.",
                    "My back creaks louder than my broomstick these days. One sip of mandrake and I'll be flying again.",
                }),
            ("sprout", "Forest Spirit",
                new[] { "*rustle* ...could you spare...", "*hopeful chime* ...please..." },
                new[] { "*happy rustling*" },
                new[] { "*sad, quiet rustling*" },
                new[]
                {
                    "*rustle* ...the old oak is restless... loud woodcutters came by... a calming drop at its roots will help it sleep...",
                    "*soft chime* ...the fireflies flew south early... the little mushrooms are lost in the dark without their light...",
                    "*rustle* ...a hungry deer keeps nibbling my moss... I wish to hide in the shade until it wanders away...",
                    "*shiver* ...the first frost bit my leaves... a little warmth, please... before the snow comes...",
                    "*creak* ...a storm broke a branch of the old birch... mandrake heals bark as well as skin...",
                }),
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

            var services = new GameObject("Services", typeof(StubServicesInstaller));
            SetStartingItems(services.GetComponent<StubServicesInstaller>(),
                ("potion_calming", 6), ("seed_lavender", 3),
                ("lavender", 10), ("moonpetal", 5), ("nightshade", 5), ("emberroot", 5), ("mandrake", 5));

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
                    a.displayName = c.name;
                    a.neutral = LoadSprite(path + "neutral.png");
                    a.happy = LoadSprite(path + "happy.png");
                    a.disappointed = LoadSprite(path + "disappointed.png");
                    a.thanksLines = c.thanks;
                    a.disappointedLines = c.sorry;
                });

                // Customers created before stories existed get them (and the matching request lines) once.
                if (customer.stories.Count == 0)
                {
                    customer.requestLines = c.request;
                    for (int i = 0; i < Potions.Length && i < c.stories.Length; i++)
                    {
                        customer.stories.Add(new PotionStory
                        {
                            potion = AssetDatabase.LoadAssetAtPath<PotionDefinition>(PotionPath(Potions[i].key)),
                            lines = new[] { c.stories[i] },
                        });
                    }
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

            var customerImage = MakeImage("Customer", location, ArtRoot + "/Characters/Customers/char_customer_fox_neutral.png");
            var customerRt = customerImage.rectTransform;
            Place(customerRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-300, 150));
            customerRt.gameObject.AddComponent<CanvasGroup>();
            var customerView = customerRt.gameObject.AddComponent<CustomerView>();
            Set(customerView, "image", customerImage);

            var counter = MakeImage("CounterFront", location, ArtRoot + "/Backgrounds/counter_front.png");
            Place(counter.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero);

            var pile = MakeImage("CoinPile", location, ArtRoot + "/Effects/counter_coins_pile.png");
            Place(pile.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-40, 185));
            pile.gameObject.SetActive(false);

            var dialogue = BuildCustomerDialogue(location);

            // Coins fly from the customer to the counter, where a coin pile appears.
            var fxLayer = MakeRect("CoinFx", location);
            Stretch(fxLayer);
            var burst = fxLayer.gameObject.AddComponent<CoinBurst>();
            Set(burst, "target", pile.rectTransform);
            Set(burst, "coinPile", pile);
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
            Set(controller, "coinBurst", burst);
        }

        private static CustomerDialogue BuildCustomerDialogue(RectTransform parent)
        {
            // Dialogue window to the right of the customer.
            var box = MakeImage("CustomerDialogue", parent, UIArtRoot + "/Panels/ui_dialog_box_9s.png", false);
            box.type = UnityEngine.UI.Image.Type.Sliced;
            box.raycastTarget = true;
            var boxRt = box.rectTransform;
            Place(boxRt, Center, Center, new Vector2(400, 150), new Vector2(760, 330));
            var dialogue = box.gameObject.AddComponent<CustomerDialogue>();
            var advance = box.gameObject.AddComponent<Button>();
            advance.transition = Selectable.Transition.None;

            var nameplate = MakeImage("Nameplate", boxRt, UIArtRoot + "/Panels/ui_dialog_nameplate.png");
            Place(nameplate.rectTransform, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(36, 0), new Vector2(290, 75));
            var speaker = MakeText("SpeakerName", nameplate.rectTransform, "Old Witch", 28, Cream, TextAlignmentOptions.Center);
            Stretch(speaker.rectTransform);

            var body = MakeText("Body", boxRt, "My grandchildren visit this week. All seven of them. At once. I'll need something to keep my nerves in one piece, dearie.", 28, Brown, TextAlignmentOptions.TopLeft);
            var bodyRt = body.rectTransform;
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(40, 30);
            bodyRt.offsetMax = new Vector2(-40, -54);

            var next = MakeButton("Button_Round", boxRt, "Button_Next", null, new Vector2(72, 72), 0);
            Place((RectTransform)next.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 18));
            var arrow = next.transform.Find("Icon").GetComponent<Image>();
            arrow.sprite = LoadSprite(UIArtRoot + "/Icons/ui_icon_arrow_right.png");
            arrow.color = Color.white;
            arrow.preserveAspect = true;
            arrow.rectTransform.sizeDelta = new Vector2(42, 42);

            // Order row at the bottom of the window.
            var row = MakeRect("Request", boxRt);
            Place(row, Vector2.zero, Vector2.zero, new Vector2(40, 26), new Vector2(680, 110));
            var potionIcon = MakeImage("PotionIcon", row, ArtRoot + "/Items/Potions/icon_potion_calming.png", false);
            potionIcon.preserveAspect = true;
            Place(potionIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(72, 102));
            var request = MakeText("RequestText", row, "2 × Calming Draught", 32, Brown, TextAlignmentOptions.BottomLeft);
            Place(request.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(90, 2), new Vector2(380, 44));
            var stock = MakeText("StockText", row, "In stock: 6", 24, Brown, TextAlignmentOptions.TopLeft);
            Place(stock.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 1), new Vector2(90, -4), new Vector2(380, 34));
            var price = PriceTag("PriceTag", row, "20", new Vector2(190, 71));
            Place((RectTransform)price.transform.parent.parent, new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero);

            // Answers right under the window.
            var answers = MakeRect("Answers", boxRt);
            Place(answers, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(720, 84));
            var sell = MakeButton("Button_Primary", answers, "Button_Sell", "SELL", new Vector2(280, 84), 30);
            Place((RectTransform)sell.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero);
            var sorry = MakeButton("Button_Secondary", answers, "Button_Sorry", "SORRY, WE'RE OUT", new Vector2(420, 84), 28);
            Place((RectTransform)sorry.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero);

            Set(dialogue, "speakerText", speaker);
            Set(dialogue, "bodyText", body);
            Set(dialogue, "advanceArea", advance);
            Set(dialogue, "nextButton", next);
            Set(dialogue, "requestRow", row.gameObject);
            Set(dialogue, "potionIcon", potionIcon);
            Set(dialogue, "requestText", request);
            Set(dialogue, "priceText", price);
            Set(dialogue, "stockText", stock);
            Set(dialogue, "answers", answers.gameObject);
            Set(dialogue, "sellButton", sell);
            Set(dialogue, "sorryButton", sorry);
            return dialogue;
        }

        #endregion

        #region Helpers

        private static void SetStartingItems(StubServicesInstaller installer, params (string id, int count)[] items)
        {
            var so = new SerializedObject(installer);
            var property = so.FindProperty("startingItems");
            property.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("itemId").stringValue = items[i].id;
                element.FindPropertyRelative("count").intValue = items[i].count;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

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
