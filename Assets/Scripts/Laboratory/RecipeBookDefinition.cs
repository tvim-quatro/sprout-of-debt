using System.Collections.Generic;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    [CreateAssetMenu(fileName = "RecipeBook_", menuName = "Sprout of Debt/Laboratory/Recipe Book")]
    public class RecipeBookDefinition : ScriptableObject
    {
        [SerializeField] private List<RecipeDefinition> recipes = new List<RecipeDefinition>();

        public IReadOnlyList<RecipeDefinition> Recipes => recipes;
    }
}
