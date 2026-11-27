using System.Collections;
using SproutOfDebt.Items;
using SproutOfDebt.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Dialogue window next to the customer: their name, their story typed out letter by letter,
    /// then the order with the Sell / Sorry answers.
    /// A click on the window, the arrow or Space skips the typing and moves on.
    /// </summary>
    public class CustomerDialogue : MonoBehaviour
    {
        private static readonly Color StockOkColor = new Color(0.24f, 0.15f, 0.09f);
        private static readonly Color StockLowColor = new Color(0.62f, 0.13f, 0.1f);

        [Tooltip("Optional portrait of the customer")]
        [SerializeField] private Image portrait;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button advanceArea;
        [SerializeField] private Button nextButton;
        [Tooltip("Shared typing animation on the body text (the same as in the onboarding lines).")]
        [SerializeField] private TypewriterText typewriter;

        [Header("Order")]
        [SerializeField] private GameObject requestRow;
        [SerializeField] private Image potionIcon;
        [SerializeField] private TMP_Text requestText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text stockText;

        [Header("Answers")]
        [SerializeField] private GameObject answers;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button sorryButton;

        private CustomerData customer;
        private bool advanceRequested;

        public Button SellButton => sellButton;
        public Button SorryButton => sorryButton;

        private void Awake()
        {
            advanceArea.onClick.AddListener(RequestAdvance);
            nextButton.onClick.AddListener(RequestAdvance);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) RequestAdvance();
        }

        private void RequestAdvance()
        {
            advanceRequested = true;
        }

        public void Open(CustomerData data)
        {
            customer = data;
            gameObject.SetActive(true);
            speakerText.text = data.displayName;
            bodyText.text = string.Empty;
            SetMood(CustomerMood.Neutral);
            requestRow.SetActive(false);
            answers.SetActive(false);
            nextButton.gameObject.SetActive(false);
        }

        public void SetMood(CustomerMood mood)
        {
            if (customer == null || portrait == null) return;
            portrait.sprite = customer.GetSprite(mood);
        }

        /// <summary>Types the text out. With waitForNext, then waits until the player moves on.</summary>
        public IEnumerator Say(string text, bool waitForNext)
        {
            requestRow.SetActive(false);
            answers.SetActive(false);
            nextButton.gameObject.SetActive(false);
            yield return Type(text);

            if (!waitForNext) yield break;
            nextButton.gameObject.SetActive(true);
            advanceRequested = false;
            yield return new WaitUntil(() => advanceRequested);
            nextButton.gameObject.SetActive(false);
        }

        /// <summary>Types the request line, then shows the order with its total price and the answers.</summary>
        public IEnumerator ShowRequest(string line, PotionDefinition potion, int quantity, int totalPrice)
        {
            potionIcon.sprite = potion.Icon;
            requestText.text = $"{quantity} × {potion.DisplayName}";
            priceText.text = totalPrice.ToString();
            yield return Say(line, false);
            requestRow.SetActive(true);
            answers.SetActive(true);
        }

        public void SetStock(int inStock, int requested)
        {
            bool enough = inStock >= requested;
            stockText.text = $"In stock: {inStock}";
            stockText.color = enough ? StockOkColor : StockLowColor;
            sellButton.interactable = enough;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private IEnumerator Type(string text)
        {
            advanceRequested = false;
            typewriter.Play(text);
            while (typewriter.IsTyping)
            {
                if (advanceRequested) typewriter.Complete();
                yield return null;
            }
            advanceRequested = false;
        }
    }
}
