using System.Collections;
using System.Collections.Generic;
using SproutOfDebt.Core;
using SproutOfDebt.Items;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Shop Counter loop: while this location is active, customers arrive one at a time, tell why they need a potion
    /// and ask for 1–3 of an unlocked one; the player sells or apologizes. The next customer arrives a few seconds
    /// later. Customers come in waves with breaks in between, see <see cref="CustomerWaves"/>.
    /// When the player runs out of potions, the "Out of potions" button ends the wave early: the customer at the
    /// counter leaves and the usual break countdown starts.
    /// </summary>
    public class ShopCounterController : MonoBehaviour
    {
        private enum Answer
        {
            None,
            Sell,
            Sorry
        }

        [SerializeField] private ShopConfig config;
        [SerializeField] private CustomerView customerView;
        [SerializeField] private CustomerDialogue dialog;
        [SerializeField] private CoinBurst coinBurst;
        [Tooltip("Ends the current wave early when the player has nothing left to sell")]
        [SerializeField] private Button outOfPotionsButton;
        [Tooltip("Coins paid for the first sale during the tutorial (the tutorial dialogue counts on this amount)")]
        [SerializeField] private int tutorialSalePrice = 25;
        [Tooltip("Delay before the first customer when the player comes to the counter")]
        [SerializeField] private float firstCustomerDelay = 1f;

        private Answer answer;
        private CustomerData lastCustomer;
        private PotionDefinition lastPotion;
        private int lastQuantity;
        private PotionDefinition requestedPotion;
        private int requestedQuantity;
        private int requestedTotalPrice;
        private CustomerData currentCustomer;
        private bool currentCustomerAnswered;

        public event System.Action<PotionDefinition, int> PotionSold;

        private static bool IsTutorialRunning => GameServices.Tutorial != null && !GameServices.Tutorial.IsCompleted;

        private void Awake()
        {
            dialog.SellButton.onClick.AddListener(() => answer = Answer.Sell);
            dialog.SorryButton.onClick.AddListener(() => answer = Answer.Sorry);
        }

        private void OnEnable()
        {
            dialog.Close();
            customerView.HideImmediate();
            coinBurst.ClearPile();
            currentCustomer = null;
            RefreshOutOfPotionsButton();
            StartCoroutine(CustomerLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            StopWatchingStock();
        }

        private IEnumerator CustomerLoop()
        {
            yield return new WaitForSeconds(firstCustomerDelay);
            while (true)
            {
                bool canServe = CustomerWaves.Current.CanServe();
                RefreshOutOfPotionsButton();
                if (!canServe)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    continue;
                }
                yield return ServeCustomer();
                yield return new WaitForSeconds(config.nextCustomerDelay);
            }
        }

        private IEnumerator ServeCustomer()
        {
            var treasury = ShopContext.Treasury;
            var wallet = GameServices.Wallet;
            var inventory = GameServices.Inventory;
            List<PotionDefinition> potions = treasury != null ? treasury.GetUnlockedPotions() : new List<PotionDefinition>();
            if (potions.Count == 0 || config.customers.Count == 0 || wallet == null || inventory == null)
            {
                yield break;
            }

            bool isTutorial = IsTutorialRunning;
            var customer = PickCustomer();
            currentCustomer = customer;
            currentCustomerAnswered = false;
            PickOrder(potions);
            // Every customer pays their own price: base price times a random multiplier from their range.
            requestedTotalPrice = isTutorial
                ? tutorialSalePrice
                : customer.RollUnitPrice(requestedPotion) * requestedQuantity;

            yield return customerView.Enter(customer);

            dialog.Open(customer);
            string story = customer.GetStory(requestedPotion);

            if (isTutorial)
            {
                // In tutorial, display the order and SELL button immediately without waiting for an extra advance click
                string line = !string.IsNullOrEmpty(story) ? story : CustomerData.PickLine(customer.requestLines, "Could I please get a Calming Draught?");
                yield return dialog.ShowRequest(line, requestedPotion, requestedQuantity, requestedTotalPrice);
            }
            else
            {
                if (!string.IsNullOrEmpty(story)) yield return dialog.Say(story, true);
                yield return dialog.ShowRequest(CustomerData.PickLine(customer.requestLines, "Could I get..."),
                    requestedPotion, requestedQuantity, requestedTotalPrice);
            }

            answer = Answer.None;
            RefreshStock();
            inventory.ItemChanged += OnItemChanged;

            yield return new WaitUntil(() => answer != Answer.None);
            StopWatchingStock();
            currentCustomerAnswered = true;
            CustomerWaves.Current.OnCustomerServed();
            RefreshOutOfPotionsButton();

            if (answer == Answer.Sell && inventory.TryRemove(requestedPotion.Id, requestedQuantity))
            {
                int earned = requestedTotalPrice;
                wallet.Add(earned);
                SetMood(CustomerMood.Happy);
                coinBurst.Play(customerView.Center, earned);
                PotionSold?.Invoke(requestedPotion, requestedQuantity);
                yield return dialog.Say(CustomerData.PickLine(customer.thanksLines, "Thank you!"), false);
            }
            else
            {
                SetMood(CustomerMood.Disappointed);
                yield return dialog.Say(CustomerData.PickLine(customer.disappointedLines, "Oh... maybe next time."), false);
            }

            yield return new WaitForSeconds(config.replyDuration);
            dialog.Close();
            yield return customerView.Leave();
            coinBurst.ClearPile();
            currentCustomer = null;
        }

        /// <summary>"Out of potions" button: no more customers in this wave, the break countdown starts now.</summary>
        public void OnOutOfPotionsClicked()
        {
            if (IsTutorialRunning || CustomerWaves.Current.IsOnBreak) return;

            CustomerWaves.Current.EndWaveEarly();
            RefreshOutOfPotionsButton();
            StopAllCoroutines();
            StopWatchingStock();
            StartCoroutine(CloseForBreak());
        }

        private IEnumerator CloseForBreak()
        {
            if (currentCustomer != null) yield return SendCurrentCustomerAway();
            yield return CustomerLoop();
        }

        /// <summary>The customer at the counter leaves: disappointed if they were still waiting for their order.</summary>
        private IEnumerator SendCurrentCustomerAway()
        {
            var customer = currentCustomer;
            if (!currentCustomerAnswered)
            {
                dialog.Open(customer);
                SetMood(CustomerMood.Disappointed);
                yield return dialog.Say(CustomerData.PickLine(customer.disappointedLines, "Oh... maybe next time."), false);
                yield return new WaitForSeconds(config.replyDuration);
            }
            dialog.Close();
            yield return customerView.Leave();
            coinBurst.ClearPile();
            currentCustomer = null;
        }

        private void RefreshOutOfPotionsButton()
        {
            if (outOfPotionsButton == null) return;
            // During the tutorial the shop must stay open for the scripted first sale.
            outOfPotionsButton.interactable = !IsTutorialRunning && !CustomerWaves.Current.IsOnBreak;
        }

        private void SetMood(CustomerMood mood)
        {
            customerView.SetMood(mood);
            dialog.SetMood(mood);
        }

        /// <summary>Random order that differs from the previous one: another potion if possible, else another quantity.</summary>
        private void PickOrder(List<PotionDefinition> potions)
        {
            if (IsTutorialRunning)
            {
                requestedPotion = potions.Find(p => p != null && p.Id == "potion_calming") ?? potions[0];
                requestedQuantity = 1;
                lastPotion = requestedPotion;
                lastQuantity = requestedQuantity;
                return;
            }

            int maxQuantity = Mathf.Max(config.minQuantity, config.maxQuantity);
            do
            {
                requestedPotion = potions[Random.Range(0, potions.Count)];
            } while (potions.Count > 1 && requestedPotion == lastPotion);

            do
            {
                requestedQuantity = Random.Range(config.minQuantity, maxQuantity + 1);
            } while (requestedPotion == lastPotion && requestedQuantity == lastQuantity && maxQuantity > config.minQuantity);

            lastPotion = requestedPotion;
            lastQuantity = requestedQuantity;
        }

        [ContextMenu("Debug/Skip customer break")]
        private void DebugSkipBreak()
        {
            CustomerWaves.Current.SkipBreak();
        }

        private CustomerData PickCustomer()
        {
            var customers = config.customers;
            CustomerData customer;
            do
            {
                customer = customers[Random.Range(0, customers.Count)];
            } while (customers.Count > 1 && customer == lastCustomer);
            lastCustomer = customer;
            return customer;
        }

        private void OnItemChanged(string itemId, int count)
        {
            if (requestedPotion != null && itemId == requestedPotion.Id) RefreshStock();
        }

        private void RefreshStock()
        {
            dialog.SetStock(GameServices.Inventory.GetCount(requestedPotion.Id), requestedQuantity);
        }

        private void StopWatchingStock()
        {
            if (GameServices.Inventory != null) GameServices.Inventory.ItemChanged -= OnItemChanged;
        }
    }
}
