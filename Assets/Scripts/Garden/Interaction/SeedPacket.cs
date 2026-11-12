using SproutOfDebt.Core;
using SproutOfDebt.Core.Save;
using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// A packet of one plant's seeds. It is carried upright and tips over to pour only above a tilled cell,
    /// sowing it after a short pour (one seed per cell, taken from the inventory). With no seeds left it stays
    /// upright. According to GDD: seeds only affect tilled cells.
    /// Temporary place for the seeds until the Inventory window exists; the sowing logic stays the same then.
    /// </summary>
    public class SeedPacket : GardenTool
    {
        [Header("Seeds")]
        [SerializeField] private PlantDefinition plant;
        [Tooltip("Seeds falling from the packet while it pours.")]
        [SerializeField] private ParticleSystem pourEffect;
        [Tooltip("Tilt in degrees while pouring (mouth down).")]
        [SerializeField] private float pourTilt = 140f;
        [Tooltip("How long the packet pours over a cell before the cell is sown, in seconds.")]
        [SerializeField, Min(0f)] private float sowSeconds = 0.8f;

        private IInventory inventory;
        private bool pouring;
        private GardenPlot pourPlot;
        private float pourTime;

        private int SeedCount => plant != null && inventory != null ? inventory.GetCount(plant.SeedItemId) : 0;

        private void Start()
        {
            // Services are installed in Awake, so they are ready here.
            inventory = GameServices.Inventory;
        }

        public override void Hover(GardenPlot plot)
        {
            if (plot != pourPlot)
            {
                pourPlot = plot;
                pourTime = 0f;
            }
            SetPouring(plot != null && plot.State == PlotState.Tilled && SeedCount > 0);
        }

        public override bool UseOn(GardenPlot plot)
        {
            if (!pouring)
            {
                return false;
            }
            pourTime += Time.deltaTime;
            if (pourTime < sowSeconds || !inventory.TryRemove(plant.SeedItemId, 1))
            {
                return false;
            }
            pourTime = 0f;
            bool sown = plot.TrySow(plant);
            // The cell is sown now (or the seeds ran out), so the packet stands up again.
            Hover(plot);
            return sown;
        }

        protected override void OnReturnedHome()
        {
            SetPouring(false);
            pourPlot = null;
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
