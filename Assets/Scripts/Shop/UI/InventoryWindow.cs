using System;
using System.Collections.Generic;
using SproutOfDebt.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    public enum InventoryTab
    {
        Seeds,
        Ingredients,
        Potions
    }

    /// <summary>
    /// Inventory window (Figma: Navigation / Windows / Inventory window):
    /// docked under the HUD inventory button (top right), with tabs for Seeds, Ingredients and Potions.
    /// Opens on the tab of the current location and updates live via GameServices.Inventory.
    /// </summary>
    public class InventoryWindow : UIWindow
    {
        private const int MinSlotCount = 12;
        private const float SeedsViewportHeight = 353f;
        private const float DefaultViewportHeight = 456f;

        private static readonly Color ActiveTextColor = new Color(0.24f, 0.15f, 0.09f);
        private static readonly Color InactiveTextColor = new Color(0.98f, 0.93f, 0.82f);

        [Header("Data")]
        [SerializeField] private InventoryDatabase database;

        [Header("Tabs")]
        [SerializeField] private Button seedsTabButton;
        [SerializeField] private Button ingredientsTabButton;
        [SerializeField] private Button potionsTabButton;
        [SerializeField] private Image seedsTabBg;
        [SerializeField] private Image ingredientsTabBg;
        [SerializeField] private Image potionsTabBg;
        [SerializeField] private TMP_Text seedsTabText;
        [SerializeField] private TMP_Text ingredientsTabText;
        [SerializeField] private TMP_Text potionsTabText;
        [SerializeField] private Sprite tabActiveSprite;
        [SerializeField] private Sprite tabInactiveSprite;

        [Header("Grid & Scrolling")]
        [SerializeField] private RectTransform slotsContainer;
        [SerializeField] private RectTransform scrollViewport;
        [SerializeField] private RectTransform scrollbarRect;
        [SerializeField] private InventorySlotView slotTemplate;

        [Header("Footer")]
        [SerializeField] private GameObject footer;
        [SerializeField] private Button buySeedsButton;
        [SerializeField] private UIWindow seedShopWindow;

        private InventoryTab currentTab = InventoryTab.Seeds;
        private readonly List<InventorySlotView> activeSlots = new List<InventorySlotView>();

        public InventoryTab CurrentTab => currentTab;

        protected override void Awake()
        {
            base.Awake();

            if (seedsTabButton != null)
                seedsTabButton.onClick.AddListener(() => SwitchTab(InventoryTab.Seeds));
            if (ingredientsTabButton != null)
                ingredientsTabButton.onClick.AddListener(() => SwitchTab(InventoryTab.Ingredients));
            if (potionsTabButton != null)
                potionsTabButton.onClick.AddListener(() => SwitchTab(InventoryTab.Potions));

            if (buySeedsButton != null)
                buySeedsButton.onClick.AddListener(OnBuySeedsClicked);

            if (slotTemplate != null)
                slotTemplate.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            currentTab = GetDefaultTabForActiveScene();
            SwitchTab(currentTab);

            if (GameServices.Inventory != null)
            {
                GameServices.Inventory.ItemChanged += OnItemChanged;
            }
        }

        private void OnDisable()
        {
            if (GameServices.Inventory != null)
            {
                GameServices.Inventory.ItemChanged -= OnItemChanged;
            }
        }

        public void SwitchTab(InventoryTab tab)
        {
            currentTab = tab;
            UpdateTabVisuals();
            UpdateLayoutForTab();
            RefreshSlots();
        }

        private void OnItemChanged(string itemId, int newCount)
        {
            RefreshSlots();
        }

        private void OnBuySeedsClicked()
        {
            if (windowManager != null && seedShopWindow != null)
            {
                windowManager.Open(seedShopWindow);
            }
        }

        private static InventoryTab GetDefaultTabForActiveScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.IndexOf("Lab", StringComparison.OrdinalIgnoreCase) >= 0)
                return InventoryTab.Ingredients;
            if (sceneName.IndexOf("Counter", StringComparison.OrdinalIgnoreCase) >= 0 ||
                sceneName.IndexOf("Shop", StringComparison.OrdinalIgnoreCase) >= 0)
                return InventoryTab.Potions;

            return InventoryTab.Seeds;
        }

        private void UpdateTabVisuals()
        {
            SetTabState(seedsTabBg, seedsTabText, currentTab == InventoryTab.Seeds);
            SetTabState(ingredientsTabBg, ingredientsTabText, currentTab == InventoryTab.Ingredients);
            SetTabState(potionsTabBg, potionsTabText, currentTab == InventoryTab.Potions);
        }

        private void SetTabState(Image bg, TMP_Text text, bool active)
        {
            if (bg != null)
                bg.sprite = active ? tabActiveSprite : tabInactiveSprite;
            if (text != null)
                text.color = active ? ActiveTextColor : InactiveTextColor;
        }

        private void UpdateLayoutForTab()
        {
            bool isSeeds = currentTab == InventoryTab.Seeds;
            if (footer != null)
                footer.SetActive(isSeeds);

            float targetHeight = isSeeds ? SeedsViewportHeight : DefaultViewportHeight;
            if (scrollViewport != null)
            {
                var size = scrollViewport.sizeDelta;
                size.y = targetHeight;
                scrollViewport.sizeDelta = size;
            }
            if (scrollbarRect != null)
            {
                var size = scrollbarRect.sizeDelta;
                size.y = targetHeight;
                scrollbarRect.sizeDelta = size;
            }
        }

        private void RefreshSlots()
        {
            if (database == null || slotsContainer == null || slotTemplate == null) return;

            var items = CollectCurrentTabItems();
            int totalSlots = Mathf.Max(items.Count, MinSlotCount);

            while (activeSlots.Count < totalSlots)
            {
                var slot = Instantiate(slotTemplate, slotsContainer);
                activeSlots.Add(slot);
            }

            for (int i = 0; i < activeSlots.Count; i++)
            {
                var slot = activeSlots[i];
                if (i < items.Count)
                {
                    slot.gameObject.SetActive(true);
                    slot.SetItem(items[i].icon, items[i].count);
                }
                else if (i < totalSlots)
                {
                    slot.gameObject.SetActive(true);
                    slot.SetEmpty();
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }

        private List<(Sprite icon, int count)> CollectCurrentTabItems()
        {
            var result = new List<(Sprite icon, int count)>();
            var inventory = GameServices.Inventory;
            if (inventory == null) return result;

            switch (currentTab)
            {
                case InventoryTab.Seeds:
                    foreach (var seed in database.Seeds)
                    {
                        if (seed == null) continue;
                        int count = inventory.GetCount(seed.id);
                        if (count > 0) result.Add((seed.icon, count));
                    }
                    break;
                case InventoryTab.Ingredients:
                    foreach (var ingr in database.Ingredients)
                    {
                        if (ingr == null) continue;
                        int count = inventory.GetCount(ingr.id);
                        if (count > 0) result.Add((ingr.icon, count));
                    }
                    break;
                case InventoryTab.Potions:
                    foreach (var potion in database.Potions)
                    {
                        if (potion == null) continue;
                        int count = inventory.GetCount(potion.Id);
                        if (count > 0) result.Add((potion.Icon, count));
                    }
                    break;
            }

            return result;
        }
    }
}
