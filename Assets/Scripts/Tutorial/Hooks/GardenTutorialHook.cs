using System.Collections;
using SproutOfDebt.Core;
using SproutOfDebt.Core.Save;
using SproutOfDebt.Garden;
using SproutOfDebt.Items;
using SproutOfDebt.Shop.UI;
using UnityEngine;

namespace SproutOfDebt.Tutorial.Hooks
{
    /// <summary>
    /// Monitors player actions in the Garden and advances tutorial steps.
    /// </summary>
    public class GardenTutorialHook : MonoBehaviour
    {
        [SerializeField] private GardenPlot[] plots;
        [SerializeField] private PlantCatalog plantCatalog;

        private void Start()
        {
            if (plots == null || plots.Length == 0)
            {
                plots = FindObjectsByType<GardenPlot>(FindObjectsSortMode.InstanceID);
            }

            if (plantCatalog == null && plots != null && plots.Length > 0 && plots[0] != null)
            {
                plantCatalog = plots[0].Catalog;
            }

            foreach (var plot in plots)
            {
                if (plot != null)
                {
                    plot.StateChanged += OnPlotStateChanged;
                    plot.Harvested += OnPlotHarvested;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var plot in plots)
            {
                if (plot != null)
                {
                    plot.StateChanged -= OnPlotStateChanged;
                    plot.Harvested -= OnPlotHarvested;
                }
            }
        }

        private void Update()
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted)
            {
                return;
            }

            TutorialStep step = GameServices.Tutorial.CurrentStep;
            var drag = FindFirstObjectByType<GardenDragController>();
            var rake = FindFirstObjectByType<Rake>();
            var can = FindFirstObjectByType<WateringCan>();
            var packet = FindFirstObjectByType<SeedPacket>();
            var inventory = FindFirstObjectByType<InventoryWindow>();

            bool isHoldingRake = (rake != null && rake.IsHeld) || (drag != null && drag.HeldTool is Rake);
            bool isHoldingCan = (can != null && can.IsHeld) || (drag != null && drag.HeldTool is WateringCan);
            bool isHoldingSeeds = (packet != null && packet.IsHeld) || (drag != null && drag.HeldTool is SeedPacket);
            bool isInventoryOpen = inventory != null && inventory.IsOpen;

            GardenPlot targetPlot = (plots != null && plots.Length > 0) ? plots[0] : null;

