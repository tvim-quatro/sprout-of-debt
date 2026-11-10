using System.Collections.Generic;

namespace SproutOfDebt.Core.Stubs
{
    /// <summary>In-memory brewed potion log used until the save system exists.</summary>
    public class StubBrewedPotionLog : IBrewedPotionLog
    {
        private readonly HashSet<string> brewed = new HashSet<string>();

        public bool HasBrewed(string potionId)
        {
            return brewed.Contains(potionId);
        }

        public void MarkBrewed(string potionId)
        {
            brewed.Add(potionId);
        }
    }
}
