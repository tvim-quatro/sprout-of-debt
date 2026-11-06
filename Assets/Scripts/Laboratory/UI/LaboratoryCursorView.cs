using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SproutOfDebt.Laboratory.UI
{
    public class LaboratoryCursorView : MonoBehaviour
    {
        [SerializeField] private LaboratoryDragController controller;
        [SerializeField] private Image cursorImage;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite hoverSprite;
        [SerializeField] private Sprite grabSprite;
        [Tooltip("Where the arrow tip sits inside the default sprite, 0-1 from the bottom-left corner.")]
        [SerializeField] private Vector2 defaultPivot = new Vector2(0.2f, 0.85f);
        [SerializeField] private Vector2 handPivot = new Vector2(0.5f, 0.5f);

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.CursorStateChanged += OnCursorStateChanged;
            }
            Cursor.visible = false;
            if (cursorImage != null)
            {
                cursorImage.enabled = true;
            }
            Apply(LaboratoryCursorState.Default);
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.CursorStateChanged -= OnCursorStateChanged;
            }
            Cursor.visible = true;
            if (cursorImage != null)
            {
                cursorImage.enabled = false;
            }
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse != null && cursorImage != null)
            {
                cursorImage.rectTransform.position = mouse.position.ReadValue();
            }
        }

        private void OnCursorStateChanged(LaboratoryCursorState state)
        {
            Apply(state);
        }

        private void Apply(LaboratoryCursorState state)
        {
            if (cursorImage == null)
            {
                return;
            }

            switch (state)
            {
                case LaboratoryCursorState.Grab:
                    cursorImage.sprite = grabSprite;
                    cursorImage.rectTransform.pivot = handPivot;
                    break;
                case LaboratoryCursorState.Hover:
                    cursorImage.sprite = hoverSprite;
                    cursorImage.rectTransform.pivot = handPivot;
                    break;
                default:
                    cursorImage.sprite = defaultSprite;
                    cursorImage.rectTransform.pivot = defaultPivot;
                    break;
            }

            bool hasSprite = cursorImage.sprite != null;
            cursorImage.enabled = hasSprite;
            Cursor.visible = !hasSprite;
        }
    }
}
