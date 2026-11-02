using System;
using System.Collections.Generic;
using SproutOfDebt.Core;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Shop
{
    /// <summary>Debt state: how much is repaid and which potion recipes the repayment has unlocked.</summary>
    public class Treasury
    {
        public ShopConfig Config { get; }
        public int Repaid { get; private set; }
        public int TotalDebt => Config.totalDebt;
        public int Remaining => Mathf.Max(0, TotalDebt - Repaid);
        public float Progress => Mathf.Clamp01((float)Repaid / TotalDebt);
        public bool IsPaidOff => Repaid >= TotalDebt;

        /// <summary>Raised after any change of the repaid amount.</summary>
        public event Action Changed;

        /// <summary>Raised once for every potion unlocked by a deposit.</summary>
        public event Action<PotionDefinition> PotionUnlocked;

        public Treasury(ShopConfig config)
        {
            Config = config;
        }

        /// <summary>Sets the repaid amount without spending coins, e.g. when loading a save.</summary>
        public void Restore(int repaid)
        {
            Repaid = Mathf.Clamp(repaid, 0, TotalDebt);
            Changed?.Invoke();
        }

        /// <summary>Most coins the player can deposit right now.</summary>
        public int MaxDeposit(IWallet wallet)
        {
            return wallet == null ? 0 : Mathf.Min(wallet.Coins, Remaining);
        }

        /// <summary>Moves coins from the wallet to the debt. Returns the potions this deposit unlocked.</summary>
        public List<PotionDefinition> Deposit(IWallet wallet, int amount)
        {
            var unlocked = new List<PotionDefinition>();
            amount = Mathf.Min(amount, MaxDeposit(wallet));
            if (amount <= 0 || !wallet.TrySpend(amount)) return unlocked;

            int before = Repaid;
            Repaid += amount;
            foreach (var milestone in Config.milestones)
            {
                if (milestone.potion != null && milestone.threshold > before && milestone.threshold <= Repaid)
                {
                    unlocked.Add(milestone.potion);
                }
            }

            Changed?.Invoke();
            foreach (var potion in unlocked)
            {
                PotionUnlocked?.Invoke(potion);
            }
            return unlocked;
        }

        /// <summary>Repayment needed to unlock the potion, or -1 if no milestone unlocks it.</summary>
        public int GetUnlockThreshold(PotionDefinition potion)
        {
            if (potion == null) return 0;
            foreach (var milestone in Config.milestones)
            {
                if (milestone.potion == potion) return milestone.threshold;
            }
            return -1;
        }

        public bool IsUnlocked(PotionDefinition potion)
        {
            int threshold = GetUnlockThreshold(potion);
            return threshold >= 0 && threshold <= Repaid;
        }

        public bool IsUnlocked(SeedData seed)
        {
            return seed.unlockedWith == null || IsUnlocked(seed.unlockedWith);
        }

        public List<PotionDefinition> GetUnlockedPotions()
        {
            var result = new List<PotionDefinition>();
            foreach (var milestone in Config.milestones)
            {
                if (milestone.potion != null && milestone.threshold <= Repaid && !result.Contains(milestone.potion))
                {
                    result.Add(milestone.potion);
                }
            }
            return result;
        }

        /// <summary>The closest milestone not reached yet, or null when everything is unlocked.</summary>
        public DebtMilestone GetNextMilestone()
        {
            DebtMilestone next = null;
            foreach (var milestone in Config.milestones)
            {
                if (milestone.threshold > Repaid && (next == null || milestone.threshold < next.threshold))
                {
                    next = milestone;
                }
            }
            return next;
        }
    }
}
