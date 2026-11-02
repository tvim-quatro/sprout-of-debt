using SproutOfDebt.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>Window where the player deposits any amount of coins towards the debt.</summary>
    public class TreasuryWindow : UIWindow
    {
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button allButton;
        [SerializeField] private Button depositButton;
        [SerializeField] private int step = 10;

        private int amount;

        protected override void Awake()
        {
            base.Awake();
            minusButton.onClick.AddListener(() => SetAmount(amount - step));
            plusButton.onClick.AddListener(() => SetAmount(amount + step));
            allButton.onClick.AddListener(() => SetAmount(MaxDeposit));
            depositButton.onClick.AddListener(Deposit);
        }

        private int MaxDeposit => ShopContext.Treasury != null ? ShopContext.Treasury.MaxDeposit(GameServices.Wallet) : 0;

        private void OnEnable()
        {
            if (GameServices.Wallet != null) GameServices.Wallet.CoinsChanged += OnCoinsChanged;
            amount = MaxDeposit;
            statusText.text = NextUnlockText();
            Refresh();
        }

        private void OnDisable()
        {
            if (GameServices.Wallet != null) GameServices.Wallet.CoinsChanged -= OnCoinsChanged;
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

            var unlocked = treasury.Deposit(GameServices.Wallet, amount);
            if (treasury.IsPaidOff)
            {
                statusText.text = "The debt is fully repaid! The lab is yours.";
            }
            else if (unlocked.Count > 0)
            {
                var names = new System.Collections.Generic.List<string>();
                foreach (var potion in unlocked) names.Add(potion.DisplayName);
                statusText.text = "New recipe unlocked: " + string.Join(", ", names) + "!";
            }
            else
            {
                statusText.text = NextUnlockText();
            }
            SetAmount(amount);
        }

        private string NextUnlockText()
        {
            var treasury = ShopContext.Treasury;
            if (treasury == null) return string.Empty;
            if (treasury.IsPaidOff) return "The debt is fully repaid! The lab is yours.";
            var next = treasury.GetNextMilestone();
            return next != null && next.potion != null
                ? $"Next recipe: {next.potion.DisplayName} at {next.threshold} repaid"
                : string.Empty;
        }

        private void Refresh()
        {
            var treasury = ShopContext.Treasury;
            if (treasury == null) return;
            summaryText.text = $"Repaid {treasury.Repaid} of {treasury.TotalDebt} coins\nRemaining: {treasury.Remaining}";
            amountText.text = amount.ToString();
            int max = MaxDeposit;
            minusButton.interactable = amount > 0;
            plusButton.interactable = amount < max;
            allButton.interactable = amount < max;
            depositButton.interactable = amount > 0;
        }
    }
}
