#if UNITY_EDITOR
using SproutOfDebt.Core;
using UnityEditor;
using UnityEngine;

namespace SproutOfDebt.Shop
{
    /// <summary>
    /// Test helpers for Play mode, since a new game starts with an empty inventory and no coins:
    /// Tools > Debug > Add Test Items puts 5 of every seed, ingredient and potion into the inventory,
    /// Tools > Debug > Add 100 Coins adds coins. Editor only; nothing here ships with the game.
    /// </summary>
    public static class InventoryDebugMenu
    {
        private const string DatabasePath = "Assets/Data/Inventory/InventoryDatabase.asset";
        private const int TestItemCount = 5;
        private const int TestCoins = 100;

        [MenuItem("Tools/Debug/Add Test Items")]
        private static void AddTestItems()
        {
            var database = AssetDatabase.LoadAssetAtPath<InventoryDatabase>(DatabasePath);
            IInventory inventory = GameServices.Inventory;
            if (database == null || inventory == null)
            {
                Debug.LogWarning("[Debug] Enter Play mode first (and keep " + DatabasePath + " in place)");
                return;
            }
            foreach (var seed in database.Seeds)
            {
                if (seed != null) inventory.Add(seed.id, TestItemCount);
            }
            foreach (var ingredient in database.Ingredients)
            {
                if (ingredient != null) inventory.Add(ingredient.id, TestItemCount);
            }
            foreach (var potion in database.Potions)
            {
                if (potion != null) inventory.Add(potion.Id, TestItemCount);
            }
            Debug.Log($"[Debug] Added {TestItemCount} of every item");
        }

        [MenuItem("Tools/Debug/Add 100 Coins")]
        private static void AddCoins()
        {
            if (GameServices.Wallet == null)
            {
                Debug.LogWarning("[Debug] Enter Play mode first");
                return;
            }
            GameServices.Wallet.Add(TestCoins);
        }

        [MenuItem("Tools/Debug/Add Test Items", true)]
        [MenuItem("Tools/Debug/Add 100 Coins", true)]
        private static bool IsPlaying()
        {
            return Application.isPlaying;
        }
    }
}
#endif
