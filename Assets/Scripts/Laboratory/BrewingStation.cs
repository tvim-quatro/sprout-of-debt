using System.Collections.Generic;
using DrugCrafter.Items;
using UnityEngine;

namespace DrugCrafter.Laboratory
{
    public class BrewingStation : MonoBehaviour
    {
        public TempInventory inventory;
        public Brazier brazier;

        private int addedCount;
        private float stirredSeconds;

        public RecipeData CurrentRecipe { get; private set; }
        public int CurrentStepIndex { get; private set; }

        public bool IsBrewing
        {
            get { return CurrentRecipe != null; }
        }

        public BrewingStep CurrentStep
        {
            get
            {
                if (!IsBrewing)
                {
                    return null;
                }
                return CurrentRecipe.steps[CurrentStepIndex];
            }
        }

        public bool CanBrew(RecipeData recipe)
        {
            Dictionary<IngredientData, int> needed = new Dictionary<IngredientData, int>();
            foreach (BrewingStep step in recipe.steps)
            {
                if (step.type != BrewingStepType.AddIngredient || step.ingredient == null)
                {
                    continue;
                }
                needed.TryGetValue(step.ingredient, out int sum);
                needed[step.ingredient] = sum + step.amount;
            }

            foreach (KeyValuePair<IngredientData, int> pair in needed)
            {
                if (inventory.GetCount(pair.Key) < pair.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public bool StartBrewing(RecipeData recipe)
        {
            if (IsBrewing)
            {
                Debug.Log("Already brewing " + CurrentRecipe.potion.displayName);
                return false;
            }
            if (!CanBrew(recipe))
            {
                Debug.Log("Not enough ingredients for " + recipe.potion.displayName);
                return false;
            }

            CurrentRecipe = recipe;
            CurrentStepIndex = 0;
            ResetStepProgress();
            Debug.Log("Start brewing " + recipe.potion.displayName + ". " + DescribeCurrentStep());
            return true;
        }

        public bool LightFire()
        {
            if (!IsCurrentStep(BrewingStepType.LightFire))
            {
                return false;
            }
            brazier.Light();
            CompleteStep();
            return true;
        }

        public bool AddIngredient(IngredientData ingredient)
        {
            if (!IsCurrentStep(BrewingStepType.AddIngredient) || ingredient != CurrentStep.ingredient)
            {
                return false;
            }
            if (!inventory.Remove(ingredient, 1))
            {
                return false;
            }

            addedCount++;
            Debug.Log("Added " + ingredient.displayName + " (" + addedCount + "/" + CurrentStep.amount + ")");
            if (addedCount >= CurrentStep.amount)
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
            if (stirredSeconds >= CurrentStep.stirSeconds)
            {
                CompleteStep();
            }
            return true;
        }

        private bool IsCurrentStep(BrewingStepType type)
        {
            return IsBrewing && CurrentStep.type == type;
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

            if (CurrentStepIndex >= CurrentRecipe.steps.Count)
            {
                FinishBrewing();
                return;
            }
            Debug.Log(DescribeCurrentStep());
        }

        private void FinishBrewing()
        {
            inventory.Add(CurrentRecipe.potion, 1);
            Debug.Log("Brewed " + CurrentRecipe.potion.displayName + "!");
            brazier.Extinguish();
            CurrentRecipe = null;
        }

        private string DescribeCurrentStep()
        {
            BrewingStep step = CurrentStep;
            string prefix = "Step " + (CurrentStepIndex + 1) + "/" + CurrentRecipe.steps.Count + ": ";

            switch (step.type)
            {
                case BrewingStepType.LightFire:
                    return prefix + "light the fire";
                case BrewingStepType.AddIngredient:
                    return prefix + "add " + step.amount + " " + step.ingredient.displayName;
                default:
                    return prefix + "stir for " + step.stirSeconds + " s";
            }
        }
    }
}
