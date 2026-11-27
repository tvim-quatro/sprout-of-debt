using System;
using System.Collections.Generic;
using SproutOfDebt.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SproutOfDebt.Shop
{
    public enum CustomerMood
    {
        Neutral,
        Happy,
        Disappointed
    }

    /// <summary>Why the customer needs this potion, told in their own words before the order.</summary>
    [Serializable]
    public class PotionStory
    {
        public PotionDefinition potion;
        [TextArea(2, 5)] public string[] lines;
    }

    [CreateAssetMenu(menuName = "Sprout of Debt/Shop/Customer", fileName = "Customer")]
    public class CustomerData : ScriptableObject
    {
        public string displayName;

        [Header("Expressions")]
        public Sprite neutral;
        public Sprite happy;
        public Sprite disappointed;

        [Header("Stories (one per potion, picked at random if there are several)")]
        public List<PotionStory> stories = new List<PotionStory>();

        [Header("Lines (one is picked at random)")]
        [Tooltip("Said after the story, right before the order")]
        [TextArea] public string[] requestLines;
        [TextArea] public string[] thanksLines;
        [TextArea] public string[] disappointedLines;

        [Header("Prices")]
        [Tooltip("Lowest multiplier of the potion's base price this customer pays (1 = base price)")]
        [SerializeField, Min(0.1f)] private float minPriceMultiplier = 0.9f;
        [Tooltip("Highest multiplier of the potion's base price this customer pays (1 = base price)")]
        [SerializeField, Min(0.1f)] private float maxPriceMultiplier = 1.1f;

        public float MinPriceMultiplier => minPriceMultiplier;
        public float MaxPriceMultiplier => maxPriceMultiplier;

        /// <summary>
        /// Price of one potion for this customer: base price times a random multiplier from the customer's range,
        /// rounded to whole coins (at least 1).
        /// </summary>
        public int RollUnitPrice(PotionDefinition potion)
        {
            if (potion == null) return 0;
            float multiplier = Random.Range(minPriceMultiplier, maxPriceMultiplier);
            return Mathf.Max(1, Mathf.RoundToInt(potion.Price * multiplier));
        }

        public Sprite GetSprite(CustomerMood mood)
        {
            switch (mood)
            {
                case CustomerMood.Happy: return happy != null ? happy : neutral;
                case CustomerMood.Disappointed: return disappointed != null ? disappointed : neutral;
                default: return neutral;
            }
        }

        /// <summary>A story about the potion, or null if the customer has none for it.</summary>
        public string GetStory(PotionDefinition potion)
        {
            foreach (var story in stories)
            {
                if (story.potion == potion && story.lines != null && story.lines.Length > 0)
                {
                    return story.lines[Random.Range(0, story.lines.Length)];
                }
            }
            return null;
        }

        private void OnValidate()
        {
            if (maxPriceMultiplier < minPriceMultiplier) maxPriceMultiplier = minPriceMultiplier;
        }

        public static string PickLine(string[] lines, string fallback)
        {
            return lines != null && lines.Length > 0 ? lines[Random.Range(0, lines.Length)] : fallback;
        }
    }
}
