using System;
using System.Collections.Generic;
using UnityEngine;

namespace SproutOfDebt.Core.Stubs
{
    /// <summary>
    /// Registers stub wallet/inventory in <see cref="GameServices"/> when no real implementation is registered,
    /// so a location scene can be played on its own. Remove once the real systems exist.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class StubServicesInstaller : MonoBehaviour
    {
        [Serializable]
        public struct StartingItem
        {
            public string itemId;
            public int count;
        }

        [SerializeField] private int startingCoins;
        [SerializeField] private List<StartingItem> startingItems = new List<StartingItem>();

        private void Awake()
        {
            if (GameServices.Wallet == null)
            {
                GameServices.Wallet = new StubWallet(startingCoins);
            }

            if (GameServices.BrewedPotions == null)
            {
                GameServices.BrewedPotions = new StubBrewedPotionLog();
            }

            if (GameServices.Inventory == null)
            {
                var inventory = new StubInventory();
                foreach (var item in startingItems)
                {
                    inventory.Add(item.itemId, item.count);
                }
                GameServices.Inventory = inventory;
            }
        }

        [ContextMenu("Debug/Add 100 coins")]
        private void DebugAddCoins()
        {
            GameServices.Wallet?.Add(100);
        }

        [ContextMenu("Debug/Add starting items again")]
        private void DebugAddItems()
        {
            if (GameServices.Inventory == null) return;
            foreach (var item in startingItems)
            {
                GameServices.Inventory.Add(item.itemId, item.count);
            }
        }
    }
}
