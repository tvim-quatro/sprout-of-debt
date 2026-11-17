using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// The watering can. It is carried tilted (Figma "Watering can (held, tilted)") and pours only above a cell
    /// that needs water. A cell is watered after a few seconds of pouring, then its plant starts growing.
    /// According to GDD: water only affects sown cells.
    /// </summary>
    public class WateringCan : GardenTool
    {
        [Header("Water")]
        [Tooltip("Drops falling from the spout while the can pours.")]
        [SerializeField] private ParticleSystem pourEffect;
        [Tooltip("How long the can pours over a sown cell until it is fully watered, in seconds.")]
        [SerializeField, Min(0.1f)] private float waterSeconds = 2f;

        private bool pouring;

        public override void Hover(GardenPlot plot)
        {
            SetPouring(plot != null && plot.NeedsWater);
        }

        public override bool UseOn(GardenPlot plot)
        {
            if (!pouring)
            {
                return false;
            }
            bool watered = plot.AddWater(Time.deltaTime / waterSeconds);
            if (watered)
            {
                // The cell has all its water now, so the can stops pouring over it.
                Hover(plot);
            }
            return watered;
        }

        protected override void OnReturnedHome()
        {
            SetPouring(false);
        }

        private void SetPouring(bool value)
        {
            if (pouring == value)
            {
                return;
            }
            pouring = value;
            if (pourEffect == null)
            {
                return;
            }
            if (value)
            {
                pourEffect.Play();
            }
            else
            {
                pourEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
