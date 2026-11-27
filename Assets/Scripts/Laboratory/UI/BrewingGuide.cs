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
            station.StepChanged += Refresh;
            station.IngredientAdded += Refresh;
        }

        private void OnDisable()
        {
            if (station == null)
            {
                return;
            }
            station.StepChanged -= Refresh;
            station.IngredientAdded -= Refresh;
        }

        private void Refresh()
        {
            if (station.IsBrewing)
            {
                Show(station.CurrentRecipe, station.CurrentStepIndex);
            }
            else
            {
                Hide();
            }
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
