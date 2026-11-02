using System;
using SproutOfDebt.Items;

namespace SproutOfDebt.Laboratory
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
