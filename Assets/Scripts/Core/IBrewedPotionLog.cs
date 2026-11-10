namespace SproutOfDebt.Core
{
    /// <summary>Remembers which potions the player has already brewed at least once. Must be persisted by the save system.</summary>
    public interface IBrewedPotionLog
    {
        bool HasBrewed(string potionId);

        void MarkBrewed(string potionId);
    }
}
