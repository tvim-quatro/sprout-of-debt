using System;

namespace SproutOfDebt.Core
{
    /// <summary>Single player inventory (seeds, ingredients, potions), keyed by item id. No slot limit.</summary>
    public interface IInventory
    {
        /// <summary>Raised with (itemId, newCount) whenever an item count changes.</summary>
        event Action<string, int> ItemChanged;

        int GetCount(string itemId);

        void Add(string itemId, int amount);

        /// <summary>Removes items if there are enough of them. Returns false and removes nothing otherwise.</summary>
        bool TryRemove(string itemId, int amount);
    }
}
