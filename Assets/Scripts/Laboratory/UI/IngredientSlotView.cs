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
        [SerializeField] private Color enoughColor = new Color(0.239f, 0.149f, 0.09f);
        [SerializeField] private Color missingColor = new Color(0.62f, 0.129f, 0.102f);

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
