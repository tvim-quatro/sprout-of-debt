using System;
using SproutOfDebt.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Visual view of one inventory grid slot (150x144 px):
    /// centered item icon (84x84 px) and bottom-right count badge (58x32 px).
    /// Reports hover (for the item tooltip) and a press on an item the location lets the player drag out.
    /// </summary>
    public class InventorySlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private Image slotBackground;
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject countBadge;
        [SerializeField] private TMP_Text countText;

        private string itemId;
        private bool hovered;

        /// <summary>The pointer entered (true) or left (false) a filled slot.</summary>
        public event Action<InventorySlotView, bool> HoverChanged;
        /// <summary>The player pressed a slot whose item can be dragged into the location.</summary>
        public event Action<InventorySlotView> DragRequested;

        /// <summary>Id of the item in the slot; null when the slot is empty.</summary>
        public string ItemId => itemId;

        private bool CanDrag => itemId != null && GameServices.ItemHandler != null && GameServices.ItemHandler.CanDrag(itemId);

        public void SetItem(string id, Sprite icon, int count)
        {
            if (icon == null || count <= 0)
            {
                SetEmpty();
                return;
            }
            itemId = id;

            if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(true);
                itemIcon.sprite = icon;
                itemIcon.color = Color.white;
            }

            if (countBadge != null)
            {
                countBadge.SetActive(true);
            }

            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }

        public void SetEmpty()
        {
            OnPointerExit(null);
            itemId = null;

            if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(false);
            }

            if (countBadge != null)
            {
                countBadge.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (itemId == null)
            {
                return;
            }
            hovered = true;
            // Figma cursors: the open hand only over items that can be dragged out.
            GameServices.GameCursor?.Show(CanDrag ? CursorState.Hover : CursorState.Default);
            HoverChanged?.Invoke(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!hovered)
            {
                return;
            }
            hovered = false;
            GameServices.GameCursor?.Show(CursorState.Default);
            HoverChanged?.Invoke(this, false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && CanDrag)
            {
                DragRequested?.Invoke(this);
            }
        }

        private void OnDisable()
        {
            OnPointerExit(null);
        }
    }
}
