using System.Collections.Generic;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    [CreateAssetMenu(fileName = "Recipe_", menuName = "Sprout of Debt/Laboratory/Recipe")]
    public class RecipeDefinition : ScriptableObject
    {
        [SerializeField] private PotionDefinition potion;
        [SerializeField, Min(0)] private int unlockAtDebtPaid;
        [SerializeField] private List<BrewingStep> steps = new List<BrewingStep>();

        public PotionDefinition Potion => potion;
        public int UnlockAtDebtPaid => unlockAtDebtPaid;
        public IReadOnlyList<BrewingStep> Steps => steps;
    }
}
