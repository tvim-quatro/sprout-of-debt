namespace SproutOfDebt.Core
{
    /// <summary>
    /// Short notification banner at the top of the screen (Figma component "Toast"), such as "+1 Lavender".
    /// Gameplay code shows one through GameServices.Toasts without knowing the UI behind it.
    /// </summary>
    public interface IToastService
    {
        /// <summary>Shows the message for a moment; a new message replaces the one on screen.</summary>
        void Show(string message);
    }
}
