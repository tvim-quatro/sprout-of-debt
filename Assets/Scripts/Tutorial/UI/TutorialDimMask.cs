using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Tutorial.UI
{
    /// <summary>
    /// Dark dim overlay with a rectangular cutout hole over the interactable target,
    /// plus a gold highlight frame (#FFE08A) around the cutout.
    /// When an item is focused, the screen is darkened with high opacity (85%).
    /// </summary>
    public class TutorialDimMask : MonoBehaviour
    {
        [Header("Dim Panels")]
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private RectTransform fullDimPanel;

        [Header("Dim Opacity")]
        [SerializeField, Range(0f, 1f)] private float fullDimAlpha = 0.70f;
        [SerializeField, Range(0f, 1f)] private float focusedDimAlpha = 0.85f;

        [Header("Highlight")]
        [SerializeField] private RectTransform highlightFrame;
        [SerializeField] private float highlightPadding = 6f;

        private RectTransform dimRect;

        private void Awake()
        {
            dimRect = GetComponent<RectTransform>();
        }

        public void ShowFullscreenDim()
        {
            gameObject.SetActive(true);
            SetHoleActive(false);
            SetPanelsAlpha(fullDimAlpha);
            if (fullDimPanel != null)
            {
                fullDimPanel.gameObject.SetActive(true);
            }
            if (highlightFrame != null)
            {
                highlightFrame.gameObject.SetActive(false);
            }
        }

        public void SetCutout(Rect holeInCanvasSpace)
        {
            gameObject.SetActive(true);
            if (fullDimPanel != null)
            {
                fullDimPanel.gameObject.SetActive(false);
            }
            SetHoleActive(true);
            SetPanelsAlpha(focusedDimAlpha);

            if (dimRect == null)
            {
                dimRect = GetComponent<RectTransform>();
            }

            Rect frameBounds = dimRect != null ? dimRect.rect : new Rect(-960f, -540f, 1920f, 1080f);
            float cLeft = frameBounds.xMin;
            float cRight = frameBounds.xMax;
            float cBottom = frameBounds.yMin;
            float cTop = frameBounds.yMax;

            const float padding = 6f;
            float minX = Mathf.Clamp(holeInCanvasSpace.xMin - padding, cLeft, cRight);
            float maxX = Mathf.Clamp(holeInCanvasSpace.xMax + padding, cLeft, cRight);
            float minY = Mathf.Clamp(holeInCanvasSpace.yMin - padding, cBottom, cTop);
            float maxY = Mathf.Clamp(holeInCanvasSpace.yMax + padding, cBottom, cTop);

            // Left panel: [cLeft, minX] across full height [cBottom, cTop]
            if (leftPanel != null)
            {
                float w = Mathf.Max(0f, minX - cLeft);
                float h = Mathf.Max(0f, cTop - cBottom);
                leftPanel.anchoredPosition = new Vector2((cLeft + minX) * 0.5f, (cBottom + cTop) * 0.5f);
                leftPanel.sizeDelta = new Vector2(w, h);
            }

            // Right panel: [maxX, cRight] across full height [cBottom, cTop]
            if (rightPanel != null)
            {
                float w = Mathf.Max(0f, cRight - maxX);
                float h = Mathf.Max(0f, cTop - cBottom);
                rightPanel.anchoredPosition = new Vector2((maxX + cRight) * 0.5f, (cBottom + cTop) * 0.5f);
                rightPanel.sizeDelta = new Vector2(w, h);
            }

            // Top panel: [minX, maxX] from maxY to cTop
            if (topPanel != null)
            {
                float w = Mathf.Max(0f, maxX - minX);
                float h = Mathf.Max(0f, cTop - maxY);
                topPanel.anchoredPosition = new Vector2((minX + maxX) * 0.5f, (maxY + cTop) * 0.5f);
                topPanel.sizeDelta = new Vector2(w, h);
            }

            // Bottom panel: [minX, maxX] from cBottom to minY
            if (bottomPanel != null)
            {
                float w = Mathf.Max(0f, maxX - minX);
                float h = Mathf.Max(0f, minY - cBottom);
                bottomPanel.anchoredPosition = new Vector2((minX + maxX) * 0.5f, (cBottom + minY) * 0.5f);
                bottomPanel.sizeDelta = new Vector2(w, h);
            }

            // Highlight frame: surrounds the hole with padding
            if (highlightFrame != null)
            {
                highlightFrame.gameObject.SetActive(true);
                highlightFrame.anchoredPosition = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
                highlightFrame.sizeDelta = new Vector2((maxX - minX) + highlightPadding * 2f, (maxY - minY) + highlightPadding * 2f);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (highlightFrame != null)
            {
                highlightFrame.gameObject.SetActive(false);
            }
        }

        private void SetHoleActive(bool active)
        {
            if (topPanel != null) topPanel.gameObject.SetActive(active);
            if (bottomPanel != null) bottomPanel.gameObject.SetActive(active);
            if (leftPanel != null) leftPanel.gameObject.SetActive(active);
            if (rightPanel != null) rightPanel.gameObject.SetActive(active);
        }

        private void SetPanelsAlpha(float alpha)
        {
            Color c = new Color(0f, 0f, 0f, alpha);
            SetPanelColor(fullDimPanel, c);
            SetPanelColor(topPanel, c);
            SetPanelColor(bottomPanel, c);
            SetPanelColor(leftPanel, c);
            SetPanelColor(rightPanel, c);
        }

        private static void SetPanelColor(RectTransform panel, Color color)
        {
            if (panel != null && panel.TryGetComponent<Image>(out var img))
            {
                img.color = color;
            }
        }
    }
}
