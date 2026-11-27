namespace SproutOfDebt.Core
{
    /// <summary>
    /// Tells the rest of the game that a full-screen window outside the WindowManager (such as the Recipe Book) is open,
    /// so global shortcuts (Esc to the main menu, Tab/I, 1-3) must not react.
    /// </summary>
    public interface IModalState
    {
        /// <summary>True while a modal is open and during the frame it closed, so the same key press cannot also trigger a shortcut.</summary>
        bool IsOpen { get; }

        void Opened();

        void Closed();
    }
}
