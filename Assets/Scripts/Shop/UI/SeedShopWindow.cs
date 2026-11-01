using System.Collections.Generic;
using DrugCrafter.Core;
using UnityEngine;

namespace DrugCrafter.Shop.UI
{
    /// <summary>Window where the player buys seeds with coins. New seeds unlock together with their recipes.</summary>
    public class SeedShopWindow : UIWindow
    {
        [SerializeField] private ShopConfig config;
        [SerializeField] private SeedCardView cardTemplate;

        private readonly List<SeedCardView> cards = new List<SeedCardView>();

        private void OnEnable()
        {
            if (cards.Count == 0) BuildCards();
            Subscribe(true);
            Refresh();
        }

        private void OnDisable()
        {
            Subscribe(false);
        }

        private void BuildCards()
        {
            cardTemplate.gameObject.SetActive(false);
            foreach (var seed in config.seeds)
            {
                if (seed == null) continue;
                var card = Instantiate(cardTemplate, cardTemplate.transform.parent);
                card.name = "SeedCard_" + seed.id;
                card.gameObject.SetActive(true);
                card.Bind(seed, Buy);
                cards.Add(card);
            }
        }

        private void Buy(SeedData seed)
        {
            var treasury = ShopContext.Treasury;
            if (treasury == null || !treasury.IsUnlocked(seed)) return;
            if (GameServices.Wallet.TrySpend(seed.buyPrice))
            {
                GameServices.Inventory.Add(seed.id, 1);
            }
        }

        private void Refresh()
        {
            var treasury = ShopContext.Treasury;
            var wallet = GameServices.Wallet;
            var inventory = GameServices.Inventory;
            if (treasury == null || wallet == null || inventory == null) return;

            foreach (var card in cards)
            {
                var seed = card.Seed;
                card.Refresh(treasury.IsUnlocked(seed), treasury.GetUnlockThreshold(seed.unlockedWith),
                    wallet.Coins >= seed.buyPrice, inventory.GetCount(seed.id));
            }
        }

        private void Subscribe(bool subscribe)
        {
            var wallet = GameServices.Wallet;
            var inventory = GameServices.Inventory;
            var treasury = ShopContext.Treasury;
            if (subscribe)
            {
                if (wallet != null) wallet.CoinsChanged += OnCoinsChanged;
                if (inventory != null) inventory.ItemChanged += OnItemChanged;
                if (treasury != null) treasury.Changed += Refresh;
            }
            else
            {
                if (wallet != null) wallet.CoinsChanged -= OnCoinsChanged;
                if (inventory != null) inventory.ItemChanged -= OnItemChanged;
                if (treasury != null) treasury.Changed -= Refresh;
            }
        }

        private void OnCoinsChanged(int coins) => Refresh();

        private void OnItemChanged(string itemId, int count) => Refresh();
    }
}
