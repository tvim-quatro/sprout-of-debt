using System;
using SproutOfDebt.Core;
using SproutOfDebt.Core.Save;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// One planting cell of the garden bed: shows its state and changes it by the GDD rules
    /// (rake on empty, seeds on tilled, water on sown, click on ripe to harvest). The state itself lives in GameServices.Progress,
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
        [Tooltip("Click area of the plant; it takes the plant sprite's size and is on only while the plant is ripe.")]
        [SerializeField] private BoxCollider2D plantCollider;

        [Header("Soil Sprites")]
        [SerializeField] private Sprite emptySoil;
        [SerializeField] private Sprite tilledSoil;
        [SerializeField] private Sprite sownSoil;
        [SerializeField] private Sprite wateredSoil;

        [Header("Effects")]
        [Tooltip("Soil clods that fly up when the cell is tilled.")]
        [SerializeField] private ParticleSystem tillEffect;

        [Header("Layout")]
        [Tooltip("How far below the cell center the bottom of the plant sprite sits, in world units (Figma: 43.5 px).")]
        [SerializeField] private float plantBaseOffset = 0.435f;

        private PlotProgress data;
        private PlantDefinition plant;
        private bool showsStage2;

        public int Index => index;
        public PlantCatalog Catalog => catalog;
        public PlotState State => data.State;
        public PlantDefinition Plant => plant;
        /// <summary>According to GDD: water only affects sown cells.</summary>
        public bool NeedsWater => State == PlotState.Sown;
        /// <summary>How much of the needed water a sown cell has got, from 0 to 1 (saved, so it survives leaving the Garden).</summary>
        public float WaterProgress => data.WaterProgress;
        /// <summary>According to GDD: seeds only go on tilled cells.</summary>
        public bool CanSow => State == PlotState.Tilled;
        /// <summary>According to GDD: a ripe plant is harvested by clicking it.</summary>
        public bool IsRipe => State == PlotState.Ripe;
        /// <summary>Growth progress from 0 to 1 while growing, 1 when ripe.</summary>
        public float Progress => State == PlotState.Ripe ? 1f : plant != null ? Mathf.Clamp01(GrowTime / plant.GrowSeconds) : 0f;
        /// <summary>Seconds until the plant is ripe; 0 when it is not growing.</summary>
        public float RemainingSeconds => State == PlotState.Growing && plant != null ? Mathf.Max(0f, plant.GrowSeconds - GrowTime) : 0f;

        // According to GDD: timers run in real time, even while the game is closed, so growth uses UTC, not Time.time.
        private static long NowUtcMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private float GrowTime => (NowUtcMs - data.GrowStartUtcMs) / 1000f;

        public event Action<GardenPlot> StateChanged;
        /// <summary>Raised after a harvest went into the inventory: what was picked and how many.</summary>
        public event Action<GardenPlot, IngredientData, int> Harvested;

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
            if (!CanSow || seedPlant == null) return false;
            plant = seedPlant;
            data.SetPlant(seedPlant.Id);
            SetState(PlotState.Sown);
            return true;
        }

        /// <summary>
        /// Pours some water on a sown cell (a share of all the water it needs, 0..1).
        /// When the cell has got all of it, the plant starts growing. Returns true at that moment.
        /// </summary>
        public bool AddWater(float share)
        {
            if (!NeedsWater) return false;
            data.SetWaterProgress(data.WaterProgress + share);
            return data.WaterProgress >= 1f && TryWater();
        }

        /// <summary>Waters a sown cell at once and starts the growth timer.</summary>
        public bool TryWater()
        {
            if (!NeedsWater) return false;
            data.StartGrowing(NowUtcMs);
            showsStage2 = false;
            SetState(PlotState.Growing);
            return true;
        }

        /// <summary>
        /// According to GDD: puts the harvest into the inventory and frees the cell, which must be tilled again.
        /// </summary>
        public bool TryHarvest()
        {
            if (!IsRipe) return false;
            IngredientData item = plant != null ? plant.Harvest : null;
            int amount = plant != null ? plant.HarvestAmount : 0;
            if (item != null)
            {
                GameServices.Inventory?.Add(item.id, amount);
            }
            plant = null;
            data.SetPlant(null);
            SetState(PlotState.Empty);
            if (item != null)
            {
                Harvested?.Invoke(this, item, amount);
            }
            return true;
        }

        public void ForceRipe(PlantDefinition newPlant = null)
        {
            if (newPlant != null)
            {
                plant = newPlant;
                data.SetPlant(newPlant.Id);
            }
            SetState(PlotState.Ripe);
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
                UpdatePlantCollider();
            }
        }

        private void UpdatePlantCollider()
        {
            if (plantCollider == null)
            {
                return;
            }
            plantCollider.enabled = IsRipe && plantRenderer.sprite != null;
            if (plantCollider.enabled)
            {
                plantCollider.size = plantRenderer.sprite.bounds.size;
                plantCollider.offset = plantRenderer.sprite.bounds.center;
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
