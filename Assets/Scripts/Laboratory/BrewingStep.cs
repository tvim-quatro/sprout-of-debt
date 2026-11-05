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
    }
}
