using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SproutOfDebt.UI
{
    /// <summary>
    /// Switches a button's label color while the pointer is over it, for buttons whose hover sprite is light
    /// (Figma nav buttons: cream text on the dark normal face, dark text on the light hover / active face).
    /// Leaves the label alone while the button is not interactable, so the active nav button keeps its color.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonLabelHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color normalColor = new Color(0.98f, 0.93f, 0.82f);
        [SerializeField] private Color hoverColor = new Color(0.24f, 0.15f, 0.09f);

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetColor(hoverColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetColor(normalColor);
        }

        private void SetColor(Color color)
        {
            if (label != null && button != null && button.interactable)
            {
                label.color = color;
            }
        }
    }
}
