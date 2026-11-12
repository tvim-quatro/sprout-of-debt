using System.Linq;
using SproutOfDebt.Laboratory.UI;
using SproutOfDebt.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using static SproutOfDebt.Laboratory.Editor.RecipeBookUIFactory;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class RecipeBookBuilder
    {
        private const string BookAssetPath = "Assets/Data/Recipes/RecipeBook_Laboratory.asset";
        private const string RecipesFolder = "Assets/Data/Recipes";
        private const string IconsRoot = "Assets/UI/Art/Icons";
        private const string SpreadPath = "Assets/Art/Laboratory/lab_recipe_book_spread.png";
        private const string SealPath = "Assets/Art/Laboratory/ui_book_seal.png";
        private const string CanvasName = "RecipeBookCanvas";
        private const string BookObjectName = "lab_book_closed";
        private const int SlotCount = 4;
        private const int StepCount = 6;

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

            LoadFont();
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
            FixedFrameCanvas.ConfigureScaler(scaler);
            // Content goes in the fixed 16:9 frame, like every other screen.
            return FixedFrameCanvas.CreateFrame((RectTransform)go.transform);
        }

        private static RecipeBookWindow BuildWindow(RectTransform canvas, RecipeBookDefinition book, BrewingStation station)
        {
            RectTransform root = MakeRect("RecipeBook", canvas);
            Stretch(root);
            RecipeBookWindow window = root.gameObject.AddComponent<RecipeBookWindow>();

            RectTransform panel = MakeRect("Panel", root);
            Stretch(panel);
            Image dimmer = MakeImage("Dimmer", panel, null);
            Stretch(dimmer.rectTransform);
            dimmer.color = new Color(0.05f, 0.02f, 0.01f, 0.6f);
            dimmer.raycastTarget = true;
            Button dimmerButton = dimmer.gameObject.AddComponent<Button>();
            dimmerButton.transition = Selectable.Transition.None;

            Image spread = MakeImage("Spread", panel, SpreadPath);
            spread.preserveAspect = false;
            spread.raycastTarget = true;
            Place(spread.rectTransform, Vector2.zero, new Vector2(1680, 960));
            RectTransform pages = spread.rectTransform;

            RectTransform left = MakeRect("LeftPage", pages);
            Place(left, new Vector2(-375, 40), new Vector2(600, 740));
            Vertical(left.gameObject, 28, TextAnchor.UpperLeft).childForceExpandWidth = true;
            TMP_Text title = MakeText("Title", left, "", 32, true, Dark, TextAlignmentOptions.Left);

            RectTransform recipePage = BuildRecipePage(left, window);
            RectTransform lockedPage = BuildLockedPage(left, window);

            RectTransform right = MakeRect("RightPage", pages);
            Place(right, new Vector2(385, 40), new Vector2(620, 740));
            Image potion = MakeImage("PotionImage", right, null);
            Place(potion.rectTransform, Vector2.zero, new Vector2(312, 444));

            TMP_Text pageText = MakeText("PageNumber", pages, "", 18, false, Dark, TextAlignmentOptions.Center);
            Place(pageText.rectTransform, new Vector2(385, -339), new Vector2(200, 30));

            Button previous = MakeIconButton(pages, "PreviousButton", "ui_icon_arrow_left.png", new Vector2(-823, -407));
            Button next = MakeIconButton(pages, "NextButton", "ui_icon_arrow_right.png", new Vector2(823, -407));
            Button close = MakeButton("Button_Close", pages, "CloseButton", null);
            Place((RectTransform)close.transform, new Vector2(823, 405), new Vector2(114, 117));

            UnityEventTools.AddPersistentListener(previous.onClick, window.OnPreviousClicked);
            UnityEventTools.AddPersistentListener(next.onClick, window.OnNextClicked);
            UnityEventTools.AddPersistentListener(close.onClick, window.OnCloseClicked);
            UnityEventTools.AddPersistentListener(dimmerButton.onClick, window.OnCloseClicked);

            Set(window, "book", book);
            Set(window, "station", station);
            Set(window, "panel", panel.gameObject);
            Set(window, "recipePage", recipePage.gameObject);
            Set(window, "lockedPage", lockedPage.gameObject);
            Set(window, "titleText", title);
            Set(window, "pageText", pageText);
            Set(window, "potionImage", potion);
            Set(window, "previousButton", previous);
            Set(window, "nextButton", next);

            panel.gameObject.SetActive(false);
            return window;
        }

        private static RectTransform BuildRecipePage(RectTransform parent, RecipeBookWindow window)
        {
            RectTransform page = MakeRect("RecipePage", parent);
            Vertical(page.gameObject, 28, TextAnchor.UpperLeft).childForceExpandWidth = true;
            Flexible(page, 1, 1);

            RectTransform ingredients = MakeRect("Ingredients", page);
            Vertical(ingredients.gameObject, 12, TextAnchor.UpperLeft);
            MakeText("Label", ingredients, "Ingredients", 26, true, Dark, TextAlignmentOptions.Left);
            RectTransform row = MakeRect("Row", ingredients);
            Horizontal(row.gameObject, 28, TextAnchor.UpperLeft);
            IngredientSlotView[] slots = new IngredientSlotView[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                slots[i] = BuildSlot(row);
            }
            TMP_Text missing = MakeText("MissingHint", ingredients, "Not enough ingredients", 26, false, Error, TextAlignmentOptions.Left);

            RectTransform steps = MakeRect("Steps", page);
            Vertical(steps.gameObject, 4, TextAnchor.UpperLeft).childForceExpandWidth = true;
            MakeText("Label", steps, "Steps", 26, true, Dark, TextAlignmentOptions.Left);
            RecipeStepView[] stepViews = new RecipeStepView[StepCount];
            for (int i = 0; i < StepCount; i++)
            {
                stepViews[i] = BuildStepRow(steps);
            }

            Flexible(MakeRect("Spacer", page), 0, 1);

            RectTransform footer = MakeRect("Footer", page);
            Horizontal(footer.gameObject, 10, TextAnchor.MiddleLeft);
            Size(MakeImage("CoinIcon", footer, IconsRoot + "/ui_icon_coin.png"), 44, 44);
            TMP_Text price = MakeText("Price", footer, "", 26, true, Dark, TextAlignmentOptions.Left);
            Flexible(MakeRect("Gap", footer), 1, 0);
            Button brew = MakeButton("Button_Primary", footer, "BrewButton", "Brew");
            Size(brew, 432, 87);
            UnityEventTools.AddPersistentListener(brew.onClick, window.OnBrewClicked);

            Set(window, "priceText", price);
            Set(window, "missingHint", missing.gameObject);
            Set(window, "brewButton", brew);
            Set(window, "brewLabel", brew.GetComponentInChildren<TMP_Text>());
            SetArray(window, "ingredientSlots", slots);
            SetArray(window, "stepViews", stepViews);
            return page;
        }

        private static RectTransform BuildLockedPage(RectTransform parent, RecipeBookWindow window)
        {
            RectTransform page = MakeRect("LockedPage", parent);
            VerticalLayoutGroup layout = Vertical(page.gameObject, 24, TextAnchor.UpperCenter);
            layout.padding = new RectOffset(0, 0, 120, 0);
            layout.childForceExpandWidth = true;
            Flexible(page, 1, 1);
            Image seal = MakeImage("Seal", page, SealPath);
            Size(seal, 168, 168);
            TMP_Text text = MakeText("LockedText", page, "", 26, false, Dark, TextAlignmentOptions.Center);
            Set(window, "lockedText", text);
            return page;
        }

        private static IngredientSlotView BuildSlot(RectTransform parent)
        {
            RectTransform slot = MakeRect("IngredientSlot", parent);
            Vertical(slot.gameObject, 6, TextAnchor.UpperCenter);
            Image icon = MakeImage("Icon", slot, null);
            Size(icon, 60, 76.5f);
            TMP_Text count = MakeText("Count", slot, "", 26, true, Dark, TextAlignmentOptions.Center);
            IngredientSlotView view = slot.gameObject.AddComponent<IngredientSlotView>();
            Set(view, "icon", icon);
            Set(view, "countText", count);
            return view;
        }

        private static RecipeStepView BuildStepRow(RectTransform parent)
        {
            RectTransform row = MakeRect("StepRow", parent);
            HorizontalLayoutGroup layout = Horizontal(row.gameObject, 14, TextAnchor.MiddleLeft);
            layout.padding = new RectOffset(10, 10, 6, 6);
            Size(row, -1, 40);
            TMP_Text number = MakeText("Number", row, "", 26, false, Dark, TextAlignmentOptions.Center);
            Size(number, 36, 36);
            TMP_Text text = MakeText("Text", row, "", 26, false, Dark, TextAlignmentOptions.Left);
            Flexible(text, 1, 0);
            RecipeStepView view = row.gameObject.AddComponent<RecipeStepView>();
            Set(view, "numberText", number);
            Set(view, "stepText", text);
            return view;
        }

        private static Button MakeIconButton(RectTransform parent, string name, string iconFile, Vector2 position)
        {
            Button button = MakeButton("Button_Icon", parent, name, null);
            Place((RectTransform)button.transform, position, new Vector2(114, 114));
            Transform icon = button.transform.Find("Icon");
            if (icon != null && icon.TryGetComponent(out Image image))
            {
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconsRoot + "/" + iconFile);
                image.color = Color.white;
                image.rectTransform.sizeDelta = new Vector2(62, 62);
            }
            return button;
        }
    }
}
