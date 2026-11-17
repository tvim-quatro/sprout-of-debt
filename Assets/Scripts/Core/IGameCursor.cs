namespace SproutOfDebt.Core
{
    /// <summary>The mouse cursor shared by all locations. Gameplay code only says which look it needs.</summary>
    public interface IGameCursor
    {
        void Show(CursorState state);
    }
}
