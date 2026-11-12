using System.Collections.Generic;
using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// All plant species of the game. The progress stores only a plant id, and the garden finds the plant here.
    /// </summary>
    [CreateAssetMenu(fileName = "PlantCatalog", menuName = "Sprout of Debt/Garden/Plant Catalog")]
    public class PlantCatalog : ScriptableObject
    {
        [SerializeField] private List<PlantDefinition> plants = new List<PlantDefinition>();

        public IReadOnlyList<PlantDefinition> Plants => plants;

        /// <summary>Returns the plant with this id, or null when the id is empty or unknown.</summary>
        public PlantDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            foreach (PlantDefinition plant in plants)
            {
                if (plant != null && plant.Id == id)
                {
                    return plant;
                }
            }
            return null;
        }
    }
}
