using System;
using SproutOfDebt.Core;
using SproutOfDebt.Core.Save;
using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// One planting cell of the garden bed: shows its state and changes it by the GDD rules
    /// (rake on empty, seeds on tilled, water on sown). The state itself lives in GameServices.Progress,
    /// so it survives location changes and can be saved.
    /// </summary>
    public class GardenPlot : MonoBehaviour
    {
        [Tooltip("Cell number in the garden progress: 0 = top-left, left to right, top row first.")]
        [SerializeField] private int index;

        [Tooltip("All plants, to find the one growing here by the id saved in the progress.")]
        [SerializeField] private PlantCatalog catalog;

        [Header("Renderers")]
        [SerializeField] private SpriteRenderer soilRenderer;
        [SerializeField] private SpriteRenderer plantRenderer;

        [Header("Soil Sprites")]
        [SerializeField] private Sprite emptySoil;
        [SerializeField] private Sprite tilledSoil;
        [SerializeField] private Sprite sownSoil;
        [SerializeField] private Sprite wateredSoil;

        [Header("Effects")]
        [Tooltip("Soil clods that fly up when the cell is tilled.")]
        [SerializeField] private ParticleSystem tillEffect;

        [Header("Layout")]
        [Tooltip("How far below the cell center the bottom of the plant sprite sits, in world units.")]
        [SerializeField] private float plantBaseOffset = 0.2f;

        private PlotProgress data;
        private PlantDefinition plant;
        private bool showsStage2;

        public PlotState State => data.State;
        public PlantDefinition Plant => plant;
        /// <summary>Growth progress from 0 to 1 while growing, 1 when ripe.</summary>
        public float Progress => State == PlotState.Ripe ? 1f : plant != null ? Mathf.Clamp01(GrowTime / plant.GrowSeconds) : 0f;

        // Real time, not Time.time, so the plant also grows while the game is closed.
        private static long NowUtcMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private float GrowTime => (NowUtcMs - data.GrowStartUtcMs) / 1000f;

        public event Action<GardenPlot> StateChanged;

        private void Awake()
        {
            data = GameServices.Progress.Garden.GetPlot(index);
            plant = catalog != null ? catalog.Find(data.PlantId) : null;
            showsStage2 = State == PlotState.Growing && plant != null && GrowTime >= plant.GrowSeconds / 2f;
            Refresh();
        }

        private void Update()
        {
            if (State != PlotState.Growing || plant == null)
            {
                return;
            }
            if (GrowTime >= plant.GrowSeconds)
            {
                SetState(PlotState.Ripe);
            }
            else if (!showsStage2 && GrowTime >= plant.GrowSeconds / 2f)
            {
                showsStage2 = true;
                Refresh();
            }
        }

        public bool TryTill()
        {
            if (State != PlotState.Empty) return false;
            SetState(PlotState.Tilled);
            if (tillEffect != null)
            {
                tillEffect.Play();
            }
            return true;
        }

        public bool TrySow(PlantDefinition seedPlant)
        {
            if (State != PlotState.Tilled || seedPlant == null) return false;
            plant = seedPlant;
            data.SetPlant(seedPlant.Id);
            SetState(PlotState.Sown);
            return true;
        }

        public bool TryWater()
        {
            if (State != PlotState.Sown) return false;
            data.StartGrowing(NowUtcMs);
            showsStage2 = false;
            SetState(PlotState.Growing);
            return true;
        }

        /// <summary>Puts the harvest into the inventory and frees the cell, which must be tilled again.</summary>
        public bool TryHarvest()
        {
            if (State != PlotState.Ripe) return false;
            if (plant.Harvest != null)
            {
                GameServices.Inventory?.Add(plant.Harvest.id, plant.HarvestAmount);
            }
            plant = null;
            data.SetPlant(null);
            SetState(PlotState.Empty);
            return true;
        }

        private void SetState(PlotState state)
        {
            data.SetState(state);
            Refresh();
            StateChanged?.Invoke(this);
        }

        private void Refresh()
        {
            if (soilRenderer != null)
            {
                soilRenderer.sprite = SoilSprite();
            }
            if (plantRenderer != null)
            {
                plantRenderer.sprite = PlantSprite();
                if (plantRenderer.sprite != null)
                {
                    // Sprites are centered, so lift each one by half its height to stand on the soil.
                    float y = plantRenderer.sprite.bounds.extents.y - plantBaseOffset;
                    plantRenderer.transform.localPosition = new Vector3(0f, y, 0f);
                }
            }
        }

        private Sprite SoilSprite()
        {
            switch (State)
            {
                case PlotState.Tilled: return tilledSoil;
                case PlotState.Sown: return sownSoil;
                case PlotState.Growing:
                case PlotState.Ripe: return wateredSoil;
                default: return emptySoil;
            }
        }

        private Sprite PlantSprite()
        {
            if (plant == null) return null;
            switch (State)
            {
                case PlotState.Growing: return showsStage2 ? plant.Stage2Sprite : plant.SproutSprite;
                case PlotState.Ripe: return plant.RipeSprite;
                default: return null;
            }
        }
    }
}
