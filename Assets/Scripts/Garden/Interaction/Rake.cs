namespace SproutOfDebt.Garden
{
    /// <summary>According to GDD: the rake only affects empty cells and turns them into tilled ones.</summary>
    public class Rake : GardenTool
    {
        public override bool UseOn(GardenPlot plot)
        {
            return plot.TryTill();
        }
    }
}
