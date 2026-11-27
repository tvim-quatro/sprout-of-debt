using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SproutOfDebt.UI
{
    /// <summary>
    /// The parts of the UI Kit button states that a sprite swap can't show (Figma: "Button", "Icon button",
    /// "HUD / Debt bar"): pressed moves the content down, disabled fades the button or greys its label,
    /// hover can show an extra object such as an outline. One component for every button, set up by the
    /// prefab builders, so all buttons react the same way.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public class ButtonStateStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Pressed")]
        [Tooltip("Label or icon moved down while the button is held. Leave empty if the state has no offset.")]
        [SerializeField] private RectTransform content;
        [Tooltip("Figma: the pressed state has a 6 px top padding.")]
        [SerializeField] private float pressedOffset = 6f;

        [Header("Hover")]
        [Tooltip("Shown while the pointer is over the button (Figma: the debt bar's hover outline).")]
        [SerializeField] private GameObject hoverObject;

        [Header("Disabled")]
        [Tooltip("Faded while the button is disabled (Figma: Icon button disabled = 40% opacity).")]
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.4f;
        [Tooltip("Label that turns grey while the button is disabled (Figma: Button disabled).")]
        [SerializeField] private TMP_Text disabledLabel;
        [SerializeField] private Color disabledLabelColor = new Color32(0x47, 0x42, 0x3D, 0xFF);

        private Selectable selectable;
        private Vector2 contentHome;
        private Color labelColor;
        private bool hovered;
        private bool held;
        private bool interactable = true;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            if (content != null)
            {
                contentHome = content.anchoredPosition;
            }
            if (disabledLabel != null)
            {
                labelColor = disabledLabel.color;
            }
            interactable = selectable.IsInteractable();
        }

        private void OnEnable()
        {
            hovered = false;
            held = false;
            Apply();
        }

        // Selectable has no event for interactable changes, so compare one bool per frame.
        private void LateUpdate()
        {
            bool now = selectable.IsInteractable();
            if (now == interactable)
            {
                return;
            }
            interactable = now;
            Apply();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            Apply();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            Apply();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            held = true;
            Apply();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            held = false;
            Apply();
        }

        // Same rule as Selectable: pressed while the button is held and the pointer is over it.
        private void Apply()
        {
            if (content != null)
            {
                bool pressed = interactable && held && hovered;
                content.anchoredPosition = contentHome + (pressed ? new Vector2(0f, -pressedOffset) : Vector2.zero);
            }
            if (hoverObject != null)
            {
                hoverObject.SetActive(interactable && hovered);
            }
            if (fadeGroup != null)
            {
                fadeGroup.alpha = interactable ? 1f : disabledAlpha;
            }
            if (disabledLabel != null)
            {
                disabledLabel.color = interactable ? labelColor : disabledLabelColor;
            }
        }
    }
}
