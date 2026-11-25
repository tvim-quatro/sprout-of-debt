using SproutOfDebt.Core;
using SproutOfDebt.Items;
using SproutOfDebt.Shop;
using SproutOfDebt.Shop.UI;
using UnityEngine;

namespace SproutOfDebt.Tutorial.Hooks
{
    /// <summary>
    /// Monitors counter customer interaction and debt repayment during tutorial.
    /// </summary>
    public class ShopTutorialHook : MonoBehaviour
    {
        [SerializeField] private ShopCounterController counterController;
        [SerializeField] private TreasuryWindow treasuryWindow;
        [SerializeField] private CustomerDialogue dialog;

        private void Awake()
        {
            FindReferences();
        }

        private void Start()
        {
            FindReferences();
            WireListeners();

            // Ensure player has sufficient potions so customer request can always be sold
            if (GameServices.Tutorial != null && !GameServices.Tutorial.IsCompleted)
            {
                if (GameServices.Inventory != null && GameServices.Inventory.GetCount("potion_calming") < 3)
                {
                    GameServices.Inventory.Add("potion_calming", 3 - GameServices.Inventory.GetCount("potion_calming"));
                }
            }

            // If entering ShopCounter during LabGoToCounter, advance to CounterCustomerRequest
            if (GameServices.Tutorial != null && GameServices.Tutorial.CurrentStep == TutorialStep.LabGoToCounter)
            {
                GameServices.Tutorial.SetStep(TutorialStep.CounterCustomerRequest);
            }
        }

        private void OnEnable()
        {
            FindReferences();
            WireListeners();
        }

        private void OnDestroy()
        {
            UnwireListeners();
        }

        private void FindReferences()
        {
            if (counterController == null) counterController = FindFirstObjectByType<ShopCounterController>(FindObjectsInactive.Include);
            if (treasuryWindow == null) treasuryWindow = FindFirstObjectByType<TreasuryWindow>(FindObjectsInactive.Include);
            if (dialog == null) dialog = FindFirstObjectByType<CustomerDialogue>(FindObjectsInactive.Include);
        }

        private void WireListeners()
        {
            if (counterController != null)
            {
                counterController.PotionSold -= OnPotionSold;
                counterController.PotionSold += OnPotionSold;
            }

            if (dialog != null && dialog.SellButton != null)
            {
                dialog.SellButton.onClick.RemoveListener(NotifyPotionSold);
                dialog.SellButton.onClick.AddListener(NotifyPotionSold);
            }

            if (ShopContext.Treasury != null)
            {
                ShopContext.Treasury.Changed -= OnTreasuryChanged;
                ShopContext.Treasury.Changed += OnTreasuryChanged;
            }
        }

        private void UnwireListeners()
        {
            if (counterController != null)
            {
                counterController.PotionSold -= OnPotionSold;
            }

            if (dialog != null && dialog.SellButton != null)
            {
                dialog.SellButton.onClick.RemoveListener(NotifyPotionSold);
            }

            if (ShopContext.Treasury != null)
            {
                ShopContext.Treasury.Changed -= OnTreasuryChanged;
            }
        }

        private void Update()
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted)
            {
                return;
            }

            TutorialStep step = GameServices.Tutorial.CurrentStep;

            // Continually ensure references and listeners are hooked up
            if (dialog == null || counterController == null || treasuryWindow == null)
            {
                FindReferences();
                WireListeners();
            }

            if (step == TutorialStep.CounterCustomerRequest || step == TutorialStep.CounterSellPotion)
            {
                // If potion was sold and wallet has >= 25 coins, advance step immediately
                if (GameServices.Wallet != null && GameServices.Wallet.Coins >= 25)
                {
                    GameServices.Tutorial.SetStep(TutorialStep.CounterPotionSold);
                }
            }
            else if (step == TutorialStep.CounterClickDebtBar)
            {
                if (treasuryWindow != null && treasuryWindow.IsOpen)
                {
                    GameServices.Tutorial.CompleteStep(TutorialStep.CounterClickDebtBar);
                }
            }
            else if (step == TutorialStep.CounterPayDebt)
            {
                if (ShopContext.Treasury != null && ShopContext.Treasury.Repaid > 0)
                {
                    if (treasuryWindow != null && treasuryWindow.IsOpen)
                    {
                        treasuryWindow.Close();
                    }
                    GameServices.Tutorial.CompleteStep(TutorialStep.CounterPayDebt);
                }
                else if (treasuryWindow != null && !treasuryWindow.IsOpen && (ShopContext.Treasury == null || ShopContext.Treasury.Repaid == 0))
                {
                    // If treasury window was closed without paying debt, prompt to open it again
                    GameServices.Tutorial.SetStep(TutorialStep.CounterClickDebtBar);
                }
            }
        }

        private void OnPotionSold(PotionDefinition potion, int quantity)
        {
            NotifyPotionSold();
        }

        public void NotifyPotionSold()
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) return;

            // Ensure player has at least 25 coins for the debt repayment milestone requested by Moneylender/Imp
            if (GameServices.Wallet != null && GameServices.Wallet.Coins < 25)
            {
                GameServices.Wallet.Add(25 - GameServices.Wallet.Coins);
            }

            if (GameServices.Tutorial.CurrentStep == TutorialStep.CounterCustomerRequest ||
                GameServices.Tutorial.CurrentStep == TutorialStep.CounterSellPotion)
            {
                GameServices.Tutorial.SetStep(TutorialStep.CounterPotionSold);
            }
        }

        private void OnTreasuryChanged()
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted) return;

            if (GameServices.Tutorial.CurrentStep == TutorialStep.CounterPayDebt)
            {
                if (ShopContext.Treasury != null && ShopContext.Treasury.Repaid > 0)
                {
                    if (treasuryWindow != null && treasuryWindow.IsOpen)
                    {
                        treasuryWindow.Close();
                    }
                    GameServices.Tutorial.CompleteStep(TutorialStep.CounterPayDebt);
                }
            }
        }
    }
}
