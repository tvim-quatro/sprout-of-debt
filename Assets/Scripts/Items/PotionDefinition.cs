using UnityEngine;
using UnityEngine.Serialization;

namespace SproutOfDebt.Items
{
    /// <summary>
    /// One potion type, shared by every area: brewed in the Laboratory, stored in the inventory by <see cref="Id"/>,
    /// requested by customers and sold at the Shop Counter. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "Potion_", menuName = "Sprout of Debt/Items/Potion")]
    public class PotionDefinition : ScriptableObject
    {
        [Tooltip("Stable inventory / save id, e.g. potion_calming")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [Tooltip("Fixed price of one potion at the Shop Counter")]
        [FormerlySerializedAs("sellPrice")]
        [SerializeField, Min(0)] private int price = 10;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int Price => price;
    }
}
