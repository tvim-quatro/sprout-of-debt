using UnityEngine;

namespace DrugCrafter.Items
{
    [CreateAssetMenu(fileName = "Ingredient_", menuName = "Sprout of Debt/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
    }
}
