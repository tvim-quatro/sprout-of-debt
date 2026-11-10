#if UNITY_EDITOR || DEVELOPMENT_BUILD
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class LabDebugActions : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private RecipeDefinition recipe;
        [SerializeField] private IngredientData ingredient;

        [ContextMenu("1. Start Brewing")]
        private void StartBrewing()
        {
            if (station != null)
            {
                station.StartBrewing(recipe);
            }
        }

        [ContextMenu("2. Light Fire")]
        private void LightFire()
        {
            Report(station != null && station.LightFire());
        }

        [ContextMenu("3. Add Ingredient")]
        private void AddIngredient()
        {
            Report(station != null && station.AddIngredient(ingredient));
        }

        [ContextMenu("4. Stir 1 Second")]
        private void Stir()
        {
            Report(station != null && station.Stir(1f));
        }

        private void Report(bool success)
        {
            if (!success)
            {
                Debug.Log("[Laboratory] This action does not fit the current step");
            }
        }
    }
}
#endif
