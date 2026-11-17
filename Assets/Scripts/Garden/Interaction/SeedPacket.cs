using SproutOfDebt.Core;
using SproutOfDebt.Core.Save;
using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// The seed packet in the player's hand, taken from the Inventory window (Figma "Garden - Sowing").
    /// It is hidden until the player drags seeds out of the inventory, shows that plant's packet, and disappears when
    /// the mouse button is released. It is carried upright and tips over to pour only above a tilled cell.
    /// A cell is sown after pouring on it without a break for the full sowing time; only then one seed is taken from
    /// the inventory. Moving off the cell (or letting go) before that cancels the pour: nothing is sown or spent.
    /// With no seeds left it stays upright. According to GDD: seeds only affect tilled cells.
    /// </summary>
    public class SeedPacket : GardenTool
    {
        [Header("Seeds")]
        [Tooltip("Seeds falling from the packet while it pours.")]
        [SerializeField] private ParticleSystem pourEffect;
        [Tooltip("Tilt in degrees while pouring (mouth down).")]
        [SerializeField] private float pourTilt = 140f;
        [Tooltip("Pouring time without a break that sows one cell, in seconds.")]
        [SerializeField, Min(0f)] private float sowSeconds = 0.8f;

        private PlantDefinition plant;
        private bool pouring;
        // The cell being poured on and for how long, in seconds; both reset when the packet leaves the cell.
        private GardenPlot pourPlot;
        private float pourTime;

        private static IInventory Inventory => GameServices.Inventory;

        private int SeedCount => plant != null && Inventory != null ? Inventory.GetCount(plant.SeedItemId) : 0;

        /// <summary>Shows the packet of this plant's seeds; the drag controller then puts it into the hand.</summary>
        public void Fill(PlantDefinition seeds)
        {
            plant = seeds;
            gameObject.SetActive(true);
            if (seeds.SeedSprite != null)
            {
                GetComponent<SpriteRenderer>().sprite = seeds.SeedSprite;
            }
        }

        public override void Hover(GardenPlot plot)
        {
            if (plot != pourPlot)
            {
                pourPlot = plot;
                pourTime = 0f;
            }
            SetPouring(plot != null && plot.CanSow && SeedCount > 0);
        }

        public override bool UseOn(GardenPlot plot)
        {
            if (!pouring)
            {
                return false;
            }
            pourTime += Time.deltaTime;
            if (pourTime < sowSeconds)
            {
                return false;
            }
            // The full sowing time is poured: exactly one seed goes from the inventory into this cell.
            bool sown = Inventory.TryRemove(plant.SeedItemId, 1) && plot.TrySow(plant);
            pourTime = 0f;
            // The cell is sown now (or the seeds ran out), so the packet stands up again.
            Hover(plot);
            return sown;
        }

        // Released: the seeds stay in the inventory, so the packet just disappears.
        protected override void OnReturnedHome()
        {
            SetPouring(false);
            pourPlot = null;
            pourTime = 0f;
            plant = null;
            gameObject.SetActive(false);
        }

        private void SetPouring(bool value)
        {
            if (pouring == value)
            {
                return;
            }
            pouring = value;
            SetTilt(value ? pourTilt : 0f);
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