            if (step == TutorialStep.GardenRake)
            {
                if (isHoldingRake)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.GardenRake);
                }
            }
            else if (step == TutorialStep.GardenTill)
            {
                // If user dropped the rake without tilling, prompt to pick up rake again
                if (!isHoldingRake && targetPlot != null && targetPlot.State == PlotState.Empty)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.GardenRake);
                }
            }
            else if (step == TutorialStep.GardenOpenInventory)
            {
                if (isInventoryOpen)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.GardenOpenInventory);
                }
                else if (isHoldingSeeds)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.GardenSow);
                }
            }
            else if (step == TutorialStep.GardenPickSeeds)
            {
                if (isHoldingSeeds)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.GardenPickSeeds);
                }
                // If user closed inventory without picking seeds, prompt to open inventory again
                else if (!isInventoryOpen)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.GardenOpenInventory);
                }
            }
            else if (step == TutorialStep.GardenSow)
            {
                // If user dropped seeds without sowing, prompt to open inventory / pick seeds again
                if (!isHoldingSeeds && targetPlot != null && targetPlot.State == PlotState.Tilled)
                {
                    GameServices.Tutorial.SetStep(isInventoryOpen ? TutorialStep.GardenPickSeeds : TutorialStep.GardenOpenInventory);
                }
            }
            else if (step == TutorialStep.GardenWateringCan)
            {
                if (isHoldingCan)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.GardenWateringCan);
                }
            }
            else if (step == TutorialStep.GardenWater)
            {
                // If user dropped the watering can without watering, prompt to pick it up again
                if (!isHoldingCan && targetPlot != null && targetPlot.NeedsWater)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.GardenWateringCan);
                }
            }
            else if (step == TutorialStep.GardenHarvestFirst)
            {
                if (GameServices.Inventory != null && GameServices.Inventory.GetCount("lavender") >= 1)
                {
                    CheckAllHarvested();
                }
            }
            else if (step == TutorialStep.GardenHarvestRest)
            {
                CheckAllHarvested();
            }
        }

        private void OnPlotStateChanged(GardenPlot plot)
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) return;

            TutorialStep step = GameServices.Tutorial.CurrentStep;

            // Soil tilled: advances past rake/till directly to opening inventory
            if ((step == TutorialStep.GardenRake || step == TutorialStep.GardenTill) && plot.State == PlotState.Tilled)
            {
                GameServices.Tutorial.SetStep(TutorialStep.GardenOpenInventory);
            }
            // Seed sown: advances past picking/sowing directly to watering can
            else if ((step == TutorialStep.GardenTill || step == TutorialStep.GardenOpenInventory || step == TutorialStep.GardenPickSeeds || step == TutorialStep.GardenSow)
                     && (plot.State == PlotState.Sown || plot.State == PlotState.Growing))
            {
                GameServices.Tutorial.SetStep(TutorialStep.GardenWateringCan);
            }
            // Watered: advances past watering can / water to growth timer
            else if ((step == TutorialStep.GardenSow || step == TutorialStep.GardenWateringCan || step == TutorialStep.GardenWater)
                     && (plot.State == PlotState.Growing || plot.WaterProgress >= 1f))
            {
                GameServices.Tutorial.SetStep(TutorialStep.GardenGrowthTimer);
                StartCoroutine(RipenTutorialBed());
            }
        }

        private void OnPlotHarvested(GardenPlot plot, IngredientData ingredient, int count)
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) return;

            TutorialStep step = GameServices.Tutorial.CurrentStep;

            if (step == TutorialStep.GardenHarvestFirst)
            {
                GameServices.Tutorial.CompleteStep(TutorialStep.GardenHarvestFirst);
            }
            else if (step == TutorialStep.GardenHarvestRest)
            {
                CheckAllHarvested();
            }
        }

        private void CheckAllHarvested()
        {
            bool anyRipe = false;
            if (plots != null)
            {
                foreach (var p in plots)
                {
                    if (p != null && p.IsRipe)
                    {
                        anyRipe = true;
                        break;
                    }
                }
            }
            if (!anyRipe)
            {
                if (GameServices.Tutorial != null &&
                    (GameServices.Tutorial.CurrentStep == TutorialStep.GardenHarvestFirst || GameServices.Tutorial.CurrentStep == TutorialStep.GardenHarvestRest))
                {
                    GameServices.Tutorial.SetStep(TutorialStep.GardenGoToLab);
                }
            }
        }

        private IEnumerator RipenTutorialBed()
        {
            // Wait for player to read Growth timer hint and click Next
            while (GameServices.Tutorial != null && GameServices.Tutorial.CurrentStep == TutorialStep.GardenGrowthTimer)
            {
                yield return null;
            }

            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) yield break;

            // Make plot 0 ripe
            if (plots != null && plots.Length > 0 && plots[0] != null)
            {
                plots[0].ForceRipe();
            }

            // Spawn ready lavender on plot 1, ready moonpetal on plot 2
            PlantDefinition lavender = plantCatalog != null ? plantCatalog.Find("lavender") : null;
            PlantDefinition moonpetal = plantCatalog != null ? plantCatalog.Find("moonpetal") : null;

            if (plots != null && plots.Length > 1 && plots[1] != null)
            {
                plots[1].ForceRipe(lavender);
            }
            if (plots != null && plots.Length > 2 && plots[2] != null)
            {
                plots[2].ForceRipe(moonpetal);
            }
        }
    }
}
