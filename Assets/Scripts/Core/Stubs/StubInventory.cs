using System;
using System.Collections.Generic;

namespace DrugCrafter.Core.Stubs
{
    /// <summary>In-memory inventory used until the real one exists.</summary>
    public class StubInventory : IInventory
    {
        private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

        public event Action<string, int> ItemChanged;

        public int GetCount(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && counts.TryGetValue(itemId, out int count) ? count : 0;
        }

        public void Add(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return;
            int count = GetCount(itemId) + amount;
            counts[itemId] = count;
            ItemChanged?.Invoke(itemId, count);
        }

        public bool TryRemove(string itemId, int amount)
        {
            if (amount < 0) return false;
            int count = GetCount(itemId);
            if (amount > count) return false;
            if (amount == 0) return true;
            count -= amount;
            counts[itemId] = count;
            ItemChanged?.Invoke(itemId, count);
            return true;
        }
    }
}
