using System;
using DrugCrafter.Items;

namespace DrugCrafter.Laboratory
{
    [Serializable]
    public class BrewingStep
    {
        public BrewingStepType type;
        public IngredientData ingredient;
        public int amount = 1;
        public float stirSeconds = 3f;
    }
}
