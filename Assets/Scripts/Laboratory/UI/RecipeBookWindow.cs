using System.Collections.Generic;
using SproutOfDebt.Core;
using SproutOfDebt.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Laboratory.UI
{
    public class RecipeBookWindow : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private RecipeBookDefinition book;
        [SerializeField] private BrewingStation station;
        [Tooltip("Coins already paid to the Moneylender. Temporary until the debt is available from Core.")]
        [SerializeField, Min(0)] private int debtPaid;

        [Header("Pages")]
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject recipePage;
        [SerializeField] private GameObject lockedPage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text stepsText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text lockedText;
        [SerializeField] private TMP_Text pageText;
        [SerializeField] private IngredientSlotView[] ingredientSlots;

        [Header("Buttons")]
        [SerializeField] private Button brewButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;

        private int pageIndex;

        public bool IsOpen => panel != null && panel.activeSelf;

        private int PageCount => book != null ? book.Recipes.Count : 0;

        private RecipeDefinition CurrentRecipe => PageCount > 0 ? book.Recipes[pageIndex] : null;

        private void Awake()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (GameServices.Inventory != null)
            {
                GameServices.Inventory.ItemChanged -= OnItemChanged;
            }
        }

        public void Open()
        {
            if (panel == null || IsOpen)
            {
                return;
            }
            panel.SetActive(true);
            if (GameServices.Inventory != null)
            {
                GameServices.Inventory.ItemChanged += OnItemChanged;
            }
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }
            panel.SetActive(false);
            if (GameServices.Inventory != null)
            {
                GameServices.Inventory.ItemChanged -= OnItemChanged;
            }
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void ShowPage(int index)
        {
            if (PageCount == 0)
            {
                return;
            }
            pageIndex = Mathf.Clamp(index, 0, PageCount - 1);
            Refresh();
        }

        public void OnCloseClicked()
        {
            Close();
        }

        public void OnPreviousClicked()
        {
            ShowPage(pageIndex - 1);
        }

        public void OnNextClicked()
        {
            ShowPage(pageIndex + 1);
        }

        public void OnBrewClicked()
        {
            if (station != null && station.StartBrewing(CurrentRecipe))
            {
                Close();
            }
        }

        private bool IsUnlocked(RecipeDefinition recipe)
        {
            return recipe.UnlockAtDebtPaid <= debtPaid;
        }

        private void Refresh()
        {
            RecipeDefinition recipe = CurrentRecipe;
            if (recipe == null || recipe.Potion == null)
            {
                return;
            }

            bool unlocked = IsUnlocked(recipe);
            recipePage.SetActive(unlocked);
            lockedPage.SetActive(!unlocked);
            titleText.text = recipe.Potion.DisplayName;
            pageText.text = (pageIndex + 1) + "/" + PageCount;
            previousButton.interactable = pageIndex > 0;
            nextButton.interactable = pageIndex < PageCount - 1;

            if (unlocked)
            {
                ShowRecipe(recipe);
            }
            else
            {
                lockedText.text = "Unlocks at " + recipe.UnlockAtDebtPaid + " coins paid";
            }
        }

        private void ShowRecipe(RecipeDefinition recipe)
        {
            ShowIngredients(recipe);
            ShowSteps(recipe);
            priceText.text = recipe.Potion.Price.ToString();
            brewButton.interactable = station != null && !station.IsBrewing && station.CanBrew(recipe);
        }

        private void ShowIngredients(RecipeDefinition recipe)
        {
            int slot = 0;
            foreach (KeyValuePair<IngredientData, int> pair in recipe.GetIngredientAmounts())
            {
                if (slot >= ingredientSlots.Length)
                {
                    break;
                }
                ingredientSlots[slot].Show(pair.Key, GetCount(pair.Key), pair.Value);
                slot++;
            }

            for (; slot < ingredientSlots.Length; slot++)
            {
                ingredientSlots[slot].Hide();
            }
        }

        private void ShowSteps(RecipeDefinition recipe)
        {
            string text = "";
            for (int i = 0; i < recipe.Steps.Count; i++)
            {
                text += (i + 1) + ". " + recipe.Steps[i].Describe() + "\n";
            }
            stepsText.text = text;
        }

        private int GetCount(IngredientData ingredient)
        {
            if (GameServices.Inventory == null)
            {
                return 0;
            }
            return GameServices.Inventory.GetCount(ingredient.id);
        }

        private void OnItemChanged(string itemId, int count)
        {
            Refresh();
        }
    }
}
