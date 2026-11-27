using UnityEngine;

namespace SproutOfDebt.Tutorial.UI
{
    /// <summary>
    /// World-space bounds of tutorial targets in the location scenes (garden tools and plots, laboratory props).
    /// </summary>
    public static class TutorialWorldTargets
    {
        public static bool TryGetBounds(HighlightTarget target, out Bounds bounds)
        {
            bounds = new Bounds();

            // Laboratory highlights use the tools' resting places, so the cutout stays the same size
            // while the player drags the candle or the spoon.
            if (target == HighlightTarget.CandleAndCauldron)
            {
                return TryGetLabBounds(Object.FindFirstObjectByType<SproutOfDebt.Laboratory.Brazier>(),
                    new[] { Object.FindFirstObjectByType<SproutOfDebt.Laboratory.CandleTool>() }, out bounds);
            }
            if (target == HighlightTarget.CauldronBowl)
            {
                return TryGetLabBounds(Object.FindFirstObjectByType<SproutOfDebt.Laboratory.Bowl>(),
                    new SproutOfDebt.Laboratory.LaboratoryTool[0], out bounds);
            }
            if (target == HighlightTarget.SpoonAndBowl)
            {
                return TryGetLabBounds(Object.FindFirstObjectByType<SproutOfDebt.Laboratory.Bowl>(),
                    new[] { Object.FindFirstObjectByType<SproutOfDebt.Laboratory.SpoonTool>() }, out bounds);
            }

            // Garden plot targets follow the Figma cutouts: the cell, its status indicator, the plant.
            if (TryGetPlotTargetBounds(target, out bounds))
            {
                return true;
            }

            GameObject go = target switch
            {
                HighlightTarget.Rake => Object.FindFirstObjectByType<SproutOfDebt.Garden.Rake>()?.gameObject ?? GameObject.Find("Rake") ?? GameObject.Find("Rake (resting)"),
                HighlightTarget.WateringCan => Object.FindFirstObjectByType<SproutOfDebt.Garden.WateringCan>()?.gameObject ?? GameObject.Find("WateringCan") ?? GameObject.Find("Watering can (resting)"),
                HighlightTarget.RestOfBedRipe => GameObject.Find("GardenBed") ?? GameObject.Find("Bed") ?? GameObject.Find("Garden bed"),
                HighlightTarget.RecipeBook => GameObject.Find("lab_book_closed") ?? GameObject.Find("RecipeBook") ?? GameObject.Find("Recipe book (closed)"),
                _ => null
            };

            return go != null && TryGetSpriteBounds(go, out bounds);
        }

        private static bool TryGetLabBounds(Component station, SproutOfDebt.Laboratory.LaboratoryTool[] tools, out Bounds bounds)
        {
            bounds = new Bounds();
            bool hasBounds = station != null && station.TryGetComponent(out Renderer renderer)
                && AddRenderer(renderer, ref bounds, false);
            foreach (var tool in tools)
            {
                if (tool != null)
                {
                    hasBounds = Add(tool.RestBounds, ref bounds, hasBounds);
                }
            }
            if (hasBounds)
            {
                bounds.Expand(0.2f);
            }
            return hasBounds;
        }

        private static bool TryGetPlotTargetBounds(HighlightTarget target, out Bounds bounds)
        {
            bounds = new Bounds();
            bool isPlotTarget = target == HighlightTarget.Plot1 || target == HighlightTarget.PlotStatus1 ||
                                target == HighlightTarget.GrowthTimer || target == HighlightTarget.RipePlant1;
            if (!isPlotTarget) return false;

            GameObject plot = GameObject.Find("Plot_1") ?? Object.FindFirstObjectByType<SproutOfDebt.Garden.GardenPlot>()?.gameObject;
            if (plot == null) return false;

            bool found = false;
            switch (target)
            {
                case HighlightTarget.Plot1:
                    // Only the soil cell itself, not the plant or the effects.
                    found = AddRenderer(plot.GetComponent<SpriteRenderer>(), ref bounds, found);
                    break;
                case HighlightTarget.PlotStatus1:
                    found = AddStatusPart(plot.transform, "Badge", ref bounds, found);
                    break;
                case HighlightTarget.GrowthTimer:
                    found = AddStatusPart(plot.transform, "Badge", ref bounds, found);
                    found = AddStatusPart(plot.transform, "Timer", ref bounds, found);
                    found = AddRenderer(FindPlant(plot.transform), ref bounds, found);
                    break;
                case HighlightTarget.RipePlant1:
                    found = AddRenderer(plot.GetComponent<SpriteRenderer>(), ref bounds, found);
                    found = AddRenderer(FindPlant(plot.transform), ref bounds, found);
                    found = AddStatusPart(plot.transform, "Badge", ref bounds, found);
                    break;
            }
            return found;
        }

        private static SpriteRenderer FindPlant(Transform plot)
        {
            Transform plant = plot.Find("Plant");
            return plant != null ? plant.GetComponent<SpriteRenderer>() : null;
        }

        // A part of the plot's world-space status indicator (Badge or Timer), only while it is shown.
        private static bool AddStatusPart(Transform plot, string partName, ref Bounds bounds, bool hasBounds)
        {
            var status = plot.GetComponentInChildren<SproutOfDebt.Garden.UI.PlotStatusView>();
            if (status == null || !status.TryGetComponent<Canvas>(out var canvas) || !canvas.enabled) return hasBounds;

            RectTransform part = TutorialTargetLocator.FindChildRecursive(status.transform, partName);
            if (part == null || !part.gameObject.activeInHierarchy) return hasBounds;

            Vector3[] corners = new Vector3[4];
            part.GetWorldCorners(corners);
            var partBounds = new Bounds(corners[0], Vector3.zero);
            partBounds.Encapsulate(corners[2]);
            return Add(partBounds, ref bounds, hasBounds);
        }

        private static bool AddRenderer(Renderer renderer, ref Bounds bounds, bool hasBounds)
        {
            if (renderer == null || !renderer.enabled) return hasBounds;
            if (renderer is SpriteRenderer sprite && sprite.sprite == null) return hasBounds;
            return Add(renderer.bounds, ref bounds, hasBounds);
        }

        private static bool Add(Bounds part, ref Bounds bounds, bool hasBounds)
        {
            if (hasBounds)
            {
                bounds.Encapsulate(part);
            }
            else
            {
                bounds = part;
            }
            return true;
        }

        // Only the visible sprites: particle effects (water, seeds, clods) have stale bounds that
        // stretched the cutout over neighbours, e.g. the rake while the watering can was highlighted.
        private static bool TryGetSpriteBounds(GameObject go, out Bounds bounds)
        {
            bounds = new Bounds();
            bool found = false;
            foreach (var sprite in go.GetComponentsInChildren<SpriteRenderer>())
            {
                found = AddRenderer(sprite, ref bounds, found);
            }
            return found;
        }
    }
}
