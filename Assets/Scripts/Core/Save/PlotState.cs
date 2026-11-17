namespace SproutOfDebt.Core.Save
{
    /// <summary>
    /// According to GDD: till with the rake → sow seeds → water → wait for the timer → harvest → till again.
    /// </summary>
    public enum PlotState
    {
        Empty,
        Tilled,
        Sown,
        Growing,
        Ripe,
    }
}
