using System.Linq;
using SproutOfDebt.Laboratory.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class RecipeBookBuilder
    {
        private const string BookAssetPath = "Assets/Data/Recipes/RecipeBook_Laboratory.asset";
        private const string RecipesFolder = "Assets/Data/Recipes";
        private const string FontPath = "Assets/UI/Fonts/PixelifySans_FontAsset.asset";
        private const string ButtonsRoot = "Assets/UI/Prefabs/Buttons";
        private const string IconsRoot = "Assets/UI/Art/Icons";
        private const string SpreadPath = "Assets/Art/Laboratory/lab_recipe_book_spread.png";
        private const string CanvasName = "RecipeBookCanvas";
        private const string BookObjectName = "lab_book_closed";
        private const int SlotCount = 3;
        private const float PageX = 410f;

        private static readonly Color Brown = new Color(0.24f, 0.15f, 0.09f);

        private static TMP_FontAsset font;

        [MenuItem("Tools/Laboratory/Build Recipe Book")]
        public static void Build()
        {
            BrewingStation station = Object.FindAnyObjectByType<BrewingStation>();
            GameObject bookObject = GameObject.Find(BookObjectName);
            if (station == null || bookObject == null)
            {
                Debug.LogError("[RecipeBookBuilder] Open a Laboratory scene with a BrewingStation and " + BookObjectName);
                return;
            }

            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            RecipeBookDefinition book = EnsureBookAsset();
            EnsureEventSystem();
            if (!bookObject.TryGetComponent(out Collider2D bookCollider))
            {
                bookCollider = bookObject.AddComponent<BoxCollider2D>();
            }

            GameObject oldCanvas = GameObject.Find(CanvasName);
            if (oldCanvas != null)
            {
                Object.DestroyImmediate(oldCanvas);
            }

            RectTransform canvas = CreateCanvas();
            RecipeBookWindow window = BuildWindow(canvas, book, station);
            RecipeBookInput input = canvas.gameObject.AddComponent<RecipeBookInput>();
            Set(input, "window", window);
            Set(input, "bookCollider", bookCollider);

            EditorSceneManager.MarkSceneDirty(station.gameObject.scene);
            EditorSceneManager.SaveScene(station.gameObject.scene);
            Debug.Log("[RecipeBookBuilder] Recipe book built");
        }

        private static RecipeBookDefinition EnsureBookAsset()
        {
            RecipeBookDefinition book = AssetDatabase.LoadAssetAtPath<RecipeBookDefinition>(BookAssetPath);
            if (book != null)
            {
                return book;
            }

            book = ScriptableObject.CreateInstance<RecipeBookDefinition>();
            RecipeDefinition[] recipes = AssetDatabase.FindAssets("t:RecipeDefinition", new[] { RecipesFolder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<RecipeDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .OrderBy(recipe => recipe.UnlockAtDebtPaid)
                .ToArray();
            AssetDatabase.CreateAsset(book, BookAssetPath);
            SetArray(book, "recipes", recipes);
            AssetDatabase.SaveAssets();
            return book;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        private static RectTransform CreateCanvas()
        {
            GameObject go = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return (RectTransform)go.transform;
        }

        private static RecipeBookWindow BuildWindow(RectTransform canvas, RecipeBookDefinition book, BrewingStation station)
        {
            RectTransform root = MakeRect("RecipeBook", canvas);
            Stretch(root);
            RecipeBookWindow window = root.gameObject.AddComponent<RecipeBookWindow>();

            RectTransform panel = MakeRect("Panel", root);
            Stretch(panel);
            Image dimmer = MakeImage("Dimmer", panel, null, Vector2.zero, Vector2.zero);
            Stretch(dimmer.rectTransform);
            dimmer.color = new Color(0.05f, 0.02f, 0.01f, 0.6f);
            dimmer.raycastTarget = true;

            Image spread = MakeImage("Spread", panel, SpreadPath, Vector2.zero, new Vector2(1680, 960));
            spread.raycastTarget = true;
            RectTransform pages = spread.rectTransform;

            TMP_Text title = MakeText("Title", pages, "", 56, new Vector2(-PageX, 330), new Vector2(600, 80), TextAlignmentOptions.Center);
            TMP_Text pageText = MakeText("PageNumber", pages, "", 32, new Vector2(0, -385), new Vector2(160, 50), TextAlignmentOptions.Center);

            RectTransform recipePage = MakeRect("RecipePage", pages);
            Stretch(recipePage);
            IngredientSlotView[] slots = new IngredientSlotView[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                slots[i] = BuildSlot(recipePage, new Vector2(-PageX + (i - 1) * 190f, 120f));
            }
            TMP_Text steps = MakeText("Steps", recipePage, "", 38, new Vector2(PageX, 120), new Vector2(580, 400), TextAlignmentOptions.TopLeft);
            MakeImage("CoinIcon", recipePage, IconsRoot + "/ui_icon_coin.png", new Vector2(PageX - 110, -220), new Vector2(64, 64));
            TMP_Text price = MakeText("Price", recipePage, "", 44, new Vector2(PageX + 40, -220), new Vector2(200, 64), TextAlignmentOptions.Left);
            Button brew = MakeButton("Button_Primary", recipePage, "BrewButton", "Brew", new Vector2(PageX, -320), new Vector2(300, 90));

            RectTransform lockedPage = MakeRect("LockedPage", pages);
            Stretch(lockedPage);
            MakeImage("LockIcon", lockedPage, IconsRoot + "/ui_icon_lock.png", new Vector2(-PageX, 100), new Vector2(140, 140));
            TMP_Text lockedText = MakeText("LockedText", lockedPage, "", 40, new Vector2(-PageX, -60), new Vector2(560, 120), TextAlignmentOptions.Center);

            Button previous = MakeIconButton(pages, "PreviousButton", "ui_icon_arrow_left.png", new Vector2(-700, -400));
            Button next = MakeIconButton(pages, "NextButton", "ui_icon_arrow_right.png", new Vector2(700, -400));
            Button close = MakeButton("Button_Close", pages, "CloseButton", null, new Vector2(800, 440), new Vector2(68, 70));

            UnityEventTools.AddPersistentListener(brew.onClick, window.OnBrewClicked);
            UnityEventTools.AddPersistentListener(previous.onClick, window.OnPreviousClicked);
            UnityEventTools.AddPersistentListener(next.onClick, window.OnNextClicked);
            UnityEventTools.AddPersistentListener(close.onClick, window.OnCloseClicked);
            Button dimmerButton = dimmer.gameObject.AddComponent<Button>();
            dimmerButton.transition = Selectable.Transition.None;
            UnityEventTools.AddPersistentListener(dimmerButton.onClick, window.OnCloseClicked);

            Set(window, "book", book);
            Set(window, "station", station);
            Set(window, "panel", panel.gameObject);
            Set(window, "recipePage", recipePage.gameObject);
            Set(window, "lockedPage", lockedPage.gameObject);
            Set(window, "titleText", title);
            Set(window, "stepsText", steps);
            Set(window, "priceText", price);
            Set(window, "lockedText", lockedText);
            Set(window, "pageText", pageText);
            Set(window, "brewButton", brew);
            Set(window, "previousButton", previous);
            Set(window, "nextButton", next);
            SetArray(window, "ingredientSlots", slots);

            panel.gameObject.SetActive(false);
            return window;
        }

        private static IngredientSlotView BuildSlot(RectTransform parent, Vector2 position)
        {
            RectTransform slot = MakeRect("IngredientSlot", parent);
            Place(slot, position, new Vector2(160, 200));
            Image icon = MakeImage("Icon", slot, null, new Vector2(0, 30), new Vector2(120, 120));
            icon.preserveAspect = true;
            TMP_Text count = MakeText("Count", slot, "", 40, new Vector2(0, -70), new Vector2(160, 50), TextAlignmentOptions.Center);
            IngredientSlotView view = slot.gameObject.AddComponent<IngredientSlotView>();
            Set(view, "icon", icon);
            Set(view, "countText", count);
            return view;
        }

        private static Button MakeIconButton(RectTransform parent, string name, string iconFile, Vector2 position)
        {
            Button button = MakeButton("Button_Icon", parent, name, null, position, new Vector2(90, 90));
            Transform icon = button.transform.Find("Icon");
            if (icon != null && icon.TryGetComponent(out Image image))
            {
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconsRoot + "/" + iconFile);
                image.color = Color.white;
                image.rectTransform.sizeDelta = new Vector2(56, 56);
            }
            return button;
        }

        private static Button MakeButton(string prefabName, RectTransform parent, string name, string label, Vector2 position, Vector2 size)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonsRoot + "/" + prefabName + ".prefab");
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            Place((RectTransform)go.transform, position, size);
            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            if (text != null && label != null)
            {
                text.text = label;
            }
            return go.GetComponent<Button>();
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Image MakeImage(string name, Transform parent, string spritePath, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            if (spritePath != null)
            {
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }
            Place(image.rectTransform, position, size);
            return image;
        }

        private static TMP_Text MakeText(string name, Transform parent, string value, float size, Vector2 position, Vector2 box, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            if (font != null)
            {
                text.font = font;
            }
            text.text = value;
            text.fontSize = size;
            text.color = Brown;
            text.alignment = alignment;
            text.raycastTarget = false;
            Place(text.rectTransform, position, box);
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Set(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(Object target, string field, Object[] values)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
