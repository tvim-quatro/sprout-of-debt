using System.Collections;
using System.Collections.Generic;
using SproutOfDebt.Core;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Shop Counter loop (Figma: Counter page). While this location is active, visitors arrive one at a time:
    /// the dialogue box shows a greeting with the order, the order panel shows the facts and the two answers.
    /// Sell: the visitor is happy, coins pile up on the tray and fly to the HUD, a "+N coins" toast appears.
    /// "Sorry, we're out": the visitor is disappointed. Either way they say their line and leave.
    /// Customers come in waves with breaks in between, see <see cref="CustomerWaves"/>.
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
        [SerializeField] private OrderPanel orderPanel;
        [SerializeField] private CoinBurst coinBurst;
        [Tooltip("Delay before the first customer when the player comes to the counter")]
        [SerializeField] private float firstCustomerDelay = 1f;

        private Answer answer;
        private CustomerData lastCustomer;
        private PotionDefinition lastPotion;
        private int lastQuantity;
        private PotionDefinition requestedPotion;
        private int requestedQuantity;
        private int requestedTotalPrice;

        public event System.Action<PotionDefinition, int> PotionSold;

        private static bool IsTutorialRunning => GameServices.Tutorial != null && !GameServices.Tutorial.IsCompleted;

        private void Awake()
        {
            if (orderPanel == null || dialog == null || coinBurst == null || customerView == null)
            {
                Debug.LogError("[ShopCounter] The scene was built before the Figma Counter layout. " +
                               "Run Tools/Shop/Build Shop Counter Scene.", this);
                enabled = false;
                return;
            }
            orderPanel.SellButton.onClick.AddListener(() => answer = Answer.Sell);
            orderPanel.SorryButton.onClick.AddListener(() => answer = Answer.Sorry);
        }

        private void OnEnable()
        {
            dialog.Close();
            orderPanel.Hide();
            customerView.HideImmediate();
            coinBurst.ClearPile();
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
                if (!CustomerWaves.Current.CanServe())
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

            var customer = PickCustomer();
            PickOrder(potions);
            // Figma: "Pays = sell price × amount".
            requestedTotalPrice = requestedPotion.Price * requestedQuantity;

            yield return customerView.Enter(customer);

            // Greeting and order line together in the dialogue box; the order panel holds the facts.
            dialog.Open(customer);
            orderPanel.Show(requestedPotion, requestedQuantity, requestedTotalPrice);
            answer = Answer.None;
            RefreshStock();
            inventory.ItemChanged += OnItemChanged;
            yield return dialog.Say(customer.BuildArrivalLine(requestedPotion, requestedQuantity));

            yield return new WaitUntil(() => answer != Answer.None);
            StopWatchingStock();
            orderPanel.Hide();
            CustomerWaves.Current.OnCustomerServed();

            if (answer == Answer.Sell && inventory.TryRemove(requestedPotion.Id, requestedQuantity))
            {
                int earned = requestedTotalPrice;
                wallet.Add(earned);
                customerView.SetMood(CustomerMood.Happy);
                coinBurst.Play(earned);
                GameServices.Toasts?.Show($"+{earned} coins");
                PotionSold?.Invoke(requestedPotion, requestedQuantity);
                yield return dialog.Say(CustomerData.PickLine(customer.thanksLines, "Thank you!"));
            }
            else
            {
                customerView.SetMood(CustomerMood.Disappointed);
                yield return dialog.Say(CustomerData.PickLine(customer.disappointedLines, "Oh... maybe next time."));
            }

            yield return new WaitForSeconds(config.replyDuration);
            dialog.Close();
            yield return customerView.Leave();
            coinBurst.ClearPile();
        }

        /// <summary>Random order that differs from the previous one: another potion if possible, else another quantity.</summary>
        private void PickOrder(List<PotionDefinition> potions)
        {
            if (IsTutorialRunning)
            {
                // Onboarding: one Calming Potion, and the player has exactly one.
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
            orderPanel.SetStock(GameServices.Inventory.GetCount(requestedPotion.Id), requestedQuantity);
        }

        private void StopWatchingStock()
        {
            if (GameServices.Inventory != null) GameServices.Inventory.ItemChanged -= OnItemChanged;
        }
    }
}
