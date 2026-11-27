using System;
using SproutOfDebt.UI;
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
        [Tooltip("Types the line out; a click on Next while it types shows the whole line first.")]
        [SerializeField] private TypewriterText typewriter;
        [SerializeField] private Button nextButton;

        public event Action NextClicked;

        private void Awake()
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextClicked);
            }
        }

        public void Show(string line, CharacterMood mood)
        {
            gameObject.SetActive(true);

            if (typewriter != null)
            {
                typewriter.Play(line);
            }
            else if (dialogueText != null)
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
