using TMPro;
using UnityEngine;

namespace SproutOfDebt.Laboratory.UI
{
    public class RecipeStepView : MonoBehaviour
    {
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text stepText;

        public void Show(int number, string text)
        {
            gameObject.SetActive(true);
            if (numberText != null)
            {
                numberText.text = number.ToString();
            }
            if (stepText != null)
            {
                stepText.text = text;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
