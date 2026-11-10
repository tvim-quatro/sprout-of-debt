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
        public event Action IngredientAdded;
        public event Action Stirred;

        public RecipeDefinition CurrentRecipe { get; private set; }
        public int CurrentStepIndex { get; private set; }

        public bool IsBrewing => CurrentRecipe != null;

        public int AddedCount => addedCount;

        public float TotalStirProgress
        {
            get
            {
                if (!IsBrewing)
                {
                    return 0f;
                }

                float total = 0f;
                float done = 0f;
                for (int i = 0; i < CurrentRecipe.Steps.Count; i++)
                {
                    BrewingStep step = CurrentRecipe.Steps[i];
                    if (step.Type != BrewingStepType.Stir)
                    {
                        continue;
                    }
                    total += step.StirSeconds;
                    if (i < CurrentStepIndex)
                    {
                        done += step.StirSeconds;
                    }
                    else if (i == CurrentStepIndex)
                    {
                        done += Mathf.Min(stirredSeconds, step.StirSeconds);
                    }
                }
                return total > 0f ? Mathf.Clamp01(done / total) : 0f;
            }
        }

        public float StirProgress => IsCurrentStep(BrewingStepType.Stir) && CurrentStep.StirSeconds > 0f
            ? Mathf.Clamp01(stirredSeconds / CurrentStep.StirSeconds)
            : 0f;

        public BrewingStep CurrentStep => IsBrewing ? CurrentRecipe.Steps[CurrentStepIndex] : null;

        public bool CanBrew(RecipeDefinition recipe)
        {
            IInventory inventory = GameServices.Inventory;
            if (recipe == null || inventory == null)
            {
                return false;
            }

            foreach (KeyValuePair<IngredientData, int> pair in recipe.GetIngredientAmounts())
            {
                if (inventory.GetCount(pair.Key.id) < pair.Value)
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
            IngredientAdded?.Invoke();
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
            Stirred?.Invoke();
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
            return "Step " + (CurrentStepIndex + 1) + "/" + CurrentRecipe.Steps.Count + ": " + CurrentStep.Describe();
        }
    }
}
