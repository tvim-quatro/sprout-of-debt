using System;
using System.Collections.Generic;
using SproutOfDebt.Core;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class BrewingStation : MonoBehaviour
    {
        private const string LogPrefix = "[Laboratory] ";

        [SerializeField] private Brazier brazier;

        private int addedCount;
        private float stirredSeconds;

        public event Action StepChanged;
        public event Action PotionBrewed;

        public RecipeDefinition CurrentRecipe { get; private set; }
        public int CurrentStepIndex { get; private set; }

        public bool IsBrewing => CurrentRecipe != null;

        public BrewingStep CurrentStep => IsBrewing ? CurrentRecipe.Steps[CurrentStepIndex] : null;

        public bool CanBrew(RecipeDefinition recipe)
        {
            IInventory inventory = GameServices.Inventory;
            if (recipe == null || inventory == null)
            {
                return false;
            }

            Dictionary<string, int> needed = new Dictionary<string, int>();
            foreach (BrewingStep step in recipe.Steps)
            {
                if (step.Type != BrewingStepType.AddIngredient || step.Ingredient == null)
                {
                    continue;
                }
                needed.TryGetValue(step.Ingredient.id, out int sum);
                needed[step.Ingredient.id] = sum + step.Amount;
            }

            foreach (KeyValuePair<string, int> pair in needed)
            {
                if (inventory.GetCount(pair.Key) < pair.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public bool StartBrewing(RecipeDefinition recipe)
        {
            if (recipe == null || recipe.Potion == null || recipe.Steps.Count == 0)
            {
                Debug.LogWarning(LogPrefix + "Recipe is missing a potion or steps");
                return false;
            }
            if (IsBrewing)
            {
                Debug.Log(LogPrefix + "Already brewing " + CurrentRecipe.Potion.DisplayName);
                return false;
            }
            if (!CanBrew(recipe))
            {
                Debug.Log(LogPrefix + "Not enough ingredients for " + recipe.Potion.DisplayName);
                return false;
            }

            CurrentRecipe = recipe;
            CurrentStepIndex = 0;
            ResetStepProgress();
            Debug.Log(LogPrefix + "Start brewing " + recipe.Potion.DisplayName + ". " + DescribeCurrentStep());
            StepChanged?.Invoke();
            return true;
        }

        public bool LightFire()
        {
            if (!IsCurrentStep(BrewingStepType.LightFire))
            {
                return false;
            }
            if (brazier != null)
            {
                brazier.Light();
            }
            CompleteStep();
            return true;
        }

        public bool AddIngredient(IngredientData ingredient)
        {
            if (!IsCurrentStep(BrewingStepType.AddIngredient) || ingredient != CurrentStep.Ingredient)
            {
                return false;
            }
            if (GameServices.Inventory == null || !GameServices.Inventory.TryRemove(ingredient.id, 1))
            {
                return false;
            }

            addedCount++;
            Debug.Log(LogPrefix + "Added " + ingredient.displayName + " (" + addedCount + "/" + CurrentStep.Amount + ")");
            if (addedCount >= CurrentStep.Amount)
            {
                CompleteStep();
            }
            return true;
        }

        public bool Stir(float seconds)
        {
            if (!IsCurrentStep(BrewingStepType.Stir))
            {
                return false;
            }

            stirredSeconds += seconds;
            if (stirredSeconds >= CurrentStep.StirSeconds)
            {
                CompleteStep();
            }
            return true;
        }

        private bool IsCurrentStep(BrewingStepType type)
        {
            return IsBrewing && CurrentStep.Type == type;
        }

        private void ResetStepProgress()
        {
            addedCount = 0;
            stirredSeconds = 0f;
        }

        private void CompleteStep()
        {
            CurrentStepIndex++;
            ResetStepProgress();

            if (CurrentStepIndex >= CurrentRecipe.Steps.Count)
            {
                FinishBrewing();
                return;
            }
            Debug.Log(LogPrefix + DescribeCurrentStep());
            StepChanged?.Invoke();
        }

        private void FinishBrewing()
        {
            PotionDefinition potion = CurrentRecipe.Potion;
            if (GameServices.Inventory != null)
            {
                GameServices.Inventory.Add(potion.Id, 1);
            }
            Debug.Log(LogPrefix + "Brewed " + potion.DisplayName + "!");
            if (brazier != null)
            {
                brazier.Extinguish();
            }
            CurrentRecipe = null;
            StepChanged?.Invoke();
            PotionBrewed?.Invoke();
        }

        private string DescribeCurrentStep()
        {
            BrewingStep step = CurrentStep;
            string prefix = "Step " + (CurrentStepIndex + 1) + "/" + CurrentRecipe.Steps.Count + ": ";

            switch (step.Type)
            {
                case BrewingStepType.LightFire:
                    return prefix + "light the fire";
                case BrewingStepType.AddIngredient:
                    string ingredientName = step.Ingredient != null ? step.Ingredient.displayName : "?";
                    return prefix + "add " + step.Amount + " " + ingredientName;
                default:
                    return prefix + "stir for " + step.StirSeconds + " s";
            }
        }
    }
}
