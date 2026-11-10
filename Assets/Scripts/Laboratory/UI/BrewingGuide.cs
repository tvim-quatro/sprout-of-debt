using SproutOfDebt.Core;
using TMPro;
using UnityEngine;

namespace SproutOfDebt.Laboratory.UI
{
    public class BrewingGuide : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private RecipeStepView[] stepViews;

        private RecipeDefinition brewingRecipe;
        private bool guiding;

        private void Awake()
        {
            Hide();
        }

        private void OnEnable()
        {
            if (station == null)
            {
                return;
            }
            station.StepChanged += OnStepChanged;
            station.PotionBrewed += OnPotionBrewed;
            station.IngredientAdded += OnStepChanged;
        }

        private void OnDisable()
        {
            if (station == null)
            {
                return;
            }
            station.StepChanged -= OnStepChanged;
            station.PotionBrewed -= OnPotionBrewed;
            station.IngredientAdded -= OnStepChanged;
        }

        private void OnStepChanged()
        {
            if (!station.IsBrewing)
            {
                Hide();
                return;
            }

            if (brewingRecipe != station.CurrentRecipe)
            {
                brewingRecipe = station.CurrentRecipe;
                guiding = !WasBrewedBefore(brewingRecipe);
            }

            if (guiding)
            {
                Show(brewingRecipe, station.CurrentStepIndex);
            }
        }

        private void OnPotionBrewed()
        {
            if (brewingRecipe != null && brewingRecipe.Potion != null && GameServices.BrewedPotions != null)
            {
                GameServices.BrewedPotions.MarkBrewed(brewingRecipe.Potion.Id);
            }
            brewingRecipe = null;
            guiding = false;
            Hide();
        }

        private static bool WasBrewedBefore(RecipeDefinition recipe)
        {
            return recipe.Potion != null
                && GameServices.BrewedPotions != null
                && GameServices.BrewedPotions.HasBrewed(recipe.Potion.Id);
        }

        private void Show(RecipeDefinition recipe, int currentIndex)
        {
            if (titleText != null && recipe.Potion != null)
            {
                titleText.text = recipe.Potion.DisplayName;
            }

            for (int i = 0; i < stepViews.Length; i++)
            {
                if (stepViews[i] == null)
                {
                    continue;
                }
                if (i >= recipe.Steps.Count)
                {
                    stepViews[i].Hide();
                    continue;
                }
                string counter = i == currentIndex ? recipe.Steps[i].GuideCounter(station.AddedCount) : null;
                stepViews[i].Show(i + 1, recipe.Steps[i].DescribeForGuide(), counter);
                stepViews[i].SetState(i == currentIndex, i < currentIndex);
            }

            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        private void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
