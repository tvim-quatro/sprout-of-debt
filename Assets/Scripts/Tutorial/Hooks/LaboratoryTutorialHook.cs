using SproutOfDebt.Core;
using SproutOfDebt.Laboratory;
using SproutOfDebt.Laboratory.UI;
using SproutOfDebt.Shop.UI;
using UnityEngine;

namespace SproutOfDebt.Tutorial.Hooks
{
    /// <summary>
    /// Monitors player actions in the Laboratory and advances tutorial steps.
    /// </summary>
    public class LaboratoryTutorialHook : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private RecipeBookWindow recipeBook;
        [SerializeField] private Brazier brazier;

        private void Start()
        {
            if (station == null) station = FindFirstObjectByType<BrewingStation>();
            if (recipeBook == null) recipeBook = FindFirstObjectByType<RecipeBookWindow>();
            if (brazier == null) brazier = FindFirstObjectByType<Brazier>();

            if (station != null)
            {
                station.StepChanged += OnStationStepChanged;
                station.PotionBrewed += OnPotionBrewed;
                station.IngredientAdded += OnIngredientAdded;
            }

            EnsureRequiredIngredients();

            // If entering Laboratory during GardenGoToLab, automatically advance to LabOpenRecipe
            if (GameServices.Tutorial != null && GameServices.Tutorial.CurrentStep == TutorialStep.GardenGoToLab)
            {
                GameServices.Tutorial.SetStep(TutorialStep.LabOpenRecipe);
            }
        }

        private void OnDestroy()
        {
            if (station != null)
            {
                station.StepChanged -= OnStationStepChanged;
                station.PotionBrewed -= OnPotionBrewed;
                station.IngredientAdded -= OnIngredientAdded;
            }
        }

        private void Update()
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted)
            {
                return;
            }

            TutorialStep step = GameServices.Tutorial.CurrentStep;

            if (step == TutorialStep.LabOpenRecipe)
            {
                EnsureRequiredIngredients();

                if (recipeBook != null && recipeBook.IsOpen)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.LabOpenRecipe);
                }
                else if (station != null && station.IsBrewing)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.LabLightFire);
                }
            }
            else if (step == TutorialStep.LabClickBrew)
            {
                EnsureRequiredIngredients();

                if (station != null && station.IsBrewing)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.LabClickBrew);
                }
                else if (recipeBook != null && !recipeBook.IsOpen)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.LabOpenRecipe);
                }
            }
            else if (step == TutorialStep.LabLightFire)
            {
                if (brazier != null && brazier.IsLit)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.LabLightFire);
                }
            }
            else if (step == TutorialStep.LabOpenInventory || step == TutorialStep.LabPickIngredient || step == TutorialStep.LabAddIngredients)
            {
                UpdateIngredientSteps(step);
            }
            else if (step == TutorialStep.LabStir)
            {
                if (station != null && !station.IsBrewing)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.LabPotionReady);
                }
            }
        }

        private void UpdateIngredientSteps(TutorialStep step)
        {
            bool needsIngredient = station != null && station.CurrentStep != null && station.CurrentStep.Type == BrewingStepType.AddIngredient;
            bool holdingIngredient = FindFirstObjectByType<HeldIngredient>() != null;
            var inventory = FindFirstObjectByType<InventoryWindow>();
            bool inventoryOpen = inventory != null && inventory.IsOpen;

            // Every ingredient goes through the same loop: open the bag, pick the highlighted ingredient, drop it into the bowl.
            if (!needsIngredient)
            {
                GameServices.Tutorial.SetStep(TutorialStep.LabStir);
            }
            else if (step == TutorialStep.LabOpenInventory)
            {
                if (holdingIngredient)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.LabAddIngredients);
                }
                else if (inventoryOpen)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.LabOpenInventory);
                }
            }
            else if (step == TutorialStep.LabPickIngredient)
            {
                if (holdingIngredient)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.LabPickIngredient);
                }
                else if (!inventoryOpen)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.LabOpenInventory);
                }
            }
            else if (!holdingIngredient)
            {
                GameServices.Tutorial.SetStep(inventoryOpen ? TutorialStep.LabPickIngredient : TutorialStep.LabOpenInventory);
            }
        }

        private void OnIngredientAdded()
        {
            CheckIngredientsStep();
        }

        private void OnStationStepChanged()
        {
            CheckIngredientsStep();
        }

        private void CheckIngredientsStep()
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) return;

            if (GameServices.Tutorial.CurrentStep == TutorialStep.LabAddIngredients)
            {
                if (station != null && station.CurrentStep != null && station.CurrentStep.Type == BrewingStepType.Stir)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.LabAddIngredients);
                }
            }
        }

        private void OnPotionBrewed()
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) return;

            // According to Figma onboarding: the customer asks for one Calming Potion and the player has exactly one
            if (GameServices.Inventory != null && GameServices.Inventory.GetCount("potion_calming") < 1)
            {
                GameServices.Inventory.Add("potion_calming", 1 - GameServices.Inventory.GetCount("potion_calming"));
            }

            if (GameServices.Tutorial.CurrentStep <= TutorialStep.LabStir)
            {
                GameServices.Tutorial.SetStep(TutorialStep.LabPotionReady);
            }
        }

        private void EnsureRequiredIngredients()
        {
            if (GameServices.Tutorial != null && !GameServices.Tutorial.IsCompleted)
            {
                if (GameServices.Inventory != null)
                {
                    if (GameServices.Inventory.GetCount("lavender") < 2)
                    {
                        GameServices.Inventory.Add("lavender", 2 - GameServices.Inventory.GetCount("lavender"));
                    }
                    if (GameServices.Inventory.GetCount("moonpetal") < 1)
                    {
                        GameServices.Inventory.Add("moonpetal", 1 - GameServices.Inventory.GetCount("moonpetal"));
                    }
                }
            }
        }
    }
}
