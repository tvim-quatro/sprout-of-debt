namespace SproutOfDebt.Core
{
    /// <summary>
    /// The single save slot (GDD: coins, inventory, garden, unlocked recipes and debt progress, saved automatically
    /// after key events and on quit). Registered in <see cref="GameServices.Save"/> by the save system.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>True when a save file exists.</summary>
        bool HasSave { get; }

        /// <summary>Writes the current game to the save file now; does nothing before a game has started.</summary>
        void SaveNow();

        /// <summary>
        /// Loads the save into the shared services and returns the location to open.
        /// Returns false (and changes nothing) when there is no usable save.
        /// </summary>
        bool TryLoad(out string location);

        /// <summary>Resets the shared services and progress for a fresh game. The old save is replaced by the first autosave.</summary>
        void StartNewGame();
    }
}
