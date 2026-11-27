using System;
using System.Collections.Generic;
using UnityEngine;

namespace SproutOfDebt.Core.Stubs
{
    /// <summary>
    /// Registers stub wallet/inventory in <see cref="GameServices"/> when no real implementation is registered,
    /// so a location scene can be played on its own. Remove once the real systems exist.
    /// Each scene's test items go into the stub inventory the first time that scene opens in a play session,
    /// so every location has its stock whichever scene the game was started from (Main Menu too).
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

        // Scenes whose test items are already in the inventory in this play session.
        private static readonly HashSet<string> stockedScenes = new HashSet<string>();

        // Off after a save is loaded, so the saved inventory is not topped up with test items.
        private static bool testStockEnabled = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStockedScenes()
        {
            stockedScenes.Clear();
            testStockEnabled = true;
        }

        /// <summary>Turns the per-scene test items on or off and starts counting scenes again (a new game stocks them afresh).</summary>
        public static void SetTestStock(bool enabled)
        {
            stockedScenes.Clear();
            testStockEnabled = enabled;
        }

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
                GameServices.Inventory = new StubInventory();
            }

            // Test items never go into a real inventory.
            if (testStockEnabled && GameServices.Inventory is StubInventory inventory && stockedScenes.Add(gameObject.scene.name))
            {
                // Tops each item up to its test count (not on top of what another scene gave), so the numbers are predictable.
                foreach (var item in startingItems)
                {
                    inventory.Add(item.itemId, item.count - inventory.GetCount(item.itemId));
                }
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
