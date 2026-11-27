using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Shop
{
    public enum CustomerMood
    {
        Neutral,
        Happy,
        Disappointed
    }

    /// <summary>
    /// A counter visitor (Figma: Counter components, Customers). On arrival the dialogue box shows one random
    /// greeting and one random order line together; {order} is filled in by the game, so one template works
    /// for every potion. After Sell the visitor is happy and says a thanks line; after "Sorry, we're out"
    /// they are disappointed and say the matching line.
    /// </summary>
    [CreateAssetMenu(menuName = "Sprout of Debt/Shop/Customer", fileName = "Customer")]
    public class CustomerData : ScriptableObject
    {
        public const string OrderToken = "{order}";

        public string displayName;

        [Header("Expressions")]
        public Sprite neutral;
        public Sprite happy;
        public Sprite disappointed;

        [Header("Lines (one is picked at random)")]
        [Tooltip("Neutral. Shown when the visitor arrives, together with the order.")]
        [TextArea] public string[] greetingLines;
        [Tooltip("Neutral. Follows the greeting in the same dialogue box. {order} becomes e.g. \"a Calming Potion\" or \"2 Calming Potions\".")]
        [TextArea] public string[] orderLines;
        [Tooltip("Happy. Said after a sale.")]
        [TextArea] public string[] thanksLines;
        [Tooltip("Disappointed. Said after \"Sorry, we're out\".")]
        [TextArea] public string[] disappointedLines;

        public Sprite GetSprite(CustomerMood mood)
        {
            switch (mood)
            {
                case CustomerMood.Happy: return happy != null ? happy : neutral;
                case CustomerMood.Disappointed: return disappointed != null ? disappointed : neutral;
                default: return neutral;
            }
        }

        /// <summary>Random greeting plus random order line, with {order} filled in.</summary>
        public string BuildArrivalLine(PotionDefinition potion, int quantity)
        {
            string greeting = PickLine(greetingLines, "Hello!");
            string order = PickLine(orderLines, "I'd like {order}, please.");
            return greeting + " " + FillOrder(order, potion, quantity);
        }

        /// <summary>Replaces {order} with "a Calming Potion" / "2 Calming Potions"; capitalised at the start of a sentence.</summary>
        public static string FillOrder(string line, PotionDefinition potion, int quantity)
        {
            string name = potion.DisplayName;
            string order = quantity <= 1 ? $"{Article(name)} {name}" : $"{quantity} {Plural(name)}";

            var result = new System.Text.StringBuilder(line.Length + order.Length);
            int start = 0;
            int index;
            while ((index = line.IndexOf(OrderToken, start, System.StringComparison.Ordinal)) >= 0)
            {
                result.Append(line, start, index - start);
                result.Append(IsSentenceStart(line, index) ? Capitalise(order) : order);
                start = index + OrderToken.Length;
            }
            result.Append(line, start, line.Length - start);
            return result.ToString();
        }

        public static string PickLine(string[] lines, string fallback)
        {
            return lines != null && lines.Length > 0 ? lines[Random.Range(0, lines.Length)] : fallback;
        }

        private static string Article(string name)
        {
            return name.Length > 0 && "AEIOUaeiou".IndexOf(name[0]) >= 0 ? "an" : "a";
        }

        private static string Plural(string name)
        {
            return name.EndsWith("s") ? name : name + "s";
        }

        private static bool IsSentenceStart(string line, int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                char c = line[i];
                if (char.IsWhiteSpace(c)) continue;
                return c == '.' || c == '!' || c == '?';
            }
            return true;
        }

        private static string Capitalise(string text)
        {
            return text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text.Substring(1);
        }
    }
}
