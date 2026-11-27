using SproutOfDebt.Core;
using UnityEngine;

namespace SproutOfDebt.Shop
{
    /// <summary>
    /// Rules of one Seed Shop purchase (Figma "Seed Shop" flow): the player picks one unlocked seed and an amount,
    /// then pays price × amount and gets the seeds in the inventory. Holds no UI; the window only shows this state.
    /// </summary>
    public class SeedPurchase
    {
        public const int MinAmount = 1;

        public SeedData Selected { get; private set; }
        public int Amount { get; private set; } = MinAmount;

        public int Coins => GameServices.Wallet != null ? GameServices.Wallet.Coins : 0;
        public int Total => Selected != null ? Selected.buyPrice * Amount : 0;
        public bool CanAfford => Selected != null && Total <= Coins;
        public bool CanDecrease => Selected != null && Amount > MinAmount;
        // According to Figma: + is disabled when one more seed would cost more than the player has.
        public bool CanIncrease => Selected != null && Selected.buyPrice * (Amount + 1) <= Coins;

        public static bool IsUnlocked(SeedData seed)
        {
            Treasury treasury = ShopContext.Treasury;
            return seed != null && treasury != null && treasury.IsUnlocked(seed);
        }

        /// <summary>Selects an unlocked seed. Switching seeds keeps the chosen amount (Figma "another seed selected").</summary>
        public bool Select(SeedData seed)
        {
            if (!IsUnlocked(seed))
            {
                return false;
            }
            Selected = seed;
            return true;
        }

        public void Clear()
        {
            Selected = null;
            Amount = MinAmount;
        }

        public void Increase()
        {
            if (CanIncrease)
            {
                Amount++;
            }
        }

        public void Decrease()
        {
            if (CanDecrease)
            {
                Amount--;
            }
        }

        /// <summary>Pays for the selected seeds and adds them to the inventory. Returns how many were bought (0 when nothing was).</summary>
        public int TryBuy()
        {
            IInventory inventory = GameServices.Inventory;
            IWallet wallet = GameServices.Wallet;
            if (!CanAfford || !IsUnlocked(Selected) || inventory == null || wallet == null)
            {
                return 0;
            }
            if (!wallet.TrySpend(Total))
            {
                return 0;
            }
            int bought = Amount;
            inventory.Add(Selected.id, bought);
            Debug.Log($"[SeedShop] Bought {bought} × {Selected.id}");
            return bought;
        }
    }
}
