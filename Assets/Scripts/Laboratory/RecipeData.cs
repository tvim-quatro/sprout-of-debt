using System.Collections.Generic;
using DrugCrafter.Items;
using UnityEngine;

namespace DrugCrafter.Laboratory
{
    [CreateAssetMenu(fileName = "Recipe_", menuName = "Sprout of Debt/Recipe")]
    public class RecipeData : ScriptableObject
    {
        public PotionData potion;
        public int unlockAtDebtPaid;
        public List<BrewingStep> steps = new List<BrewingStep>();
    }
}
