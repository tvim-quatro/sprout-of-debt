using System;
using System.Collections.Generic;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Shop
{
    [Serializable]
    public struct ItemDisplayInfo
    {
        public string id;
        public string displayName;
        public Sprite icon;
    }

    /// <summary>Catalog of all seeds, ingredients and potions shown in the inventory.</summary>
    [CreateAssetMenu(menuName = "Sprout of Debt/Shop/Inventory Database", fileName = "InventoryDatabase")]
    public class InventoryDatabase : ScriptableObject
    {
        [Header("Seeds")]
        [SerializeField] private List<SeedData> seeds = new List<SeedData>();

        [Header("Ingredients")]
        [SerializeField] private List<IngredientData> ingredients = new List<IngredientData>();

        [Header("Potions")]
        [SerializeField] private List<PotionDefinition> potions = new List<PotionDefinition>();

        public IReadOnlyList<SeedData> Seeds => seeds;
        public IReadOnlyList<IngredientData> Ingredients => ingredients;
        public IReadOnlyList<PotionDefinition> Potions => potions;

        public bool TryGetItemInfo(string itemId, out ItemDisplayInfo info)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                info = default;
                return false;
            }

            for (int i = 0; i < seeds.Count; i++)
            {
                var seed = seeds[i];
                if (seed != null && seed.id == itemId)
                {
                    info = new ItemDisplayInfo { id = seed.id, displayName = seed.displayName, icon = seed.icon };
                    return true;
                }
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                var ingr = ingredients[i];
                if (ingr != null && ingr.id == itemId)
                {
                    info = new ItemDisplayInfo { id = ingr.id, displayName = ingr.displayName, icon = ingr.icon };
                    return true;
                }
            }

            for (int i = 0; i < potions.Count; i++)
            {
                var potion = potions[i];
                if (potion != null && potion.Id == itemId)
                {
                    info = new ItemDisplayInfo { id = potion.Id, displayName = potion.DisplayName, icon = potion.Icon };
                    return true;
                }
            }

            info = default;
            return false;
        }
    }
}
