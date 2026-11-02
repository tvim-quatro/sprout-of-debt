using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Shop
{
    [CreateAssetMenu(menuName = "Sprout of Debt/Shop/Seed", fileName = "Seed")]
    public class SeedData : ScriptableObject
    {
        [Tooltip("Inventory item id")]
        public string id;
        public string displayName;
        public Sprite icon;
        [Min(0)] public int buyPrice = 3;
        [Tooltip("Seeds become available in the Seed Shop together with this potion's recipe. Empty = always available.")]
        public PotionDefinition unlockedWith;
    }
}
