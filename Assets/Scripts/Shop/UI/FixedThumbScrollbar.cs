using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Vertical scrollbar of a ScrollRect whose thumb keeps its Figma height (ui_scrollbar_thumb, 90 px) instead of
    /// growing and shrinking with the content, as a ScrollRect-linked Scrollbar would. Keeps both in sync both ways.
    /// The Scrollbar runs bottom to top, so its value matches the ScrollRect's vertical position (1 = top).
    /// </summary>
    [RequireComponent(typeof(Scrollbar))]
    public class FixedThumbScrollbar : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [Tooltip("Thumb height in canvas pixels (Figma: 90).")]
        [SerializeField, Min(1f)] private float thumbHeight = 90f;

        private Scrollbar scrollbar;
        private RectTransform slidingArea;

        private void Awake()
        {
            scrollbar = GetComponent<Scrollbar>();
            slidingArea = scrollbar.handleRect != null ? scrollbar.handleRect.parent as RectTransform : null;
        }

        private void OnEnable()
        {
            scrollbar.onValueChanged.AddListener(OnScrollbarMoved);
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.AddListener(OnContentMoved);
                scrollbar.SetValueWithoutNotify(scrollRect.verticalNormalizedPosition);
            }
            UpdateThumbSize();
        }

        private void OnDisable()
        {
            scrollbar.onValueChanged.RemoveListener(OnScrollbarMoved);
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnContentMoved);
            }
        }

        // The Seeds tab has a shorter list area (room for the Buy seeds button), so the thumb share changes with it.
        private void OnRectTransformDimensionsChange()
        {
            if (scrollbar != null)
            {
                UpdateThumbSize();
            }
        }

        private void OnScrollbarMoved(float value)
        {
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = value;
            }
        }

        private void OnContentMoved(Vector2 position)
        {
            scrollbar.SetValueWithoutNotify(position.y);
        }

        private void UpdateThumbSize()
        {
            float height = slidingArea != null ? slidingArea.rect.height : 0f;
            scrollbar.size = height > 0f ? Mathf.Clamp01(thumbHeight / height) : 1f;
        }
    }
}
