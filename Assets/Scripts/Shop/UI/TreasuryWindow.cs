using SproutOfDebt.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Window where the player pays coins towards the debt (Figma "Treasury window", 131:3903).
    /// Displays Moneylender portrait, debt progress bar, coin balance, amount selector, preview of debt after payment,
    /// and triggers toasts when paying coins, unlocking recipes, or paying off the full debt.
    /// </summary>
    public class TreasuryWindow : UIWindow
    {
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text depositLabel;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button allButton;
        [SerializeField] private Button depositButton;
        [SerializeField] private Image moneylenderPortrait;
        [SerializeField] private Sprite neutralPortrait;
        [SerializeField] private Sprite pleasedPortrait;
        [SerializeField] private int step = 10;

        private int amount;
        private bool justPaid;

        protected override void Awake()
        {
            base.Awake();
            if (minusButton != null) minusButton.onClick.AddListener(() => SetAmount(amount - step));
            if (plusButton != null) plusButton.onClick.AddListener(() => SetAmount(amount + step));
            if (allButton != null) allButton.onClick.AddListener(() => SetAmount(MaxDeposit));
            if (depositButton != null) depositButton.onClick.AddListener(Deposit);
        }

        private int MaxDeposit => ShopContext.Treasury != null ? ShopContext.Treasury.MaxDeposit(GameServices.Wallet) : 0;

        private void OnEnable()
        {
            if (GameServices.Wallet != null) GameServices.Wallet.CoinsChanged += OnCoinsChanged;
            if (ShopContext.Treasury != null) ShopContext.Treasury.Changed += Refresh;
            justPaid = false;
            amount = MaxDeposit;
            Refresh();
        }

        private void OnDisable()
        {
            if (GameServices.Wallet != null) GameServices.Wallet.CoinsChanged -= OnCoinsChanged;
            if (ShopContext.Treasury != null) ShopContext.Treasury.Changed -= Refresh;
        }

        private void OnCoinsChanged(int coins) => SetAmount(amount);

        private void SetAmount(int value)
        {
            amount = Mathf.Clamp(value, 0, MaxDeposit);
            Refresh();
        }

        private void Deposit()
        {
            var treasury = ShopContext.Treasury;
            if (treasury == null || amount <= 0) return;

            int paid = amount;
            var unlocked = treasury.Deposit(GameServices.Wallet, paid);
            justPaid = true;

            if (treasury.IsPaidOff)
            {
                GameServices.Toasts?.Show("Debt paid off! The lab is yours!");
            }
            else if (unlocked.Count > 0)
            {
                // Figma "Garden - Recipe unlocked by payment" (246:13671)
                GameServices.Toasts?.Show("New recipe: " + unlocked[0].DisplayName);
            }
            else
            {
                // Figma "Garden - Debt payment made" (246:13491)
                GameServices.Toasts?.Show($"Paid {paid} coins");
            }

            SetAmount(MaxDeposit);
        }

        private void Refresh()
        {
            var treasury = ShopContext.Treasury;
            if (treasury == null) return;

            int walletCoins = GameServices.Wallet != null ? GameServices.Wallet.Coins : 0;
            int max = MaxDeposit;
            amount = Mathf.Clamp(amount, 0, max);

            if (summaryText != null)
            {
                summaryText.text = $"{treasury.Repaid} / {treasury.TotalDebt}";
            }
            if (coinsText != null)
            {
                coinsText.text = walletCoins.ToString();
            }
            if (amountText != null)
            {
                amountText.text = amount.ToString();
            }
            if (depositLabel != null)
            {
                depositLabel.text = amount > 0 ? $"Pay {amount}" : "Pay";
            }
            if (statusText != null)
            {
                statusText.text = BuildStatusText(treasury, walletCoins);
            }
            if (moneylenderPortrait != null)
            {
                bool pleased = justPaid || treasury.IsPaidOff;
                if (pleased && pleasedPortrait != null) moneylenderPortrait.sprite = pleasedPortrait;
                else if (neutralPortrait != null) moneylenderPortrait.sprite = neutralPortrait;
            }

            if (minusButton != null) minusButton.interactable = amount > 0;
            if (plusButton != null) plusButton.interactable = amount < max;
            if (allButton != null) allButton.interactable = amount < max;
            if (depositButton != null) depositButton.interactable = amount > 0;
        }

        private string BuildStatusText(Treasury treasury, int walletCoins)
        {
            if (treasury.IsPaidOff)
            {
                return "The debt is fully repaid! The lab is yours.";
            }
            if (walletCoins <= 0)
            {
                // Figma "Garden - Treasury no coins" (246:13851)
                return "No coins. Sell potions at the Counter.";
            }
            // Figma "Garden - Treasury open" (131:3903)
            int after = Mathf.Min(treasury.TotalDebt, treasury.Repaid + amount);
            return $"Debt after payment: {after} / {treasury.TotalDebt}";
        }
    }
}
