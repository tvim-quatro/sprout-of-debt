using UnityEngine;

namespace SproutOfDebt.Core
{
    /// <summary>
    /// Shows items going into the inventory (Figma "Laboratory - Potion is ready": the item flies to the bag button
    /// and an "Added badge" "+N" appears under it). Used for bought seeds, harvested ingredients and brewed potions.
    /// Only a visual: the caller adds the items to <see cref="IInventory"/> itself.
    /// </summary>
    public interface IItemFlyService
    {
        /// <summary>Flies the icon from a screen point (pixels) to the inventory button and shows "+amount" under it.</summary>
        void FlyToInventory(Sprite icon, int amount, Vector2 screenPoint);
    }
}
