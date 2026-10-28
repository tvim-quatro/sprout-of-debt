using UnityEngine;

namespace DrugCrafter.Items
{
    [CreateAssetMenu(fileName = "Potion_", menuName = "Sprout of Debt/Potion")]
    public class PotionData : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        public int price;
    }
}
