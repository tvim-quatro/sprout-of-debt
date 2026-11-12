using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Visual view of one inventory grid slot (150x144 px):
    /// centered item icon (84x84 px) and bottom-right count badge (58x32 px).
    /// </summary>
    public class InventorySlotView : MonoBehaviour
    {
        [SerializeField] private Image slotBackground;
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject countBadge;
        [SerializeField] private TMP_Text countText;

        public void SetItem(Sprite icon, int count)
        {
            if (icon == null || count <= 0)
            {
                SetEmpty();
                return;
            }

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
            if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(false);
            }

            if (countBadge != null)
            {
                countBadge.SetActive(false);
            }
        }
    }
}
