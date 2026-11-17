using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// One plant species: which seed grows it, how long it takes and what it gives at harvest.
    /// According to GDD: each species has its own growth timer and three growth stages.
    /// </summary>
    [CreateAssetMenu(fileName = "Plant_", menuName = "Sprout of Debt/Garden/Plant")]
    public class PlantDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [Tooltip("Inventory id of the seed that grows this plant (seed items are defined by the Seed Shop).")]
        [SerializeField] private string seedItemId;

        [Header("Growth")]
        [Tooltip("Seconds from watering to ripe. The plant shows its second stage after half of this time.")]
        [SerializeField, Min(1f)] private float growSeconds = 30f;

        [Header("Harvest")]
        [SerializeField] private IngredientData harvest;
        [SerializeField, Min(1)] private int harvestAmount = 1;

        [Header("Sprites")]
        [SerializeField] private Sprite sproutSprite;
        [SerializeField] private Sprite stage2Sprite;
        [SerializeField] private Sprite ripeSprite;
        [Tooltip("The seed packet the player holds while sowing (the seed's inventory icon).")]
        [SerializeField] private Sprite seedSprite;

        public string Id => id;
        public string SeedItemId => seedItemId;
        public float GrowSeconds => growSeconds;
        public IngredientData Harvest => harvest;
        public int HarvestAmount => harvestAmount;
        public Sprite SproutSprite => sproutSprite;
        public Sprite Stage2Sprite => stage2Sprite;
        public Sprite RipeSprite => ripeSprite;
        public Sprite SeedSprite => seedSprite;
    }
}
