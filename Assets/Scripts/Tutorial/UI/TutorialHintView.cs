using System;
using SproutOfDebt.UI;
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
        [Tooltip("Types the thought out; a click on Next while it types shows the whole line first.")]
        [SerializeField] private TypewriterText typewriter;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private Button nextButton;

        public event Action NextClicked;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextClicked);
            }
        }

        /// <summary>Places the hint where the step's Figma screen has it: top-left corner in the 1920x1080 frame.</summary>
        public void SetTopLeftPosition(Vector2 figmaTopLeft)
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            if (rectTransform == null) return;

            rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(figmaTopLeft.x, -figmaTopLeft.y);
        }

        public void Show(string thought, string action, bool showAction, bool showNext, CharacterMood mood)
        {
            gameObject.SetActive(true);

            if (typewriter != null)
            {
                typewriter.Play(thought);
            }
            else if (thoughtText != null)
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

        private void OnNextClicked()
        {
            if (typewriter != null && typewriter.IsTyping)
            {
                typewriter.Complete();
                return;
            }
            NextClicked?.Invoke();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
