using UnityEngine;

namespace DrugCrafter.Shop
{
    [CreateAssetMenu(menuName = "Sprout of Debt/Shop/Potion", fileName = "Potion")]
    public class PotionData : ScriptableObject
    {
        [Tooltip("Inventory item id")]
        public string id;
        public string displayName;
        public Sprite icon;
        [Tooltip("Fixed price of one potion at the Shop Counter")]
        [Min(0)] public int sellPrice = 10;
    }
}
