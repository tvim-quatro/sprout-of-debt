using SproutOfDebt.Core;
using SproutOfDebt.Core.Save;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    /// <summary>
    /// Keeps the brew in progress in <see cref="GameServices.Progress"/> so it survives leaving the Laboratory and saving:
    /// it writes the brew after every change and puts it back into the station when the Laboratory opens again.
    /// </summary>
    public class BrewingProgressKeeper : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private RecipeBookDefinition book;

        private void OnEnable()
        {
            if (station == null)
            {
                return;
            }
            station.StepChanged += Save;
            station.IngredientAdded += Save;
            station.Stirred += Save;
            station.PotionBrewed += Save;
        }

        private void OnDisable()
        {
            if (station == null)
            {
                return;
            }
            station.StepChanged -= Save;
            station.IngredientAdded -= Save;
            station.Stirred -= Save;
            station.PotionBrewed -= Save;
        }

        private void Start()
        {
            Restore();
        }

        private void Restore()
        {
            BrewingProgress progress = GameServices.Progress.Brewing;
            if (station == null || book == null || !progress.Active)
            {
                return;
            }

            foreach (RecipeDefinition recipe in book.Recipes)
            {
                if (recipe != null && recipe.Potion != null && recipe.Potion.Id == progress.PotionId)
                {
                    station.Restore(recipe, progress.StepIndex, progress.AddedCount, progress.StirredSeconds);
                    return;
                }
            }
            progress.Clear();
        }

        private void Save()
        {
            BrewingProgress progress = GameServices.Progress.Brewing;
            if (station.IsBrewing)
            {
                progress.Set(station.CurrentRecipe.Potion.Id, station.CurrentStepIndex, station.AddedCount, station.StirredSeconds);
            }
            else
            {
                progress.Clear();
            }
        }
    }
}
