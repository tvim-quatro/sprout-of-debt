using System.Collections;
using SproutOfDebt.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Dialogue box at the bottom left of the counter (Figma: UI Kit "Dialogue box"): the visitor's name on the
    /// nameplate and their words typed out letter by letter. A click on the box or Space finishes the typing.
    /// The order itself and the answers live in <see cref="OrderPanel"/>.
    /// </summary>
    public class CustomerDialogue : MonoBehaviour
    {
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button advanceArea;
        [Tooltip("Shared typing animation on the body text (the same as in the onboarding lines).")]
        [SerializeField] private TypewriterText typewriter;

        private bool skipRequested;

        private void Awake()
        {
            advanceArea.onClick.AddListener(RequestSkip);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) RequestSkip();
        }

        private void RequestSkip()
        {
            skipRequested = true;
        }

        public void Open(CustomerData data)
        {
            gameObject.SetActive(true);
            speakerText.text = data.displayName;
            bodyText.text = string.Empty;
        }

        /// <summary>Types the text out; a click or Space shows it at once.</summary>
        public IEnumerator Say(string text)
        {
            skipRequested = false;
            typewriter.Play(text);
            while (typewriter.IsTyping)
            {
                if (skipRequested) typewriter.Complete();
                yield return null;
            }
            skipRequested = false;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
