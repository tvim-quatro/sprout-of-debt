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

            if (target == HighlightTarget.TreasuryAmount)
            {
                return TryGetTreasuryAmountRect(canvasRect, out canvasSpaceRect);
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
            if (worldCamera != null && TutorialWorldTargets.TryGetBounds(target, out Bounds bounds))
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
                HighlightTarget.OrderPanel => GameObject.Find("OrderPanel"),
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
                    return FindChildRecursive(container, "OrderPanel");

                case HighlightTarget.SellButton:
                    return FindChildRecursive(container, "Button_Sell") ?? FindChildRecursive(container, "Button_Primary");

                case HighlightTarget.BrewButton:
                    return FindChildRecursive(container, "BrewButton") ?? FindChildRecursive(container, "Button_Brew");

                case HighlightTarget.BrewingSteps:
                    return FindChildRecursive(container, "GuidePanel");

                case HighlightTarget.TreasuryWindow:
                    return FindChildRecursive(container, "TreasuryWindow");

                case HighlightTarget.TreasuryPayment:
                    return FindChildRecursive(container, "Button_Deposit") ?? FindChildRecursive(container, "Button_Pay");

                case HighlightTarget.RequiredIngredientSlot:
                    return FindRequiredIngredientSlot(container);

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

        private static RectTransform FindRequiredIngredientSlot(RectTransform container)
        {
            var station = Object.FindFirstObjectByType<SproutOfDebt.Laboratory.BrewingStation>();
            string neededId = null;
            if (station != null && station.CurrentStep != null && station.CurrentStep.Ingredient != null)
            {
                neededId = station.CurrentStep.Ingredient.id;
            }

            SproutOfDebt.Shop.UI.InventorySlotView first = null;
            foreach (var slot in container.GetComponentsInChildren<SproutOfDebt.Shop.UI.InventorySlotView>())
            {
                if (slot == null || !slot.gameObject.activeInHierarchy || slot.ItemId == null)
                {
                    continue;
                }
                if (slot.ItemId == neededId)
                {
                    return slot.transform as RectTransform;
                }
                if (first == null)
                {
                    first = slot;
                }
            }
            return first != null ? first.transform as RectTransform : null;
        }

        // Figma "Counter - Treasury payment": one frame around the whole "Pay  -  amount  +" row.
        private static bool TryGetTreasuryAmountRect(RectTransform canvasRect, out Rect rect)
        {
            rect = Rect.zero;
            RectTransform window = FindUiTarget(HighlightTarget.TreasuryWindow, canvasRect);
            if (window == null || !window.gameObject.activeInHierarchy) return false;

            bool found = false;
            foreach (string part in new[] { "PayLabel", "Button_Minus", "Amount", "Button_Plus" })
            {
                RectTransform child = FindChildRecursive(window, part);
                if (child == null || !child.gameObject.activeInHierarchy) continue;
                Rect partRect = GetCanvasSpaceRect(child, canvasRect);
                rect = found ? Rect.MinMaxRect(Mathf.Min(rect.xMin, partRect.xMin), Mathf.Min(rect.yMin, partRect.yMin),
                    Mathf.Max(rect.xMax, partRect.xMax), Mathf.Max(rect.yMax, partRect.yMax)) : partRect;
                found = true;
            }
            return found;
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

        internal static RectTransform FindChildRecursive(Transform parent, string name)
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
