using SproutOfDebt.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Laboratory.UI
{
    public class IngredientSlotView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Color enoughColor = new Color(0.24f, 0.15f, 0.09f);
        [SerializeField] private Color missingColor = new Color(0.7f, 0.15f, 0.1f);

        public void Show(IngredientData ingredient, int have, int need)
        {
            gameObject.SetActive(true);
            if (icon != null)
            {
                icon.sprite = ingredient.icon;
            }
            if (countText != null)
            {
                countText.text = have + "/" + need;
                countText.color = have >= need ? enoughColor : missingColor;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
