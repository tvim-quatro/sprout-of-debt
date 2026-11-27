namespace SproutOfDebt.Core
{
    /// <summary>Remembers which potions the player has already brewed at least once. Must be persisted by the save system.</summary>
    public interface IBrewedPotionLog
    {
        /// <summary>Ids of every potion brewed so far, for the save system.</summary>
        System.Collections.Generic.IReadOnlyCollection<string> BrewedPotionIds { get; }

        bool HasBrewed(string potionId);

        void MarkBrewed(string potionId);
    }
}
