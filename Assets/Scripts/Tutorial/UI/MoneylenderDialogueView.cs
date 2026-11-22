using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Tutorial.UI
{
    /// <summary>
    /// Displays Moneylender dialogue at the bottom of the screen with portrait frame, nameplate and Next button.
    /// </summary>
    public class MoneylenderDialogueView : MonoBehaviour
    {
        [Header("Portraits")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Sprite neutralPortrait;
        [SerializeField] private Sprite sternPortrait;
        [SerializeField] private Sprite pleasedPortrait;

        [Header("Content")]
        [SerializeField] private TMP_Text nameplateText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button nextButton;

        public event Action NextClicked;

        private void Awake()
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            }
        }

        public void Show(string line, CharacterMood mood)
        {
            gameObject.SetActive(true);

            if (dialogueText != null)
            {
                dialogueText.text = line;
            }

            if (nameplateText != null)
            {
                nameplateText.text = "Moneylender";
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = mood switch
                {
                    CharacterMood.Stern => sternPortrait,
                    CharacterMood.Pleased => pleasedPortrait,
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
