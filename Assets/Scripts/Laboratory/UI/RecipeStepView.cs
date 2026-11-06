using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Laboratory.UI
{
    public class RecipeStepView : MonoBehaviour
    {
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text stepText;
        [SerializeField] private TMP_Text counterText;
        [SerializeField] private Image highlight;
        [SerializeField] private GameObject doneMark;

        public void Show(int number, string text, string counter = null)
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
            if (counterText != null)
            {
                counterText.gameObject.SetActive(!string.IsNullOrEmpty(counter));
                counterText.text = counter;
            }
        }

        public void SetState(bool current, bool done)
        {
            if (highlight != null)
            {
                highlight.enabled = current;
            }
            if (numberText != null)
            {
                numberText.gameObject.SetActive(!done || doneMark == null);
            }
            if (doneMark != null)
            {
                doneMark.SetActive(done);
            }
            if (stepText != null)
            {
                stepText.fontStyle = current ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
