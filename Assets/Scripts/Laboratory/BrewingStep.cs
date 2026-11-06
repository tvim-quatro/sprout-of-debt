using System;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    [Serializable]
    public class BrewingStep
    {
        [SerializeField] private BrewingStepType type;
        [SerializeField] private IngredientData ingredient;
        [SerializeField, Min(1)] private int amount = 1;
        [SerializeField, Min(0f)] private float stirSeconds = 3f;

        public BrewingStepType Type => type;
        public IngredientData Ingredient => ingredient;
        public int Amount => amount;
        public float StirSeconds => stirSeconds;

        public string Describe()
        {
            switch (type)
            {
                case BrewingStepType.LightFire:
                    return "Light the fire";
                case BrewingStepType.AddIngredient:
                    string ingredientName = ingredient != null ? ingredient.displayName : "?";
                    return "Add " + amount + " " + ingredientName;
                default:
                    return "Stir for " + stirSeconds + " s";
            }
        }

        public string GuideCounter(int addedCount)
        {
            return type == BrewingStepType.AddIngredient ? addedCount + "/" + amount : null;
        }

        public string DescribeForGuide()
        {
            switch (type)
            {
                case BrewingStepType.LightFire:
                    return "Light the fire with the candle";
                case BrewingStepType.AddIngredient:
                    string ingredientName = ingredient != null ? ingredient.displayName.ToLowerInvariant() : "?";
                    return "Add " + amount + " " + ingredientName + " to the bowl";
                default:
                    return "Stir with the spoon";
            }
        }
    }
}
