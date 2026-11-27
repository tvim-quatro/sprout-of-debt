using System;
using System.Collections.Generic;
using SproutOfDebt.Core.Save;
using UnityEngine;

namespace SproutOfDebt.Save
{
    /// <summary>
    /// Everything written to the single save file: coins, inventory, brewed potions and the shared
    /// <see cref="GameProgress"/> (garden cells, debt, tutorial). Unlocked recipes follow from the debt progress.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        [SerializeField] private int version = CurrentVersion;
        [SerializeField] private string savedAtUtc;
        [SerializeField] private string location;
        [SerializeField] private int coins;
        [SerializeField] private List<ItemStack> items = new List<ItemStack>();
        [SerializeField] private List<string> brewedPotions = new List<string>();
        [SerializeField] private GameProgress progress = new GameProgress();

        public int Version => version;
        public string SavedAtUtc => savedAtUtc;
        public string Location => location;
        public int Coins => coins;
        public IReadOnlyList<ItemStack> Items => items;
        public IReadOnlyList<string> BrewedPotions => brewedPotions;
        public GameProgress Progress => progress;

        public static SaveData Create(string location, int coins, IEnumerable<KeyValuePair<string, int>> items,
            IEnumerable<string> brewedPotions, GameProgress progress)
        {
            SaveData data = new SaveData
            {
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                location = location,
                coins = coins,
                progress = progress ?? new GameProgress()
            };
            foreach (KeyValuePair<string, int> pair in items)
            {
                if (pair.Value > 0)
                {
                    data.items.Add(new ItemStack(pair.Key, pair.Value));
                }
            }
            data.brewedPotions.AddRange(brewedPotions);
            return data;
        }
    }

    [Serializable]
    public class ItemStack
    {
        [SerializeField] private string itemId;
        [SerializeField] private int count;

        public string ItemId => itemId;
        public int Count => count;

        public ItemStack(string itemId, int count)
        {
            this.itemId = itemId;
            this.count = count;
        }
    }
}
