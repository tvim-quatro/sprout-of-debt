using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Tutorial.UI
{
    /// <summary>
    /// Displays Imp tutorial hint card with portrait frame, thought line, action line, and optional Next button.
    /// </summary>
    public class TutorialHintView : MonoBehaviour
    {
        [Header("Portraits")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Sprite neutralPortrait;
        [SerializeField] private Sprite worriedPortrait;
        [SerializeField] private Sprite happyPortrait;

        [Header("Content")]
        [SerializeField] private TMP_Text thoughtText;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private Button nextButton;

        public event Action NextClicked;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            }
        }

        public void SetBottomPosition(bool atBottom)
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            if (rectTransform == null) return;

            if (atBottom)
            {
                // Figma: x: 440, y: 840, w: 1052, h: 218 (bottom center)
                rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0.5f, 0f);
                rectTransform.anchoredPosition = new Vector2(0f, 30f);
            }
            else
            {
                // Figma: x: 440, y: 170, w: 1052, h: 218 (top center)
                rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = new Vector2(0f, -170f);
            }
        }

        public void Show(string thought, string action, bool showAction, bool showNext, CharacterMood mood)
        {
            gameObject.SetActive(true);

            if (thoughtText != null)
            {
                thoughtText.text = thought;
            }

            if (actionText != null)
            {
                actionText.gameObject.SetActive(showAction && !string.IsNullOrEmpty(action));
                actionText.text = action;
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(showNext);
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = mood switch
                {
                    CharacterMood.Worried => worriedPortrait,
                    CharacterMood.Happy => happyPortrait,
                    _ => neutralPortrait
                };
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
