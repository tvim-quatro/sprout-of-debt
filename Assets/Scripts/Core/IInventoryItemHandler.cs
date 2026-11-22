namespace SproutOfDebt.Core
{
    /// <summary>
    /// What the current location does with inventory items: which items can be dragged out of the Inventory window
    /// into the location (seeds in the Garden), and the location's line of info for the item tooltip.
    /// The location registers itself in <see cref="GameServices.ItemHandler"/>, so the inventory UI doesn't depend on it.
    /// </summary>
    public interface IInventoryItemHandler
    {
        /// <summary>True when the player can drag this item into the location now.</summary>
        bool CanDrag(string itemId);

        /// <summary>Puts the item into the player's hand; the drag goes on while the mouse button is held.</summary>
        void BeginDrag(string itemId);

        /// <summary>One line about the item for its tooltip (a seed: "Grows in 0:30"), or null when there is nothing to add.</summary>
        string GetItemInfo(string itemId);
    }
}
