using UnityEngine;

namespace SproutOfDebt.Tutorial.UI
{
    /// <summary>
    /// Finds canvas-space rect for UI RectTransforms or world-space scene objects during tutorial highlighting.
    /// Supports targets across multiple Canvases (HUD, RecipeBook, ShopCounter) and world space props.
    /// </summary>
    public static class TutorialTargetLocator
    {
        public static bool TryGetTargetRect(HighlightTarget target, RectTransform canvasRect, Camera worldCamera, out Rect canvasSpaceRect)
        {
            canvasSpaceRect = Rect.zero;
            if (target == HighlightTarget.None || canvasRect == null)
            {
                return false;
            }

            // 1. Check UI objects (in the current canvas or other canvases)
            RectTransform uiTarget = FindUiTarget(target, canvasRect);
            if (uiTarget != null && uiTarget.gameObject.activeInHierarchy)
            {
                canvasSpaceRect = GetCanvasSpaceRect(uiTarget, canvasRect);
                return true;
            }

            // 2. Check world-space targets
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
            if (worldCamera != null && TryGetWorldTargetBounds(target, out Bounds bounds))
            {
                canvasSpaceRect = WorldBoundsToCanvasRect(bounds, worldCamera, canvasRect);
                return true;
            }

            return false;
        }

        private static RectTransform FindUiTarget(HighlightTarget target, RectTransform root)
        {
            // First check within root (HUD canvas frame)
            RectTransform found = FindUiTargetInHierarchy(target, root);
            if (found != null && found.gameObject.activeInHierarchy)
            {
                return found;
            }

            // Search all canvases in the active scene
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas == null) continue;
                var rt = canvas.transform as RectTransform;
                if (rt == null || rt == root) continue;

                found = FindUiTargetInHierarchy(target, rt);
                if (found != null && found.gameObject.activeInHierarchy)
                {
                    return found;
                }
            }

            // Fallback: direct GameObject search
            GameObject fallbackGo = target switch
            {
                HighlightTarget.BrewButton => GameObject.Find("BrewButton") ?? GameObject.Find("Button_Brew"),
                HighlightTarget.SellButton => GameObject.Find("Button_Sell") ?? GameObject.Find("Button_Primary"),
                HighlightTarget.OrderPanel => GameObject.Find("CustomerDialogue") ?? GameObject.Find("OrderPanel"),
                HighlightTarget.TreasuryPayment => GameObject.Find("Button_Deposit") ?? GameObject.Find("Button_Pay"),
                _ => null
            };

            if (fallbackGo != null && fallbackGo.activeInHierarchy && fallbackGo.transform is RectTransform fallbackRt)
            {
                return fallbackRt;
            }

