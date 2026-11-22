using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Tooltip over an inventory slot (Figma "Item tooltip"): the item name and one line of info.
    /// Appears next to the cursor after a short hover and stays inside the 16:9 frame.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ItemTooltipView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text infoText;

        [Header("Behaviour")]
        [Tooltip("Hover time before the tooltip appears, in seconds.")]
        [SerializeField, Min(0f)] private float showDelay = 0.35f;
        [Tooltip("Top-left corner of the tooltip relative to the cursor tip, in frame pixels (Figma: 28 px right, 32 px down).")]
        [SerializeField] private Vector2 cursorOffset = new Vector2(28f, -32f);

        private RectTransform rect;
        private RectTransform area;
        private CanvasGroup group;
        private Camera canvasCamera;
        private float showTime = -1f;

        private void Awake()
        {
            rect = (RectTransform)transform;
            area = (RectTransform)rect.parent;
            group = GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            Canvas canvas = GetComponentInParent<Canvas>();
            canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Hide();
        }

        /// <summary>Shows the tooltip after the hover delay. The info line is hidden when info is empty.</summary>
        public void Show(string title, string info)
        {
            nameText.text = title;
            bool hasInfo = !string.IsNullOrEmpty(info);
            infoText.gameObject.SetActive(hasInfo);
            infoText.text = hasInfo ? info : string.Empty;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            rect.SetAsLastSibling();
            showTime = Time.unscaledTime + showDelay;
            group.alpha = 0f;
            enabled = true;
        }

        public void Hide()
        {
            showTime = -1f;
            if (group != null)
            {
                group.alpha = 0f;
            }
            enabled = false;
        }

        private void LateUpdate()
        {
            if (Mouse.current == null)
            {
                return;
            }
            FollowCursor(Mouse.current.position.ReadValue());
            if (group.alpha < 1f && Time.unscaledTime >= showTime)
            {
                group.alpha = 1f;
            }
        }

        // Next to the cursor, moved back inside the frame when it would stick out of it.
        private void FollowCursor(Vector2 screenPoint)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPoint, canvasCamera, out Vector2 cursor))
            {
                return;
            }
            Rect bounds = area.rect;
            Vector2 size = rect.rect.size;
            Vector2 topLeft = cursor + cursorOffset;
            topLeft.x = Mathf.Clamp(topLeft.x, bounds.xMin, bounds.xMax - size.x);
            topLeft.y = Mathf.Clamp(topLeft.y, bounds.yMin + size.y, bounds.yMax);
            // The tooltip pivot is its top-left corner; anchoredPosition counts from the anchor at the area center.
            rect.anchoredPosition = topLeft - bounds.center;
        }
    }
}
