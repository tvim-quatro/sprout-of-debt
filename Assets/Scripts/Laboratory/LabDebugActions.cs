using DrugCrafter.Items;
using UnityEngine;

namespace DrugCrafter.Laboratory
{
    public class LabDebugActions : MonoBehaviour
    {
        public BrewingStation station;
        public RecipeData recipe;
        public IngredientData ingredient;

        [ContextMenu("1. Start Brewing")]
        private void StartBrewing()
        {
            station.StartBrewing(recipe);
        }

        [ContextMenu("2. Light Fire")]
        private void LightFire()
        {
            Report(station.LightFire());
        }

        [ContextMenu("3. Add Ingredient")]
        private void AddIngredient()
        {
            Report(station.AddIngredient(ingredient));
        }

        [ContextMenu("4. Stir 1 Second")]
        private void Stir()
        {
            Report(station.Stir(1f));
        }

        private void Report(bool success)
        {
            if (!success)
            {
                Debug.Log("This action does not fit the current step");
            }
        }
    }
}
