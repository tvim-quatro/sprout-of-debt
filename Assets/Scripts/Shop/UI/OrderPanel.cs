using SproutOfDebt.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Order panel to the right of the visitor (Figma: Counter components, "Order panel"): the potion, the amount,
    /// how many the player has and the payment, with the Sell / "Sorry, we're out" answers.
    /// Pays = sell price × amount. When the player has fewer potions than asked, the count turns red and Sell is
    /// disabled. Hidden after the player answers.
    /// </summary>
    public class OrderPanel : MonoBehaviour
    {
        private static readonly Color CountOkColor = new Color(0.24f, 0.15f, 0.09f);
        private static readonly Color CountLowColor = new Color(0.78f, 0.12f, 0.08f);

        [SerializeField] private Image potionIcon;
        [SerializeField] private TMP_Text potionNameText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text haveText;
        [SerializeField] private TMP_Text paysText;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button sorryButton;

        public Button SellButton => sellButton;
        public Button SorryButton => sorryButton;

        public void Show(PotionDefinition potion, int quantity, int totalPrice)
        {
            potionIcon.sprite = potion.Icon;
            potionNameText.text = potion.DisplayName;
            amountText.text = "×" + quantity;
            paysText.text = totalPrice.ToString();
            gameObject.SetActive(true);
        }

        public void SetStock(int have, int requested)
        {
            bool enough = have >= requested;
            haveText.text = have.ToString();
            haveText.color = enough ? CountOkColor : CountLowColor;
            sellButton.interactable = enough;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
