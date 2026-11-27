using System.Collections.Generic;
using SproutOfDebt.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Seed Shop window (Figma "Seed Shop" flow): pick one seed, choose the amount with −/+, see your coins and the total,
    /// then "Buy for N". After a purchase a toast names what was bought, the seeds fly into the inventory and the selection resets.
    /// New seeds unlock together with their recipes. The rules live in <see cref="SeedPurchase"/>; this class only shows them.
    /// </summary>
    public class SeedShopWindow : UIWindow
    {
        [SerializeField] private ShopConfig config;

        [Header("Cards")]
        [SerializeField] private SeedCardView cardTemplate;
        [SerializeField] private ItemTooltipView tooltip;

        [Header("Selected seed")]
        [SerializeField] private GameObject hint;
        [SerializeField] private GameObject selectedRow;
        [SerializeField] private TMP_Text selectedNameText;
        [SerializeField] private TMP_Text selectedPriceText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;

        [Header("Summary")]
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private GameObject totalGroup;
        [SerializeField] private TMP_Text totalText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyLabel;

        private readonly List<SeedCardView> cards = new List<SeedCardView>();
        private readonly SeedPurchase purchase = new SeedPurchase();

        protected override void Awake()
        {
            base.Awake();
            if (minusButton != null) minusButton.onClick.AddListener(OnMinusClicked);
            if (plusButton != null) plusButton.onClick.AddListener(OnPlusClicked);
            if (buyButton != null) buyButton.onClick.AddListener(OnBuyClicked);
        }

        private void OnEnable()
        {
            if (cards.Count == 0) BuildCards();
            purchase.Clear();
            Subscribe(true);
            Refresh();
        }

        private void OnDisable()
        {
            Subscribe(false);
            if (tooltip != null) tooltip.Hide();
        }

        public void OnMinusClicked()
        {
            purchase.Decrease();
            Refresh();
        }

        public void OnPlusClicked()
        {
            purchase.Increase();
            Refresh();
        }

        public void OnBuyClicked()
        {
            SeedData seed = purchase.Selected;
            SeedCardView card = FindCard(seed);
            int bought = purchase.TryBuy();
            if (bought <= 0) return;

            // According to Figma "Seeds bought": a toast names the purchase, the window stays open and the selection resets.
            GameServices.Toasts?.Show($"+{bought} {SeedTitle(seed)}");
            if (card != null && GameServices.ItemFly != null)
            {
                Vector2 from = RectTransformUtility.WorldToScreenPoint(null, card.IconCenter);
                GameServices.ItemFly.FlyToInventory(seed.icon, bought, from);
            }
            purchase.Clear();
            Refresh();
        }

        private void BuildCards()
        {
            if (config == null || cardTemplate == null) return;
            cardTemplate.gameObject.SetActive(false);
            foreach (var seed in config.seeds)
            {
                if (seed == null) continue;
                var card = Instantiate(cardTemplate, cardTemplate.transform.parent);
                card.name = "SeedCard_" + seed.id;
                card.gameObject.SetActive(true);
                card.Bind(seed, OnCardClicked, OnCardHoverChanged);
                cards.Add(card);
            }
        }

        private void OnCardClicked(SeedCardView card)
        {
            if (purchase.Select(card.Seed)) Refresh();
        }

        private void OnCardHoverChanged(SeedCardView card, bool hovered)
        {
            if (tooltip == null) return;
            if (!hovered)
            {
                tooltip.Hide();
                return;
            }
            SeedData seed = card.Seed;
            string info = SeedPurchase.IsUnlocked(seed)
                ? GameServices.ItemHandler?.GetItemInfo(seed.id)
                : $"Unlocks at {UnlockThreshold(seed)} coins of debt paid.";
            tooltip.Show(SeedTitle(seed), info);
        }

        private void Refresh()
        {
            int coins = purchase.Coins;
            foreach (var card in cards)
            {
                SeedData seed = card.Seed;
                card.Refresh(SeedPurchase.IsUnlocked(seed), seed == purchase.Selected, seed.buyPrice <= coins);
            }

            SeedData selected = purchase.Selected;
            bool hasSelection = selected != null;
            hint.SetActive(!hasSelection);
            selectedRow.SetActive(hasSelection);
            totalGroup.SetActive(hasSelection);
            coinsText.text = coins.ToString();
            if (hasSelection)
            {
                selectedNameText.text = SeedTitle(selected);
                selectedPriceText.text = $"{selected.buyPrice} coins each";
                amountText.text = purchase.Amount.ToString();
                minusButton.interactable = purchase.CanDecrease;
                plusButton.interactable = purchase.CanIncrease;
                totalText.text = purchase.Total.ToString();
            }

            // According to Figma "not enough coins": the reason is shown above the disabled Buy button.
            bool notEnough = hasSelection && !purchase.CanAfford;
            messageText.text = notEnough ? $"Not enough coins: you need {purchase.Total} and have {coins}." : string.Empty;
            buyButton.interactable = hasSelection && purchase.CanAfford;
            buyLabel.text = hasSelection ? $"Buy for {purchase.Total}" : "Buy";
        }

        private SeedCardView FindCard(SeedData seed)
        {
            foreach (var card in cards)
            {
                if (card.Seed == seed) return card;
            }
            return null;
        }

        private static int UnlockThreshold(SeedData seed)
        {
            Treasury treasury = ShopContext.Treasury;
            return treasury != null ? treasury.GetUnlockThreshold(seed.unlockedWith) : 0;
        }

        // Figma: a seed is named after its plant ("Lavender seeds").
        private static string SeedTitle(SeedData seed) => seed.displayName + " seeds";

        private void Subscribe(bool subscribe)
        {
            var wallet = GameServices.Wallet;
            var treasury = ShopContext.Treasury;
            if (subscribe)
            {
                if (wallet != null) wallet.CoinsChanged += OnCoinsChanged;
                if (treasury != null) treasury.Changed += Refresh;
            }
            else
            {
                if (wallet != null) wallet.CoinsChanged -= OnCoinsChanged;
                if (treasury != null) treasury.Changed -= Refresh;
            }
        }

        private void OnCoinsChanged(int coins) => Refresh();
    }
}
