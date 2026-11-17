namespace SproutOfDebt.Core
{
    /// <summary>Cursor looks from Figma "UI Kit / Cursors".</summary>
    public enum CursorState
    {
        /// <summary>The game's themed arrow, everywhere (also over buttons and locked elements).</summary>
        Default,
        /// <summary>Open hand, only over things the player can pick up or drag.</summary>
        Hover,
        /// <summary>Closed hand while the mouse button is held on something draggable.</summary>
        Grab,
    }
}
