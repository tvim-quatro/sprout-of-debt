using SproutOfDebt.Core;
using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// Lets the player drag seeds out of the Inventory window into the Garden: a seed of a known plant becomes the
    /// seed packet in the hand. Also gives the seed tooltip its growing time ("Grows in 0:30").
    /// Registers itself as the location's item handler while the Garden is open.
    /// </summary>
    public class GardenSeedHandler : MonoBehaviour, IInventoryItemHandler
    {
        [SerializeField] private PlantCatalog catalog;
        [SerializeField] private SeedPacket packet;
        [SerializeField] private GardenDragController dragController;

        private void OnEnable()
        {
            GameServices.ItemHandler = this;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(GameServices.ItemHandler, this))
            {
                GameServices.ItemHandler = null;
            }
        }

        public bool CanDrag(string itemId)
        {
            return packet != null && dragController != null && FindPlant(itemId) != null
                && GameServices.Inventory != null && GameServices.Inventory.GetCount(itemId) > 0;
        }

        public void BeginDrag(string itemId)
        {
            if (!CanDrag(itemId))
            {
                return;
            }
            packet.Fill(FindPlant(itemId));
            dragController.Hold(packet);
        }

        public string GetItemInfo(string itemId)
        {
            PlantDefinition plant = FindPlant(itemId);
            if (plant == null)
            {
                return null;
            }
            int seconds = Mathf.CeilToInt(plant.GrowSeconds);
            return $"Grows in {seconds / 60}:{seconds % 60:00}";
        }

        /// <summary>The plant grown from this seed item, or null when the item is not a seed of the catalog.</summary>
        private PlantDefinition FindPlant(string seedItemId)
        {
            if (catalog == null || string.IsNullOrEmpty(seedItemId))
            {
                return null;
            }
            foreach (PlantDefinition plant in catalog.Plants)
            {
                if (plant != null && plant.SeedItemId == seedItemId)
                {
                    return plant;
                }
            }
            return null;
        }
    }
}
