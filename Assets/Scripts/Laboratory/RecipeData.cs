using System.Collections.Generic;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    [CreateAssetMenu(fileName = "Recipe_", menuName = "Sprout of Debt/Recipe")]
    public class RecipeData : ScriptableObject
    {
        public PotionDefinition potion;
        public int unlockAtDebtPaid;
        public List<BrewingStep> steps = new List<BrewingStep>();
    }
}