            return null;
        }

        private static RectTransform FindUiTargetInHierarchy(HighlightTarget target, RectTransform container)
        {
            if (container == null) return null;

            switch (target)
            {
                case HighlightTarget.DebtBar:
                    return FindChildRecursive(container, "DebtBar") ?? FindChildRecursive(container, "Debt bar (click: Treasury)");

                case HighlightTarget.InventoryButton:
                    return FindChildRecursive(container, "Button_Inventory") ?? FindChildRecursive(container, "Inventory button (Tab / I)");

                case HighlightTarget.CoinCounter:
                    return FindChildRecursive(container, "Coins");

                case HighlightTarget.NavLaboratory:
                    return FindChildRecursive(container, "Button_Nav_Laboratory") ?? FindChildRecursive(container, "Nav Laboratory");

                case HighlightTarget.NavCounter:
                    return FindChildRecursive(container, "Button_Nav_Counter") ?? FindChildRecursive(container, "Nav Counter");

                case HighlightTarget.OrderPanel:
                    return FindChildRecursive(container, "CustomerDialogue") ?? FindChildRecursive(container, "OrderPanel");

                case HighlightTarget.SellButton:
                    return FindChildRecursive(container, "Button_Sell") ?? FindChildRecursive(container, "Button_Primary");

                case HighlightTarget.BrewButton:
                    return FindChildRecursive(container, "BrewButton") ?? FindChildRecursive(container, "Button_Brew");

                case HighlightTarget.TreasuryPayment:
                    return FindChildRecursive(container, "Button_Deposit") ?? FindChildRecursive(container, "Button_Pay");

                case HighlightTarget.LavenderSeedsSlot:
                    var slots = container.GetComponentsInChildren<SproutOfDebt.Shop.UI.InventorySlotView>();
                    foreach (var s in slots)
                    {
                        if (s != null && s.gameObject.activeInHierarchy && (s.ItemId == "seed_lavender" || s.ItemId != null))
                        {
                            return s.transform as RectTransform;
                        }
                    }
                    return FindChildRecursive(container, "Slot_Template(Clone)") ?? FindChildRecursive(container, "Slot_LavenderSeeds");

                default:
                    return null;
            }
        }

        private static bool TryGetWorldTargetBounds(HighlightTarget target, out Bounds bounds)
        {
            bounds = new Bounds();

            if (target == HighlightTarget.CandleAndCauldron)
            {
                var brazier = Object.FindFirstObjectByType<SproutOfDebt.Laboratory.Brazier>();
                var candle = Object.FindFirstObjectByType<SproutOfDebt.Laboratory.CandleTool>();
                var r1 = brazier != null ? brazier.GetComponent<Renderer>() : null;
                var r2 = candle != null ? candle.GetComponent<Renderer>() : null;
                if (r1 != null && r2 != null)
                {
                    bounds = r1.bounds;
                    bounds.Encapsulate(r2.bounds);
                    bounds.Expand(0.2f);
                    return true;
                }
                if (r1 != null) { bounds = r1.bounds; bounds.Expand(0.2f); return true; }
                if (r2 != null) { bounds = r2.bounds; bounds.Expand(0.2f); return true; }
            }
            else if (target == HighlightTarget.CauldronBowl)
            {
                var bowl = Object.FindFirstObjectByType<SproutOfDebt.Laboratory.Bowl>();
                if (bowl != null && bowl.TryGetComponent<Renderer>(out var bowlRenderer))
                {
                    bounds = bowlRenderer.bounds;
                    bounds.Expand(0.2f);
                    return true;
                }
            }
            else if (target == HighlightTarget.SpoonAndBowl)
            {
                var spoon = Object.FindFirstObjectByType<SproutOfDebt.Laboratory.SpoonTool>();
                var bowl = Object.FindFirstObjectByType<SproutOfDebt.Laboratory.Bowl>();
                var r1 = spoon != null ? spoon.GetComponent<Renderer>() : null;
                var r2 = bowl != null ? bowl.GetComponent<Renderer>() : null;
                if (r1 != null && r2 != null)
                {
                    bounds = r1.bounds;
                    bounds.Encapsulate(r2.bounds);
                    bounds.Expand(0.2f);
                    return true;
                }
                if (r1 != null) { bounds = r1.bounds; bounds.Expand(0.2f); return true; }
                if (r2 != null) { bounds = r2.bounds; bounds.Expand(0.2f); return true; }
            }

            GameObject go = target switch
            {
                HighlightTarget.Rake => Object.FindFirstObjectByType<SproutOfDebt.Garden.Rake>()?.gameObject ?? GameObject.Find("Rake") ?? GameObject.Find("Rake (resting)"),
                HighlightTarget.WateringCan => Object.FindFirstObjectByType<SproutOfDebt.Garden.WateringCan>()?.gameObject ?? GameObject.Find("WateringCan") ?? GameObject.Find("Watering can (resting)"),
                HighlightTarget.Plot1 => GameObject.Find("Plot_1") ?? Object.FindFirstObjectByType<SproutOfDebt.Garden.GardenPlot>()?.gameObject ?? GameObject.Find("Plot 1"),
                HighlightTarget.GrowthTimer => GameObject.Find("Plot_1") ?? Object.FindFirstObjectByType<SproutOfDebt.Garden.GardenPlot>()?.gameObject,
                HighlightTarget.RipePlant1 => GameObject.Find("Plot_1") ?? Object.FindFirstObjectByType<SproutOfDebt.Garden.GardenPlot>()?.gameObject,
                HighlightTarget.RestOfBedRipe => GameObject.Find("GardenBed") ?? GameObject.Find("Bed") ?? GameObject.Find("Garden bed"),
                HighlightTarget.RecipeBook => GameObject.Find("lab_book_closed") ?? GameObject.Find("RecipeBook") ?? GameObject.Find("Recipe book (closed)"),
                _ => null
            };

            if (go == null) return false;

            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return false;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            bounds.Expand(0.1f);
            return true;
        }

        private static Rect GetCanvasSpaceRect(RectTransform target, RectTransform canvasRect)
        {
            Canvas targetCanvas = target.GetComponentInParent<Canvas>();
            Canvas destCanvas = canvasRect.GetComponentInParent<Canvas>();

            Camera targetCam = (targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? targetCanvas.worldCamera : null;
            Camera destCam = (destCanvas != null && destCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? destCanvas.worldCamera : null;

            Vector3[] worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);

            Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(targetCam, worldCorners[0]);
            Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(targetCam, worldCorners[2]);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenMin, destCam, out Vector2 localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenMax, destCam, out Vector2 localMax);

            float xMin = Mathf.Min(localMin.x, localMax.x);
            float xMax = Mathf.Max(localMin.x, localMax.x);
            float yMin = Mathf.Min(localMin.y, localMax.y);
            float yMax = Mathf.Max(localMin.y, localMax.y);

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private static Rect WorldBoundsToCanvasRect(Bounds bounds, Camera cam, RectTransform canvasRect)
        {
            if (cam == null) cam = Camera.main;
            Canvas destCanvas = canvasRect.GetComponentInParent<Canvas>();
            Camera destCam = (destCanvas != null && destCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? destCanvas.worldCamera : null;

            Vector3[] worldCorners =
            {
                new Vector3(bounds.min.x, bounds.min.y, bounds.center.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.center.z)
            };

            Vector2 screenMin = cam.WorldToScreenPoint(worldCorners[0]);
            Vector2 screenMax = cam.WorldToScreenPoint(worldCorners[1]);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenMin, destCam, out Vector2 localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenMax, destCam, out Vector2 localMax);

            float xMin = Mathf.Min(localMin.x, localMax.x);
            float xMax = Mathf.Max(localMin.x, localMax.x);
            float yMin = Mathf.Min(localMin.y, localMax.y);
            float yMax = Mathf.Max(localMin.y, localMax.y);

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private static RectTransform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name && parent is RectTransform rt) return rt;
            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
