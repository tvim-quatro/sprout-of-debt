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

        // The cell the player tilled in the tutorial; it is the one that ripens first.
        private GardenPlot workPlot;

        private void Start()
        {
            if (plots == null || plots.Length == 0)
            {
                plots = FindObjectsByType<GardenPlot>(FindObjectsSortMode.None);
            }
            // Plot_1 first, as in Figma: the tutorial highlights Plot_1, so it is the cell the player works on.
            System.Array.Sort(plots, (a, b) => a.Index.CompareTo(b.Index));

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
            var drag = FindFirstObjectByType<GardenDragController>();
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted)
            {
                if (drag != null) drag.LiftToolRestriction();
                return;
            }

            TutorialStep step = GameServices.Tutorial.CurrentStep;
            RestrictTools(drag, step);
            var rake = FindFirstObjectByType<Rake>();
            var can = FindFirstObjectByType<WateringCan>();
            var packet = FindFirstObjectByType<SeedPacket>();
            var inventory = FindFirstObjectByType<InventoryWindow>();

            bool isHoldingRake = (rake != null && rake.IsHeld) || (drag != null && drag.HeldTool is Rake);
            bool isHoldingCan = (can != null && can.IsHeld) || (drag != null && drag.HeldTool is WateringCan);
            bool isHoldingSeeds = (packet != null && packet.IsHeld) || (drag != null && drag.HeldTool is SeedPacket);
            bool isInventoryOpen = inventory != null && inventory.IsOpen;

            GardenPlot targetPlot = WorkPlot;

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

        // Onboarding shows one cell only: the tools work just on the highlighted cell until it is watered,
        // and on no cell while the rest of the garden part of the tutorial runs.
        private void RestrictTools(GardenDragController drag, TutorialStep step)
        {
            if (drag == null) return;
            if (step >= TutorialStep.LabOpenRecipe)
            {
                drag.LiftToolRestriction();
                return;
            }
            drag.RestrictToolsTo(step <= TutorialStep.GardenWater ? WorkPlot : null);
        }

        private void OnPlotStateChanged(GardenPlot plot)
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) return;

            TutorialStep step = GameServices.Tutorial.CurrentStep;

            // Soil tilled: advances past rake/till directly to opening inventory
            if ((step == TutorialStep.GardenRake || step == TutorialStep.GardenTill) && plot.State == PlotState.Tilled)
            {
                workPlot = plot;
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
                // Figma "Garden - Rest of the bed ripe": only after the first harvest the bed gives one more
                // lavender and a moonpetal ("While I was busy, the bed gave me...").
                RipenBonusPlots();
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
            // Wait for Next on the Growth timer hint, or for the timer to run out if the player waits:
            // then the "ripe" step shows by itself.
            while (GameServices.Tutorial != null && GameServices.Tutorial.CurrentStep == TutorialStep.GardenGrowthTimer)
            {
                if (WorkPlot != null && WorkPlot.IsRipe)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.GardenGrowthTimer);
                    yield break;
                }
                yield return null;
            }

            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) yield break;

            // Figma "Garden - Ripe plant highlighted": only the cell the player sowed ripens.
            GardenPlot sown = WorkPlot;
            if (sown != null)
            {
                sown.ForceRipe();
            }
        }

        private GardenPlot WorkPlot => workPlot != null ? workPlot : (plots != null && plots.Length > 0 ? plots[0] : null);

        // The bonus plants go to the next free cells (Plot_2, Plot_3 in Figma), never onto a cell the player sowed.
        private void RipenBonusPlots()
        {
            if (plots == null || plantCatalog == null) return;

            PlantDefinition[] bonus = { plantCatalog.Find("lavender"), plantCatalog.Find("moonpetal") };
            int next = 0;
            for (int i = 0; i < plots.Length && next < bonus.Length; i++)
            {
                GardenPlot plot = plots[i];
                if (plot == null || plot == WorkPlot || (plot.State != PlotState.Empty && plot.State != PlotState.Tilled)) continue;
                if (bonus[next] != null)
                {
                    plot.ForceRipe(bonus[next]);
                }
                next++;
            }
        }
    }
}
