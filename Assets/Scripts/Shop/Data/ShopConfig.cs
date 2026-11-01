using System;
using System.Collections.Generic;
using UnityEngine;

namespace DrugCrafter.Shop
{
    [Serializable]
    public class DebtMilestone
    {
        [Tooltip("Total coins repaid needed to unlock the potion")]
        [Min(0)] public int threshold;
        public PotionData potion;
    }

    /// <summary>Balance and content of the shop: debt, unlock milestones, seeds for sale, customers.</summary>
    [CreateAssetMenu(menuName = "Sprout of Debt/Shop/Shop Config", fileName = "ShopConfig")]
    public class ShopConfig : ScriptableObject
    {
        [Header("Debt")]
        [Min(1)] public int totalDebt = 1000;
        [Tooltip("Potion recipes unlocked by debt repayment. A threshold of 0 means unlocked from the start.")]
        public List<DebtMilestone> milestones = new List<DebtMilestone>();

        [Header("Seed Shop")]
        public List<SeedData> seeds = new List<SeedData>();

        [Header("Shop Counter")]
        public List<CustomerData> customers = new List<CustomerData>();
        [Min(1)] public int minQuantity = 1;
        [Min(1)] public int maxQuantity = 3;
        [Tooltip("Seconds between a customer leaving and the next one arriving")]
        [Min(0)] public float nextCustomerDelay = 3f;
        [Tooltip("Seconds the customer stays after the player answers")]
        [Min(0)] public float replyDuration = 1.8f;
    }
}
